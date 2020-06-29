#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Messaging;
using Gibraltar.Monitor.Net;
using Gibraltar.Monitor.Windows;
using Gibraltar.Monitor.Windows.Internal;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor
{
    /// <summary>
    /// The central listener that manages the configuration of the individual listeners
    /// </summary>
    public static class Listener
    {
        private static readonly object m_MonitorThreadLock = new object();
        private static readonly object m_ManagerThreadLock = new object();
        private static readonly object m_ListenerLock = new object();
        private static readonly object m_ExceptionListenerLock = new object(); //has its own because many things contend, including other listeners.
        private static readonly object m_ConfigLock = new object();

        private static AgentConfiguration m_AgentConfiguration; //the active Agent configuration //LOCKED BY CONFIGLOCK
        private static ListenerConfiguration m_Configuration; //the active listener configuration //LOCKED BY CONFIGLOCK
        private static bool m_PendingConfigChange; //LOCKED BY CONFIGLOCK
        private static bool m_Initialized; //LOCKED BY CONFIGLOCK; (update only)
        private static Dictionary<Keys, MethodInvoker> m_HotKeyMap = new Dictionary<Keys, MethodInvoker>();

        private static Thread m_MonitorThread; //LOCKED BY MONITORTHREADLOCK
        private static Thread m_ManagerThread; //LOCKED BY MANAGERTHREADLOCK

        //our various listeners we're controlling
        private static bool m_ConsoleListenerRegistered; //LOCKED BY LISTENERLOCK
        private static LogListener m_TraceListener; //LOCKED BY LISTENERLOCK
        private static ExceptionListener m_ExceptionListener; //LOCKED BY EXCEPTIONLISTENERLOCK
        private static PerformanceMonitor m_PerformanceMonitor; //LOCKED BY LISTENERLOCK
        private static ProcessMonitor m_ProcessMonitor; //LOCKED BY LISTENERLOCK
        private static CLRListener m_CLRListener; //LOCKED BY LISTENERLOCK

        private static MetricSampleInterval m_SamplingInterval = MetricSampleInterval.Minute;
        private static DateTimeOffset m_PollingStarted;
        private static bool m_EventsInitialized;
        private static bool m_SuppressTraceInitialize;
        private static bool m_SuppressAlerts;

        [ThreadStatic] private static bool t_ThreadExceptionSubscribed;

        // UI resources we manage
        private static ManagerApplicationContext m_AppContext; // LOCKED BY MANAGERTHREADLOCK
        private static LiveLogViewer m_LiveViewer; // LOCKED BY MANAGERTHREADLOCK
        //private static LiveViewerForm m_LiveViewerForm; // LOCKED BY MANAGERTHREADLOCK
        private static event MessageFilterEventHandler g_MessageFilterEvent;
        private static readonly object g_MessageEventLock = new object(); // Locks add/remove of Message event subscriptions.

        private static bool m_Exiting; // LOCKED BY MANAGERTHREADLOCK ?
        private static bool m_NewHotKeyFilter; // LOCKED BY MANAGERTHREADLOCK
        private static bool m_ShowLiveViewerForm; // LOCKED BY MANAGERTHREADLOCK
        private static bool m_ShowPackagerForm; // LOCKED BY MANAGERTHREADLOCK
        private static bool m_ShowAlertForm; // LOCKED BY MANAGERTHREADLOCK
        volatile private static MessageFilter m_KeyFilter; // LOCKED BY MANAGERTHREADLOCK for creation (reads safe by volatile)
        private static Queue<ExceptionDisplayRequest> m_AlertQueue = new Queue<ExceptionDisplayRequest>(); // LOCKED BY MANAGERTHREADLOCK

        static Listener()
        {
            //create the background thread we need so we can respond to requests.
            CreateMonitorThread();
        }

        /// <summary>
        /// Apply the provided listener configuration
        /// </summary>
        /// <param name="agentConfiguration"></param>
        /// <param name="suppressTraceInitialize">True to prevent any interaction with the Trace subsystem</param>
        /// <param name="async"></param>
        /// <remarks>If calling initialization from a path that may have started with the trace listener,
        /// you must set suppressTraceInitialize to true to guarantee that the application will not deadlock
        /// or throw an unexpected exception.</remarks>
        public static void Initialize(AgentConfiguration agentConfiguration, bool suppressTraceInitialize, bool async)
        {
            ListenerConfiguration listenerConfiguration = agentConfiguration.Listener;
            //get a configuration lock so we can update the configuration
            lock(m_ConfigLock)
            {
                //and store the configuration; it's processed by the background thread.
                m_SuppressTraceInitialize = suppressTraceInitialize; //order is important since as soon as we touch the configuration the background thread could go.
                m_AgentConfiguration = agentConfiguration; // Set the top config before the local Listener config.
                m_Configuration = listenerConfiguration; // Monitor thread looks for this to be non-null before proceeding.
                m_PendingConfigChange = true;

                //wait for our events to initialize always on our background thread
                while (m_EventsInitialized == false)
                {
                    System.Threading.Monitor.Wait(m_ConfigLock, 16);
                }

                //and if we're doing a synchronous init then we even wait for the polled listeners.
                while ((async == false) && (m_PendingConfigChange))
                {
                    System.Threading.Monitor.Wait(m_ConfigLock, 16);
                }

                System.Threading.Monitor.PulseAll(m_ConfigLock);
            }

            if (Log.SessionSummary.AgentAppType == ApplicationType.Windows)
            {
                // Only useful if we're in a windows client (either Forms or WPF)...  not Console, Service, Web app...
                ConfigureHotKeyFilter(agentConfiguration); // Create or modify the hotkey filter configuration.
            }
        }

        /// <summary>
        /// Indicates if the listeners have been initialized the first time yet.
        /// </summary>
        public static bool Initialized { get { return m_Initialized; } }


        /// <summary>
        /// Signal the Messenger that the logging session is ending (typically on application exit).
        /// </summary>
        public static void OnManagerExit()
        {
            lock (m_ManagerThreadLock)
            {
                m_Exiting = true; // So the Manager thread knows to exit rather than loop again.

                // If any of the Manager's UI forms is open, we want to make sure they close.
                // If we get here and the form still exists, Log.EndSession() was called.
                if (m_AppContext != null)
                    m_AppContext.CloseAll(); // This will invoke the Close() calls needed to the right threads for us.

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// The requested interval between samples.
        /// </summary>
        /// <remarks>Determines how frequently the performance counters registered with this monitor are sampled.
        /// Changes are applied immediately and without resetting the timer.  For example, if the previous sampling 
        /// interval was every hour and it is moved up to every minute then the fractional minute since the last sample
        /// will still be counted, and if it exceeds the new interval the values will be immediately sampled.</remarks>
        public static MetricSampleInterval SamplingInterval
        {
            get
            {
                //I don't think we have to do any locking on a simple read, if we get an out of date value, so be it
                return m_SamplingInterval;
            }
            set
            {
                //this is the only place we update this, so we aren't getting a lock.  
                m_SamplingInterval = value;
            }
        }

        /// <summary>
        /// Indicates that instead of displaying new alerts, they should just handled as if the user selected the default.
        /// </summary>
        public static bool SuppressAlerts
        {
            get
            {
                lock (m_ManagerThreadLock)
                {
                    System.Threading.Monitor.PulseAll(m_ManagerThreadLock);

                    return m_SuppressAlerts;
                }
            }
            set
            {
                lock (m_ManagerThreadLock)
                {
                    m_SuppressAlerts = value;

                    System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
                }
            }
        }

        /// <summary>
        /// Handler type for a message filter event.
        /// </summary>
        /// <param name="sender">The sender of this event (the LiveLogViewer control instance or null for the main Gibraltar Live Viewer)</param>
        /// <param name="e">The Message Filter Event Args.</param>
        public delegate void MessageFilterEventHandler(object sender, MessageFilterEventArgs e);

        /// <summary>
        /// Raised whenever the Gibraltar Live Viewer receives a new log message, allowing the client to block it from being
        /// included for display to users.
        /// </summary>
        /// <remarks><para>Each LiveLogViewer instance filters independently with its own event.  However, the main
        /// Gibraltar Live Viewer itself (accessible via hotkey unless disabled) is created within the Agent and its control
        /// object is not accessible to client code.  This static event on the Log class therefore allows clients to bind to
        /// the filter on the main Gibraltar Live Viewer in order to filter the messages it displays to users (eg. to block
        /// sensitive internal data).</para>
        /// <para>The Message property of the event args provides the log message in consideration, and the Cancel property
        /// allows the message to be displayed (false, the default) or blocked (true).  The sender parameter of the event will
        /// be null to signify the main Gibraltar Live Viewer rather than a client-instantiated LiveLogViewer.</para></remarks>
        public static event MessageFilterEventHandler LiveViewerMessageFilter
        {
            add
            {
                if (value == null)
                    return;

                lock (g_MessageEventLock)
                {
                    if (g_MessageFilterEvent == null && m_LiveViewer != null)
                    {
                        m_LiveViewer.MessageFilter += LiveViewer_MessageFilter;
                    }

                    g_MessageFilterEvent += value;
                }
            }
            remove
            {
                if (value == null)
                    return;

                lock (g_MessageEventLock)
                {
                    if (g_MessageFilterEvent == null)
                        return; // Already empty, no subscriptions to remove.

                    g_MessageFilterEvent -= value;

                    if (g_MessageFilterEvent == null && m_LiveViewer != null)
                    {
                        m_LiveViewer.MessageFilter -= LiveViewer_MessageFilter;
                    }
                }
            }
        }

        private static void LiveViewer_MessageFilter(object sender, MessageFilterEventArgs e)
        {
            MessageFilterEventHandler eventHandler = g_MessageFilterEvent;

            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        /// <summary>
        /// Check if a given Form (or WPF Window) is a known Gibraltar Agent UI window/form class.
        /// </summary>
        /// <param name="form">The window/form object to check.</param>
        /// <returns>True if the type of the object is recognized as a Gibraltar Agent UI window/form class.</returns>
        public static bool GibraltarWindowForm(object form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            string typeName = form.GetType().FullName;
            if (typeName.StartsWith("Gibraltar.Monitor.Windows."))
            {
                typeName = typeName.Substring(26); // Strip off that known prefix.
                if (typeName == "Internal.LiveViewerForm" || typeName == "UIPackagerDialog")
                    return true; // It's one of our UI forms!
            }

            return false; // Otherwise, it's not one of ours.
        }

        /// <summary>
        /// Check if client has a windows Form active yet.
        /// </summary>
        /// <returns>The first client Form found, or null.</returns>
        public static Form ClientFormsActive()
        {
            Form firstForm = null;
            try
            {
                FormCollection formsCollection = Application.OpenForms;
                if (formsCollection != null)
                {
                    foreach (Form form in formsCollection)
                    {
                        if (form == null || form.IsHandleCreated == false || GibraltarWindowForm(form))
                            continue;

                        firstForm = form; // If we get here, it must be a client form...
                        break; // ...so we can stop looking.
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    throw;

                Log.DebugBreak(); // Catch this in the debugger.
            }

            return firstForm;
        }

        /// <summary>
        /// Check if client has a WPF Window active yet.
        /// </summary>
        /// <returns>Boolean for now, some other type once this is really implemented.</returns>
        public static bool ClientWindowsActive()
        {
            return false; // We don't yet know how to check, so we'll assume it isn't.
        }

        #region Internal Properties and Methods

        /// <summary>
        /// Performs initialization needed on a per-thread basis when the thread is first discovered.
        /// </summary>
        /// <param name="appType">The ApplicationType of the client application.</param>
        /// <param name="ignoreConfig">Overrides CatchApplicationException config setting if this is true.</param>
        internal static void InitializeThreadListening(ApplicationType appType, bool ignoreConfig)
        {
            // Exception listening initialization per-thread is only meaningful for Windows apps.
            // Only UI threads issue Application.ThreadException events (and Application.ThreadExit events).
            // And hotkeys are only usable in a Windows app.

            //NOTE:  do NOT access m_ListenerLock here or you WILL cause deadlocks.

            if (appType != ApplicationType.Windows) // Only applies to Windows apps.
                return;

            if (t_ThreadExceptionSubscribed == false)
            {
                lock (m_ExceptionListenerLock) //the exception listener has its own lock
                {
                    if (m_ExceptionListener != null)
                        m_ExceptionListener.RegisterThreadEvents(ignoreConfig);

                    System.Threading.Monitor.PulseAll(m_ExceptionListenerLock);
                }
            }

            ActivateHotKeyFilter();
        }

        /// <summary>
        /// The action run on any thread to show the alert dialog form and display an ExceptionDisplayRequest.
        /// </summary>
        /// <param name="newRequest">The ExceptionDisplayRequest to report.</param>
        /// <remarks>This should theoretically work from any thread.</remarks>
        internal static void ShowAlertDialog(ExceptionDisplayRequest newRequest)
        {
            if (newRequest == null)
                return;

            lock (m_ManagerThreadLock)
            {
                //see if we are supposed to be suppressing errors now...
                if (m_SuppressAlerts || Log.IsMonoRuntime)
                {
                    //set a default result. Without a result, things can go bad.
                    newRequest.Result = DialogResult.OK;
                    return;
                }

                m_AlertQueue.Enqueue(newRequest);

                m_ShowAlertForm = true; // Signal that it needs to be shown.
                if (m_ManagerThread == null)
                {
                    // First time activation, we need to launch the thread first.
                    // It'll create the application context, which we then need to tell to show the form.
                    CreateManagerThread(); // Create the Manager thread and get it ready to show the newly created form.
                }
                else
                {
                    // Note: This is a hack to make sure the form gets raised to the front upon first creation.

                    System.Threading.Monitor.PulseAll(m_ManagerThreadLock); // Pulse to alert that we set show-form to true.
                    if (m_AppContext == null && m_Exiting == false) // Don't loop, we could deadlock?
                    {
                        // There could be a race condition in which the flag gets cleared and app context exits and leaves
                        // us waiting forever (blocking a UI thread!), so use a timeout and give up after 2 seconds.
                        System.Threading.Monitor.Wait(m_ManagerThreadLock, 2000);
                    }

                    if (m_AppContext != null && m_Exiting == false)
                    {
                        // Now move the existing form to the foreground.  We can BeginInvoke to do it.
                        // This only works once App Context exists and is running in the message loop.
                        m_AppContext.DoInvoke(RaiseAlertDialog);
                    }
                    // Otherwise, we couldn't do it, but the queue is not empty, so it should try again.
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// The action run on any thread to show the viewer form.
        /// </summary>
        /// <remarks>This should theoretically work from any thread.</remarks>
        internal static void ShowViewerForm()
        {
            if (Log.IsMonoRuntime)
            {
                Log.Write(LogMessageSeverity.Information, "Gibraltar.Agent.Live Viewer", "Gibraltar Live Viewer is not supported on Mono",
                          "The Gibraltar Live Viewer is designed to run in a separate UI thread, but the Mono runtime does not "+
                          "support multiple UI threads as of version 2.6, so the GLV is disabled when running under Mono.\r\n"+
                          "You can achieve Live Viewer functionality under Mono by placing the Gibraltar.Agent.Windows.LiveLogViewer "+
                          "control on one of your own forms and displaying it in your main UI thread.");
                return;
            }

            lock (m_ManagerThreadLock)
            {
                m_ShowLiveViewerForm = true; // Signal that it needs to be shown.
                if (m_ManagerThread == null)
                {
                    // First time activation, we need to launch the thread first.
                    // It'll create the application context, which we then need to tell to show the form.
                    CreateManagerThread(); // Create the Manager thread and get it ready to show the newly created form.
                }
                else
                {
                    // Note: This is a hack to make sure the form gets raised to the front upon first creation.

                    System.Threading.Monitor.PulseAll(m_ManagerThreadLock); // Pulse to alert that we set show-form to true.
                    while (m_AppContext == null && m_Exiting == false)
                    {
                        // There could be a race condition in which the flag gets cleared and app context exits and leaves
                        // us waiting forever (blocking a UI thread!), so use a timeout and give up after 2 seconds.
                        System.Threading.Monitor.Wait(m_ManagerThreadLock, 2000);
                    }

                    if (m_AppContext != null && m_Exiting == false)
                    {
                        // Now move the existing form to the foreground.  We can BeginInvoke to do it.
                        // This only works once App Context exists and is running in the message loop.
                        m_AppContext.DoInvoke(RaiseViewerForm);
                    }
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// The action run on any thread to clear the viewer form.
        /// </summary>
        /// <remarks>This should theoretically work from any thread.</remarks>
        internal static void ClearViewerForm()
        {
            //BUG:  This does nothing yet.
        }

        internal static void ThreadExitedMessageLoop()
        {
            ThreadExceptionUnsubscribed();
            DeactivateHotKeyFilter();
        }

        internal static void ThreadExceptionUnsubscribed()
        {
            t_ThreadExceptionSubscribed = false;
        }

        internal static void ThreadExceptionSubscribed()
        {
            t_ThreadExceptionSubscribed = true;
        }

        #endregion

        #region Private Properties and Methods

        private static bool IsHotKeyConfigDisabled(string hotKeyConfig)
        {
            if (string.IsNullOrEmpty(hotKeyConfig))
                return true;

            string hotKeyString = hotKeyConfig.ToUpperInvariant();
            if (hotKeyString == "DISABLE" || hotKeyString == "DISABLED" || hotKeyString == "DIS" || hotKeyString == "NONE")
                return true;

            // Otherwise, they set a hotkey config to be parsed.
            return false;
        }

        /// <summary>
        /// Create or modify the hotkey mapping based on the new active configuration.
        /// </summary>
        /// <param name="agentConfiguration">The top configuration object for the entire Agent.</param>
        private static void ConfigureHotKeyFilter(AgentConfiguration agentConfiguration)
        {
            ViewerMessengerConfiguration viewerConfiguration = agentConfiguration.Viewer;
            PackagerConfiguration packagerConfiguration = agentConfiguration.Packager;

            lock (m_ManagerThreadLock)
            {
                string viewerHotKeyString = viewerConfiguration.HotKey;
                Keys viewerHotKeyCode;
                // Mono doesn't support multiple UI threads, so disable the GLV hotkey until we can launch it on the main UI thread.
                if (IsHotKeyConfigDisabled(viewerHotKeyString) == false && Log.IsMonoRuntime == false)
                    viewerHotKeyCode = MessageFilter.ParseHotKeyString(viewerHotKeyString);
                else
                    viewerHotKeyCode = Keys.None;

                string packagerHotKeyString = packagerConfiguration.HotKey;
                Keys packagerHotKeyCode;
                if (IsHotKeyConfigDisabled(packagerHotKeyString) == false)
                    packagerHotKeyCode = MessageFilter.ParseHotKeyString(packagerHotKeyString);
                else
                {
                    packagerHotKeyCode = Keys.None;
                }

                if (m_KeyFilter != null)
                    m_KeyFilter.Disable = true; // Disable existing KeyFilter while we change the HotKeyMap dictionary.

                m_HotKeyMap.Clear(); // Clear any existing hotkey mapping.

                if (viewerHotKeyCode == packagerHotKeyCode)
                {
                    // Both are set to the same key or both are disabled.
                    if (viewerHotKeyCode != Keys.None)
                        m_HotKeyMap[viewerHotKeyCode] = OnShowViewerAndPackagerHotKeyPressed;

                    // If both disabled, just leave the hotkey map clear.
                }
                else
                {
                    // Hotkeys are set to different key codes (normal case), but one or the other may be disabled.
                    if (viewerHotKeyCode != Keys.None)
                        m_HotKeyMap[viewerHotKeyCode] = OnShowViewerHotKeyPressed;

                    if (packagerHotKeyCode != Keys.None)
                        m_HotKeyMap[packagerHotKeyCode] = OnShowPackagerHotKeyPressed;
                }

                // HotKeyMap changes are done at this point.  We can reenable existing KeyFilter.
                if (m_KeyFilter != null)
                {
                    m_KeyFilter.Disable = false; // Enable new or existing KeyFilter now that changes are done.
                }
                else if (m_HotKeyMap.Count > 0) // else it's null, so check if we need to make one.
                {
                    m_KeyFilter = new MessageFilter(m_HotKeyMap);
                    m_NewHotKeyFilter = true; // Signal the Manager thread to check and initialize the created key filter.
                    InitializeHotKeyFilter(); // Start checking for client active to activate key filter on client UI thread.
                    if (m_AppContext != null) // Manager thread may be in the message loop and might not see the signal flag.
                        m_AppContext.DoInvoke(ActivateHotKeyFilter); // Make sure Manager thread activates the key filter, too.

                    //CreateManagerThread(); // Kick off our Agent UI manager thread, if it isn't already running.
                }
                // Otherwise, it's already set up or waiting on client activity to complete activating the filter.

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// Check if client forms (WinForms or WPF) are active yet and manage event subscription for continued checking.
        /// </summary>
        /// <param name="subscribed">Pass true if already subscribed to the event (eg. calling from event handler).</param>
        private static void CheckClientActive(bool subscribed)
        {
            if (m_KeyFilter == null)
                return;

            Form form = ClientFormsActive();
            if (form != null)
            {
                Publisher.MessageDispatching -= Publisher_MessageDispatching; // Unsubscribe, we no longer need it.

                form.BeginInvoke(new MethodInvoker(ActivateHotKeyFilter)); // Activate it on the right thread.
            }
            else if (ClientWindowsActive())
            {
                Publisher.MessageDispatching -= Publisher_MessageDispatching; // Unsubscribe, we no longer need it.

                // ToDo: Need to handle WPF case.
            }
            else if (subscribed == false)
            {
                // No client active yet, we need to subscribe to keep checking until it is.
                Publisher.MessageDispatching += Publisher_MessageDispatching;
            }
        }

        /// <summary>
        /// Activate a keystroke filter (thread-specific) on the current thread.
        /// </summary>
        private static void ActivateHotKeyFilter()
        {
            MessageFilter keyFilter = m_KeyFilter; // We can do this without the lock because it never goes away once created.
            if (keyFilter != null)
                keyFilter.Activate(); // Won't double-activate on the same thread, so it's safe to just call it.
        }

        /// <summary>
        /// Deactivate the keystroke filter on the current thread, such as when about to ExitThread().
        /// </summary>
        private static void DeactivateHotKeyFilter()
        {
            MessageFilter keyFilter = m_KeyFilter; // We can do this without the lock because it never goes away once created.
            if (keyFilter != null)
                keyFilter.Deactivate(); // Won't double-deactivate on the same thread, so it's safe to just call it.
        }

        /// <summary>
        /// Initialize a newly created hotkey filter for the client UI thread, can be called from any thread.
        /// </summary>
        private static void InitializeHotKeyFilter()
        {
            lock (m_ManagerThreadLock)
            {
                if (m_NewHotKeyFilter)
                {
                    m_NewHotKeyFilter = false; // Clear the signal, we should not do this more than once.
                    CheckClientActive(false); // Not yet subscribed, this is a one-time initialization.
                }
                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// Event handler for when the viewer hot key is pressed.
        /// </summary>
        private static void OnShowViewerHotKeyPressed()
        {
            // ToDo: This should queue a command to us to show the form, so we don't hold up the keyboard handler.
            // But until we get a command queue in place, we'll just hope this is quick enough (while developing).
            ShowViewerForm();
        }

        /// <summary>
        /// Event handler for when the packager hot key is pressed.
        /// </summary>
        private static void OnShowPackagerHotKeyPressed()
        {
            // ToDo: This should queue a command to us to show the form, so we don't hold up the keyboard handler.
            // But until we get a command queue in place, we'll just hope this is quick enough (while developing).
            Form activeForm = Form.ActiveForm;
            Form clientForm = null;

            // We should already be executing on the same thread as the ActiveForm,
            // but we want this asynchronous so we don't block the message filter! (would probably not be good)

            if (activeForm == null || GibraltarWindowForm(activeForm))
            {
                // We prefer to launch the packager on a client UI thread, so look for one of their forms.
                clientForm = ClientFormsActive();
            }

            if (clientForm != null) // If we looked and found a client form, use that for the invoke.
            {
                clientForm.BeginInvoke(new MethodInvokerWithForm(ShowPackagerDialog), clientForm);
            }
            else if (activeForm != null) // Otherwise, go with the active form, unless it somehow doesn't exist!
            {
                activeForm.BeginInvoke(new MethodInvoker(ShowPackagerDialog));
            }
            // Otherwise, we should probably give up.
        }

        /// <summary>
        /// Event handler for when the dual hot key for viewer and packager is pressed.
        /// </summary>
        private static void OnShowViewerAndPackagerHotKeyPressed()
        {
            // ToDo: This should queue a command to us to show the forms, so we don't hold up the keyboard handler.
            // But until we get a command queue in place, we'll just hope this is quick enough (while developing).
            ShowViewerForm();
            OnShowPackagerHotKeyPressed(); // And redirect this call as if it was the Packager hotkey.
        }

        /// <summary>
        /// Check whether the Agent Manager UI thread already exists, and create it if it does not.
        /// </summary>
        private static void CreateManagerThread()
        {
            lock (m_ManagerThreadLock)
            {
                if (m_ManagerThread != null)
                    return; // Only make one of them at a time.

                m_Exiting = false;
                m_ManagerThread = new Thread(ManagerThreadMain);
                m_ManagerThread.IsBackground = true;
                m_ManagerThread.Name = "Loupe Agent Manager"; //name our thread so we can isolate it out of metrics and such
                m_ManagerThread.TrySetApartmentState(ApartmentState.STA); // UI thread so must be STA.
                m_ManagerThread.Start();

                //Publisher.MessageDispatching += Publisher_MessageDispatching; // Subscribe to event to await active winform client.

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        private static void ManagerThreadMain()
        {
            bool exiting = false;
            ManagerApplicationContext appContext = null;

            Publisher.ThreadMustNotBlock();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException); // Thread-specific setting?

            // We need to check if the client windows are up yet (we wait for them to initialize first).
            lock (m_ManagerThreadLock)
            {
                // Create the viewer outside the MessageEventLock, it's too much to be doing in that lock.
                try
                {
                    LiveLogViewer liveViewer = new LiveLogViewer(true, true); // Create a persistent LiveLogViewer control which we'll retain.
                    liveViewer.GetPriorMessages = true;
                    lock(g_MessageEventLock)
                    {
                        m_LiveViewer = liveViewer; // Only set the (static) member variable inside the lock; subscribe checks it!
                        if (g_MessageFilterEvent != null) // We have subscribers! Subscribe to the LiveViewer's MessageFilter event.
                            m_LiveViewer.MessageFilter += LiveViewer_MessageFilter;
                    }
#if DEBUG
                    Log.Trace("Gibraltar Agent Manager thread launched.");
#endif

                }
                catch (Exception ex)
                {
                    m_Exiting = true;
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent.Manager", "Gibraltar Agent Manager thread exited",
                        "The thread managing the Gibraltar Live Viewer and Gibraltar Error Manager caught an unexpected exception and must exit.");
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }

            while (exiting == false)
            {
                try
                {
                    lock (m_ManagerThreadLock)
                    {
                        // Until we are told to show it or exit...
                        while (m_Exiting == false)
                        {
                            // Now that we've checked for a new filter to activate, see if we need to break out to show a form.
                            if (m_ShowLiveViewerForm || m_ShowPackagerForm || m_ShowAlertForm || m_AlertQueue.Count > 0)
                                break;

                            System.Threading.Monitor.Wait(m_ManagerThreadLock); // ...otherwise, we just wait around.
                        }

                        // Cache our exiting for the benefit of our outer while loop outside the lock.
                        exiting = m_Exiting;

                        // We only get here if showing something or exiting, and exiting overrides show, so just check exiting...
                        // ...and if it's false, a show flag must be true or we would have gone around the wait loop again.
                        if (exiting == false) // Don't make a new one if we're exiting.
                        {
                            appContext = new ManagerApplicationContext(m_LiveViewer); // Use our retained LiveLogViewer control.
                            if (m_ShowLiveViewerForm)
                            {
                                m_ShowLiveViewerForm = false; // We're about to show it (or exit), so clear the signal.
                                appContext.ShowLiveViewer();
                            }
                            if (m_ShowPackagerForm)
                            {
                                m_ShowPackagerForm = false; // We're about to show it (or exit), so clear the signal.
                                appContext.ShowPackager();
                            }
                            if (m_ShowAlertForm || m_AlertQueue.Count > 0)
                            {
                                m_ShowAlertForm = false; // We're about to show it (or exit), so clear the signal.
                                ExceptionDisplayRequest[] requestArray = m_AlertQueue.ToArray();
                                m_AlertQueue.Clear();
                                appContext.ShowAlertForm(requestArray);
                            }
                            if (m_KeyFilter != null)
                                ActivateHotKeyFilter(); // Activate the filter on the Manager UI thread.
                        }
                        m_AppContext = appContext; // Save the app context, so hotkey prods can reraise stuff while we're blocked.
                        System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
                    }

                    if (appContext != null) // It's null if we're exiting.
                    {
#if DEBUG
                        Log.Trace("Gibraltar Agent Manager entering message loop");
#endif
                        lock (m_ListenerLock)
                        {
                            // We have to resubscribe to ThreadException events each time we reenter the message loop;
                            // they get cleared upon exit from Application.Run().
                            if (m_ExceptionListener != null)
                                m_ExceptionListener.RegisterThreadEvents(true);

                            System.Threading.Monitor.PulseAll(m_ListenerLock);
                        }

                        // This has to be called outside the locks, or it would never get released and could deadlock things.
                        Application.Run(appContext); // Block until all forms close.

#if DEBUG
                        Log.Trace("Gibraltar Agent Manager exited message loop");
#endif
                    }
                }
                catch (ThreadAbortException)
                {
                    m_Exiting = true; // Exit gracefully, in theory.
                }
                catch (Exception ex)
                {
                    m_Exiting = true;
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent.Manager", "Gibraltar Agent Manager thread exited",
                        "The thread managing the Gibraltar Live Viewer and Gibraltar Error Manager caught an unexpected exception and must exit.");
#if DEBUG
                    throw; // In debug builds throw it unhandled so we definitely find out about it. ??
#endif
                }
                finally
                {
                    lock (m_ManagerThreadLock)
                    {
                        if (appContext != null)
                        {
                            try
                            {
                                appContext.Dispose();
                            }
                            catch (Exception ex)
                            {
                                // Just swallow any errors in trying to dispose appContext here.
                                if (ex is ThreadAbortException)
                                    m_Exiting = true; // Exit gracefully.  We need to end the thread.
                            }
                        }

                        m_AppContext = null; // Clear the record of it.
                        appContext = null;
                        exiting = m_Exiting; // Cache this in case it changed while form was open, to exit the loop.
                        System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
                    }
                }
            }

#if DEBUG
            Log.Trace("Gibraltar Agent Manager thread exiting");
#endif

            // Clean up our persistent LiveLogViewer control and exit the thread.
            lock (m_ManagerThreadLock)
            {
                if (m_LiveViewer != null)
                {
                    try
                    {
                        // We're disposing it outside the message loop which could be prone to problems, so be careful.
                        m_LiveViewer.Dispose(); // Clean up our persistent LiveLogViewer control.
#if DEBUG
                    Log.Write(LogMessageSeverity.Verbose, "Gibraltar.Agent.Live Viewer", "Live Viewer control disposed successfully", null);
#endif
                    }
                    catch (Exception ex)
                    {
#if !DEBUG
                        if (Log.SilentMode == false) // Log the exception unless Release mode in a client process.
#endif
                        {
                            Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent.Live Viewer", "Error disposing Live Viewer control",
                                      "An exception was thrown when disposing the Live Viewer control as the Manager thread exits.");

                            //if (Debugger.IsAttached)
                            //    Debugger.Break();
                        }
                    }

                    // These actions should be safe, at least inside the lock like we are.
                    lock(g_MessageEventLock)
                    {
                        m_LiveViewer.MessageFilter -= LiveViewer_MessageFilter; // Probably unnecessary, but for completeness.
                        // We're inside the event subscription lock for this because the subscription checks whether LiveViewer is null.
                        m_LiveViewer = null; // No longer exists, clear it out.
                    }
                }

                m_ManagerThread = null; // About to exit the thread.

                while (m_AlertQueue.Count > 0)
                {
                    ExceptionDisplayRequest request = m_AlertQueue.Dequeue();
                    // This does a lock and pulse on the request automatically.
                    request.Result = DialogResult.No; // Not presented to user due to forced termination.
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock);
            }
        }

        /// <summary>
        /// The action on the Manager thread to raise an existing viewer form.
        /// </summary>
        /// <remarks>You must BeginInvoke from the existing form/control to this method.</remarks>
        private static void RaiseViewerForm()
        {
            lock (m_ManagerThreadLock)
            {
                m_ShowLiveViewerForm = true; // Signal that it needs to be shown, in case the app context is gone?

                // Get the app context now that we're in a lock and righteous.
                if (m_AppContext != null)
                {
                    m_ShowLiveViewerForm = false; // Reset it because we're going to prod the raise here.

                    m_AppContext.ShowLiveViewer();
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock); // Pulse before we return and exit the lock.
            }
        }


        private static void ShowPackagerDialog()
        {
            ShowPackagerDialog(null);
        }

        private static void ShowPackagerDialog(Form owner)
        {
            if (UIPackagerDialog.RaisePendingToFront() == false)
            {
                // There wasn't one pending, so launch one with appropriate tear-down after it returns.
                using (UIPackagerDialog newPackager = new UIPackagerDialog()) // Dispose after it closes and returns.
                {
                    if (owner != null)
                    {
                        owner.Focus();
                        newPackager.Owner = owner; // Set the owner, if one was specified.
                    }

                    newPackager.Send(); // This will do a ShowDialog() after setting things up.
                }
            }
        }

        private delegate void MethodInvokerWithForm(Form owner);

        /// <summary>
        /// The action on the Manager thread to raise an existing viewer form.
        /// </summary>
        /// <remarks>You must BeginInvoke from the existing form/control to this method.</remarks>
        private static void RaiseAlertDialog()
        {
            lock (m_ManagerThreadLock)
            {
                m_ShowAlertForm = true; // Signal that it needs to be shown, in case the app context is gone?

                // Get the app context now that we're in a lock and righteous.
                if (m_AppContext != null)
                {
                    ExceptionDisplayRequest[] requestArray;
                    if (m_AlertQueue.Count > 0)
                    {
                        requestArray = m_AlertQueue.ToArray();
                        m_AlertQueue.Clear();
                    }
                    else
                    {
                        requestArray = null;
                    }

                    m_ShowAlertForm = false; // Reset it because we're going to prod the raise here.

                    m_AppContext.ShowAlertForm(requestArray);
                }

                System.Threading.Monitor.PulseAll(m_ManagerThreadLock); // Pulse before we return and exit the lock.
            }
        }

        private static void CreateMonitorThread()
        {
            lock (m_MonitorThreadLock)
            {
                m_MonitorThread = new Thread(MonitorThreadMain);
                m_MonitorThread.IsBackground = true;
                m_MonitorThread.Name = "Loupe Agent Monitor"; //name our thread so we can isolate it out of metrics and such
                m_MonitorThread.TrySetApartmentState(ApartmentState.MTA);
                m_MonitorThread.Start();

                System.Threading.Monitor.PulseAll(m_MonitorThreadLock);
            }
        }

        private static void MonitorThreadMain()
        {
            try
            {
                //we need to lock our culture to US english to ensure performance counters, etc. all work correctly.
                try
                {
                    CultureInfo enUSCulture = CultureInfo.CreateSpecificCulture("en-US");
                    Thread.CurrentThread.CurrentCulture = enUSCulture;
                    Thread.CurrentThread.CurrentUICulture = enUSCulture;
                }
                catch(Exception ex)
                {
                    GC.KeepAlive(ex);
#if debug
                    throw;
#endif
                }

                //First, we need to make sure we're initialized
                lock(m_ConfigLock)
                {
                    while (m_Configuration == null)
                    {
                        System.Threading.Monitor.Wait(m_ConfigLock, 1000);
                    }

                    System.Threading.Monitor.PulseAll(m_ConfigLock);
                }

                //we now have our first configuration - go for it.  This interacts with Config Log internally as it goes.
                UpdateMonitorConfiguration();

                //now we go into our wait process loop.
                m_PollingStarted = DateTimeOffset.Now;
                while (Log.IsSessionEnding == false) // Only do performance polling if we aren't shutting down.
                {
                    //mark the start of our cycle
                    DateTimeOffset previousPollStart = DateTimeOffset.UtcNow; //this really should be UTC - we aren't storing it.

                    //poll our counters
                    PerformPoll();

                    //now we need to wait for the timer to expire, but the user can update it periodically so we don't want to just
                    //assume it is unchanged for the entire wait duration.
                    DateTimeOffset targetNextPoll;
                    do
                    {
                        long waitInterval;

                        lock(m_ListenerLock)
                        {
                            waitInterval = GetTimerInterval(m_SamplingInterval);
                            System.Threading.Monitor.PulseAll(m_ListenerLock);
                        }

                        bool configUpdated;

                        if (waitInterval > 15000)
                        {
                            //the target next poll is exactly as you'd expect - the number of milliseconds from the start of the previous poll.
                            targetNextPoll = previousPollStart.AddMilliseconds(waitInterval);

                            //but we want to wake up in 15 seconds to see if the user has changed their mind.
                            configUpdated = WaitOnConfigUpdate(15000);
                        }
                        else
                        {
                            //we need to wait less than 15 seconds - pull out the time we burned since poll start.
                            int adjustedWaitInterval = (int)(previousPollStart.AddMilliseconds(waitInterval) - DateTimeOffset.Now).TotalMilliseconds;

                            //but enforce a floor so we don't go crazy cycling.
                            if (adjustedWaitInterval < 1000)
                            {
                                adjustedWaitInterval = 1000;
                            }

                            //set that to be our target next poll.
                            targetNextPoll = previousPollStart.AddMilliseconds(adjustedWaitInterval);

                            //and sleep that amount since the user won't have a chance to change their mind.
                            configUpdated = WaitOnConfigUpdate(adjustedWaitInterval);
                        }

                        if (configUpdated)
                        {
                            //apply the update.
                            UpdateMonitorConfiguration();
                        }

                    } while (targetNextPoll > DateTimeOffset.UtcNow && Log.IsSessionEnding == false);
                }
            }
            catch (Exception exception)
            {
                GC.KeepAlive(exception);
#if DEBUG
                throw; // In debug builds throw it unhandled so we definitely find out about it.
#endif
            }
            finally
            {
                m_MonitorThread = null; // We're out of the loop and about to exit the thread, so clear the thread reference.
            }
        }

        /// <summary>
        /// wait upto the specified number of milliseconds for a configuration update.
        /// </summary>
        /// <param name="maxWaitInterval"></param>
        /// <returns></returns>
        private static bool WaitOnConfigUpdate(int maxWaitInterval)
        {
            bool configUpdated;

            DateTimeOffset waitEndTime = DateTimeOffset.UtcNow.AddMilliseconds(maxWaitInterval);

            lock (m_ConfigLock)
            {
                while ((waitEndTime > DateTimeOffset.UtcNow) //haven't waited as long as we're supposed to
                    && ((m_PendingConfigChange == false) || (m_Configuration == null))) //don't have a config change
                {
                    System.Threading.Monitor.Wait(m_ConfigLock, maxWaitInterval);
                }

                configUpdated = ((m_PendingConfigChange) && (m_Configuration != null));

                System.Threading.Monitor.PulseAll(m_ConfigLock);
            }

            return configUpdated;
        }


        private static void UpdateMonitorConfiguration()
        {
            ListenerConfiguration newConfiguration;

            Log.ThreadIsInitializer = true;  //so if we wander back into Log.Initialize we won't block.

            //get the lock while we grab the configuration so we know it isn't changed out under us
            lock (m_ConfigLock)
            {
                newConfiguration = m_Configuration;

                System.Threading.Monitor.PulseAll(m_ConfigLock);
            }

            //immediately reflect this change in our multithreaded event listeners
            InitializeConsoleListener(newConfiguration);
            InitializeExceptionListener(newConfiguration);
            if (m_SuppressTraceInitialize == false) 
                InitializeTraceListener(newConfiguration);
            InitializeCLRListener(newConfiguration);

            lock(m_ConfigLock)
            {
                m_EventsInitialized = true;

                System.Threading.Monitor.PulseAll(m_ConfigLock);
            }

            //and now apply this configuration to every polled listener.
            InitializeProcessMonitor(newConfiguration);
            InitializePerformanceMonitor(newConfiguration);

            lock (m_ConfigLock)
            {
                m_PendingConfigChange = false;
                m_SuppressTraceInitialize = false;
                m_Initialized = true;

                System.Threading.Monitor.PulseAll(m_ConfigLock);
            }

            Log.ThreadIsInitializer = false;
        }


        private static void InitializeConsoleListener(ListenerConfiguration configuration)
        {
            try
            {
                lock(m_ListenerLock)
                {
                    //we can't register the console listener more than once, and it isn't designed for good register/unregister handling
                    //so we just have a simple bool to see if we did it.
                    if ((m_ConsoleListenerRegistered == false) && (configuration.EnableConsole))
                    {
                        ConsoleListener.RegisterConsoleIntercepter();
                        m_ConsoleListenerRegistered = true;
                    }
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Console Listener", 
                    "While attempting to do a routine initialization / re-initialization of the console listener, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void InitializeCLRListener(ListenerConfiguration configuration)
        {
            try
            {
                lock(m_ListenerLock)
                {
                    if (m_CLRListener == null)
                    {
                        m_CLRListener = new CLRListener();
                    }

                    //it slices and dices what is allowed internally.
                    m_CLRListener.Initialize(configuration);

                    System.Threading.Monitor.PulseAll(m_ListenerLock);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Common Language Runtime Listener",
                    "While attempting to do a routine initialization / re-initialization of the CLR listener, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void InitializeExceptionListener(ListenerConfiguration configuration)
        {
            try
            {
                lock (m_ExceptionListenerLock)
                {
                    if (m_ExceptionListener == null)
                    {
                        m_ExceptionListener = ExceptionListener.GetExceptionListener();
                    }

                    m_ExceptionListener.Initialize(configuration);

                    System.Threading.Monitor.PulseAll(m_ExceptionListenerLock);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Exception Listener",
                          "While attempting to do a routine initialization / re-initialization of the exception listener, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void InitializeTraceListener(ListenerConfiguration configuration)
        {
            try
            {
                lock (m_ListenerLock)
                {
                    if (m_TraceListener == null)
                    {
                        m_TraceListener = new LogListener();
                    }

                    m_TraceListener.Initialize(configuration);

                    System.Threading.Monitor.PulseAll(m_ListenerLock);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Trace Listener",
                          "While attempting to do a routine initialization / re-initialization of the trace listener, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void InitializePerformanceMonitor(ListenerConfiguration configuration)
        {
            try
            {
                lock (m_ListenerLock)
                {
                    //if (Log.IsMonoRuntime == false) // Perf counters don't work under Mono?  So disable them.
                    {
                        if (m_PerformanceMonitor == null)
                        {
                            m_PerformanceMonitor = new PerformanceMonitor();
                        }

                        m_PerformanceMonitor.Initialize(configuration);
                    }

                    System.Threading.Monitor.PulseAll(m_ListenerLock);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Performance Monitor",
                          "While attempting to do a routine initialization / re-initialization of the performance monitor, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void InitializeProcessMonitor(ListenerConfiguration configuration)
        {
            try
            {
                lock (m_ListenerLock)
                {
                    if ((m_ProcessMonitor == null) && (configuration.EnableProcessPerformance))
                    {
                        //if (Log.IsMonoRuntime == false) // This stuff probably doesn't work under Mono, so disable it.
                        {
                            //we don't have it and we should,
                            m_ProcessMonitor = new ProcessMonitor(false);
                        }
                    }
                    else if ((m_ProcessMonitor != null) && (configuration.EnableProcessPerformance == false))
                    {
                        //we have it and we shouldn't - get rid of it.  The polling side only looks at whether it is null or not.
                        ProcessMonitor obsoleteMonitor = m_ProcessMonitor;
                        m_ProcessMonitor = null;

                        obsoleteMonitor.Dispose();
                    }

                    System.Threading.Monitor.PulseAll(m_ListenerLock);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Initializing Process Monitor",
                          "While attempting to do a routine initialization / re-initialization of the process monitor, an exception was raised: {0}", ex.Message);
#endif
            }
        }

        private static void PerformPoll()
        {
            if (m_PerformanceMonitor != null)
            {
                try
                {
                    m_PerformanceMonitor.Poll();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                        throw;
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Polling Performance Data", "While attempting to do a routine poll of performance counters, an exception was raised: {0}", ex.Message);
#endif
                }
            }

            if (m_ProcessMonitor != null)
            {
                try
                {
                    m_ProcessMonitor.Poll();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                        throw;
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Error while Polling Performance Data", "While attempting to do a routine poll of process performance data, an exception was raised: {0}", ex.Message);
#endif
                }
            }
        }

        /// <summary>
        /// Determines the number of milliseconds in the provided interval for the timer object.
        /// </summary>
        /// <remarks>The values Default and Shortest are automatically treated as Minute by this function, effectively
        /// making once a minute the system default.</remarks>
        /// <param name="referenceInterval">The interval to calculate milliseconds for</param>
        /// <returns>The number of milliseconds between timer polls</returns>
        private static long GetTimerInterval(MetricSampleInterval referenceInterval)
        {
            //we have to convert the reference interval into the correct # of milliseconds
            long milliseconds = -1; //a safe choice because it means the timer will fire exactly once.

            switch (referenceInterval)
            {
                case MetricSampleInterval.Default:
                case MetricSampleInterval.Shortest:
                case MetricSampleInterval.Millisecond:
                    //we won't go below once a second
                    milliseconds = 1000;
                    break;
                case MetricSampleInterval.Minute:
                    milliseconds = 60000;   //sorta by definition
                    break;
                case MetricSampleInterval.Second:
                    milliseconds = 1000;   //sorta by definition
                    break;
                case MetricSampleInterval.Hour:
                    milliseconds = 3600000;
                    break;
                case MetricSampleInterval.Day:
                    milliseconds = 86400000; //get yer own calculator
                    break;
                case MetricSampleInterval.Week:
                    milliseconds = 604800000; //I mean who's going to do that, really. BTW:  Just barely a 32 bit number.
                    break;
                case MetricSampleInterval.Month:
                    milliseconds = DateTime.DaysInMonth(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month) * 86400000; //now I'm just being a smartass.
                    break;
                default:
                    break;
            }

            //before we return:  We poll artificially fast for the first few minutes and first hour.
            long secondsPolling = (long)(DateTimeOffset.Now - m_PollingStarted).TotalSeconds;
            if ((milliseconds > 5000) && (secondsPolling < 120))
            {
                milliseconds = 5000;
            }
            else if ((milliseconds > 15000) && (secondsPolling < 3600))
            {
                milliseconds = 15000;
            }

            return milliseconds;
        }

        #endregion

        #region Event Handlers

        private static void Publisher_MessageDispatching(object sender, PacketEventArgs e)
        {
            // We don't really care about the packet, we just need the prod to go check if client forms are active yet.
            CheckClientActive(true);
        }

        #endregion

        #region Private subclass ManagerApplicationContext

        private delegate void ExceptionDisplayRequestInvoker(ExceptionDisplayRequest[] newRequests);

        private class ManagerApplicationContext : ApplicationContext
        {
            private readonly int m_ThreadId;
            private readonly LiveLogViewer m_LiveLogViewer; // The persistent live viewer control.
            private LiveViewerForm m_LiveViewerForm;
            private AlertDialog m_AlertForm;
            private UIPackagerDialog m_PackagerForm;
            private int m_OpenCount;
            private bool m_LiveViewerClosing;
            private bool m_AlertFormClosing;
            private bool m_PackagerFormClosing;

            private readonly object m_Lock = new object();

            /// <summary>
            /// Create an application context to use to start a UI message loop and control its exit.
            /// </summary>
            /// <remarks>The caller should also call ShowLiveViewer() and/or ShowPackager() and/or ShowAlert()
            /// before calling Application.Run() with this context?</remarks>
            /// <param name="liveLogViewer">A persistent LiveLogViewer control.</param>
            public ManagerApplicationContext(LiveLogViewer liveLogViewer)
            {
                m_LiveLogViewer = liveLogViewer;
                m_OpenCount = 0;
                m_ThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            /// <summary>
            /// Check whether the current thread is creator of this app context or would need to invoke over to it.
            /// </summary>
            public bool InvokeRequired { get { return Thread.CurrentThread.ManagedThreadId != m_ThreadId; } }

            /// <summary>
            /// Fire an asynchronous method to be run on the Manager UI thread (not always guaranteed to run).
            /// </summary>
            /// <param name="invoker">The zero-argument delegate to invoke.</param>
            public void DoInvoke(MethodInvoker invoker)
            {
                if (InvokeRequired == false)
                {
                    invoker(); // Already on the Manager UI thread, so just call it directly.
                }
                else
                {
                    lock (m_Lock)
                    {
                        // Find any of our open forms...
                        Form invokeForm = null;
                        if (m_LiveViewerForm != null && m_LiveViewerClosing == false)
                            invokeForm = m_LiveViewerForm;
                        else if (m_AlertForm != null && m_AlertFormClosing == false)
                            invokeForm = m_AlertForm;
                        else if (m_PackagerForm != null && m_PackagerFormClosing == false)
                            invokeForm = m_PackagerForm;

                        if (invokeForm != null)
                        {
                            // Found one, use it to BeginInvoke.
                            invokeForm.BeginInvoke(invoker);
                        }
                        else
                        {
                            // Else, not much we can do, and we may be in a first-creation race condition, so don't kill us.
                            Log.DebugBreak(); // See if this is happening much.
                        }
                    }
                }
            }

            /// <summary>
            /// Fire an asynchronous method to be run on the Manager UI thread with an ExceptionDisplayRequest argument (not always guaranteed to run).
            /// </summary>
            /// <param name="invoker">An ExceptionDisplayRequestInvoker delegate to invoke.</param>
            /// <param name="newRequests">An ExceptionDisplayRequest array to pass as an argument.</param>
            public bool DoAlertInvoke(ExceptionDisplayRequestInvoker invoker, ExceptionDisplayRequest[] newRequests)
            {
                if (InvokeRequired == false)
                {
                    invoker(newRequests); // Already on the Manager UI thread, so just call it directly.
                    return true;
                }
                else
                {
                    lock (m_Lock)
                    {
                        // Find any of our open forms...
                        Form invokeForm = null;
                        if (m_AlertForm != null && m_AlertFormClosing == false)
                            invokeForm = m_AlertForm;
                        else if (m_LiveViewerForm != null && m_LiveViewerClosing == false)
                            invokeForm = m_LiveViewerForm;
                        else if (m_PackagerForm != null && m_PackagerFormClosing == false)
                            invokeForm = m_PackagerForm;

                        if (invokeForm != null)
                        {
                            // Found one, use it to BeginInvoke.
                            invokeForm.BeginInvoke(invoker, newRequests);
                            return true;
                        }
                        else
                        {
                            // Else, not much we can do, and we may be in a first-creation race condition, so don't kill us.
                            Log.DebugBreak(); // See if this is happening much.
                            return false;
                        }
                    }
                }
            }

            /// <summary>
            /// Show the Live Viewer Form, called from the Manager UI thread.
            /// </summary>
            public void ShowLiveViewer()
            {
                lock (m_Lock)
                {
                    if (m_LiveViewerForm == null)
                    {
                        m_OpenCount++;
                        m_LiveViewerClosing = false;
                        m_LiveViewerForm = new LiveViewerForm(m_LiveLogViewer);

                        m_LiveViewerForm.FormClosed += Form_FormClosed;
                        m_LiveViewerForm.Show();
                    }
                    else
                    {
                        // Otherwise, we just tell it to raise itself to the front.
                        m_LiveViewerForm.RestoreToFront();
                    }
                }
            }

            public void ShowAlertForm()
            {
                ShowAlertForm(null);
            }

            /// <summary>
            /// Show the Alert dialog, called from the Manager UI thread.
            /// </summary>
            public void ShowAlertForm(ExceptionDisplayRequest[] newRequests)
            {
                lock (m_Lock)
                {
                    if (m_AlertForm == null)
                    {
                        m_OpenCount++;
                        m_AlertFormClosing = false;
                        m_AlertForm = new AlertDialog();

                        m_AlertForm.FormClosed += Form_FormClosed;
                        m_AlertForm.Show();
                    }
                    else
                    {
                        // Otherwise, we just tell it to raise itself to the front.
                        m_AlertForm.RestoreToFront();
                    }

                    if (newRequests != null)
                    {
                        foreach (ExceptionDisplayRequest request in newRequests)
                        {
                            m_AlertForm.DisplayException(request);
                        }
                    }
                }
            }

            /// <summary>
            /// Show the Packager dialog, called from the Manager UI thread.
            /// </summary>
            public void ShowPackager()
            {
                UIPackagerDialog packagerForm;
                lock (m_Lock)
                {
                    if (m_PackagerForm == null)
                    {
                        m_OpenCount++;
                        m_PackagerFormClosing = false;
                        m_PackagerForm = new UIPackagerDialog();

                        m_PackagerForm.FormClosed += Form_FormClosed;

                        packagerForm = m_PackagerForm;
                    }
                    else
                    {
                        // Otherwise, we just tell it to raise itself to the front.
                        m_PackagerForm.RestoreToFront();
                        packagerForm = null; // No further action needed.
                    }
                }

                if (packagerForm != null)
                {
                    using (packagerForm)
                    {
                        packagerForm.StartPosition = FormStartPosition.CenterScreen; // Making it with no parent window!
                        packagerForm.Send(); // Outside the lock, because this will block until it closes!
                    }

                    // Rely on the FormClosed event to do the rest of the cleanup?
                }
            }

            /// <summary>
            /// Close all of the Manager's UI windows, called from any thread.
            /// </summary>
            public void CloseAll()
            {
                lock (m_Lock)
                {
                    // We do these as BeginInvoke even if we're already on the thread, because we want them asynchronous.
                    if (m_LiveViewerForm != null && m_LiveViewerClosing == false)
                    {
                        m_LiveViewerClosing = true;
                        m_LiveViewerForm.BeginInvoke(new MethodInvoker(m_LiveViewerForm.Close));
                    }

                    if (m_PackagerForm != null && m_PackagerFormClosing == false)
                    {
                        m_PackagerFormClosing = true;
                        m_PackagerForm.BeginInvoke(new MethodInvoker(m_PackagerForm.Close));
                    }

                    if (m_AlertForm != null && m_AlertFormClosing == false)
                    {
                        m_AlertFormClosing = true;
                        m_AlertForm.BeginInvoke(new MethodInvoker(m_AlertForm.Close));
                    }
                }
            }

            private void Form_FormClosed(object sender, EventArgs e)
            {
                if (sender == null)
                    return; // Should never happen, but could really screw us up on checks below, so bail out.

                bool exitThread = false;
                lock (m_Lock)
                {
                    if (sender == m_LiveViewerForm)
                    {
                        m_LiveViewerForm.FormClosed -= Form_FormClosed;
                        m_LiveViewerForm = null;
                        m_LiveViewerClosing = false;
                        m_OpenCount--;
                    }
                    else if (sender == m_PackagerForm)
                    {
                        m_PackagerForm.FormClosed -= Form_FormClosed;
                        m_PackagerForm = null;
                        m_PackagerFormClosing = false;
                        m_OpenCount--;
                    }
                    else if (sender == m_AlertForm)
                    {
                        m_AlertForm.FormClosed -= Form_FormClosed;
                        m_AlertForm = null;
                        m_AlertFormClosing = false;
                        m_OpenCount--;
                    }
                    else
                    {
                        // Uh-oh, an unrecognized sender!  Punt?  Recalculate the open count to make sure we're consistent.
                        int openCount = 0;
                        if (m_LiveViewerForm != null)
                            openCount++;

                        if (m_PackagerForm != null)
                            openCount++;

                        if (m_AlertForm != null)
                            openCount++;
#if DEBUG
                        if (Debugger.IsAttached)
                            Debugger.Break(); // Stop in debugger, ignore in production.
#endif

                        m_OpenCount = openCount;
                    }

                    if (m_OpenCount <= 0)
                        exitThread = true;
                }

                if (exitThread)
                {
                    //DeactivateHotKeyFilter(); // Is this needed, so we can reactivate
                    ExitThread(); // Tell our base ApplicationContext to exit the message loop (outside the lock to avoid deadlocks).
                }
            }
        }

        #endregion
    }
}