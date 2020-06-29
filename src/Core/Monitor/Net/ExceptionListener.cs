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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Loupe.Extensibility.Data;

namespace Gibraltar.Monitor.Net
{
    /// <summary>
    /// Captures application exceptions and records them to the current log.
    /// </summary>
    /// <remarks>To use this class you must also enable the LogListener.   If this class is enabled,
    /// any other unhandled exception handler may not receive exceptions (because they will be handled by this class)</remarks>
    public class ExceptionListener : IDisposable
    {
        private const string LogSystem = "Gibraltar";
        private const string Category = "System.Exception";
        private static ExceptionListener g_SingletonListener; // LOCKED BY g_Lock
        private static readonly object g_Lock = new object();
        private ListenerConfiguration m_Configuration; // LOCKED BY g_Lock
        private bool m_Initialized;
        private static volatile bool s_FatalException;

#pragma warning disable 169
        [ThreadStatic] private static bool t_PriorUIActivation;
#pragma warning restore 169

        private MessageSourceProvider m_UnhandledExceptionSourceProvider;
        private MessageSourceProvider m_ThreadExceptionSourceProvider;

        /// <summary>
        /// Create a new instance of the exception listener class.
        /// </summary>
        private ExceptionListener()
        {
        }

        /// <summary>
        /// Get or create the singleton ExceptionListener object.
        /// </summary>
        /// <returns></returns>
        public static ExceptionListener GetExceptionListener()
        {
            lock (g_Lock)
            {
                if (g_SingletonListener == null)
                    g_SingletonListener = new ExceptionListener();

                return g_SingletonListener;
            }
        }

        /// <summary>
        /// Indicates whether a fatal unhandled exception has occurred which will abort the application when the hander returns.
        /// </summary>
        public static bool FatalExceptionHasOccurred { get { return s_FatalException; } }
        
        /// <summary>
        /// Initialize the exception listener with the provided configuration, changing any running configuration.
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(ListenerConfiguration configuration)
        {
            lock (g_Lock)
            {
                ListenerConfiguration oldConfig = m_Configuration;
                m_Configuration = configuration;

                if (!m_Initialized) // First-time-only registration.
                {
                    m_Initialized = true;

                    // Note: This can only be set at the start of the application.  It can't be changed dynamically later!
                    if (Log.IsMonoRuntime == false && Log.SessionSummary.AgentAppType == ApplicationType.Windows &&
                        m_Configuration.CatchApplicationExceptions)
                    {
                        try
                        {
                            if (Debugger.IsAttached == false || AnyWindowHandleCreated() == false)
                            {
                                // Apparently, the Visual Studio debugger tends to set anyHandleCreatedInApp, so this
                                // can get a spurious exception when a debugger is attached, so test first in that case.
                                // Otherwise, we want to warn them about the order-of-execution issue, so go ahead.
                                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(LogMessageSeverity.Information, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Gibraltar Agent was unable to set default unhandled exception mode",
"The Gibraltar Agent's call to Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, false) threw an exception.  "+
"This may not usually be a problem but could affect the default way that uncaught exceptions on UI threads are handled by the runtime.  "+
"When the Agent is configured to catch application exceptions it attempts to set this default to help ensure that any UI "+
"threads which enter Application.Run() will be set up to CatchException rather than let exceptions be unhandled and fatal.\r\n\r\n"+
"This message can usually be avoided by ensuring that the Gibraltar Agent is initialized early by issuing a log message "+
"In your Program.Main method before creating the first Form or other Control and before starting any other threads.  "+
"The Agent can also be configured not to change the UnhandledExceptionMode by setting catchApplicationExceptions=\"false\" "+
"in the listener section of the gibraltar group in your App.config.\r\n");
#if DEBUG
                            if (Debugger.IsAttached)
                                Debugger.Break(); // Stop in debugger, ignore in production.
#endif
                        }
                    }

                    // It appears we only get one or the other of these, depending on whether we are the default app domain.
                    AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                    AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

                    // This is notified of any unhandled exception occurring in the current app domain.
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Doesn't break client.
                    Application.ThreadExit += Application_ThreadExit;
                }
            }
        }

        /// <summary>
        /// Register thread-specific Application events to listen for exceptions on the current UI thread.
        /// </summary>
        /// <param name="ignoreConfig">Overrides CatchApplicationException config setting if this is true.</param>
        internal void RegisterThreadEvents(bool ignoreConfig)
        {
            // The Application.ThreadException event only works on Windows UI threads (WinForms app, unsure about WPF),
            // and it could break client's own usage because it's a single-subscriber event.  Only register when appropriate.
            if (Log.SessionSummary.AgentAppType != ApplicationType.Windows)
                return;

            lock (g_Lock)
            {
                if (ignoreConfig == false && m_Configuration.CatchApplicationExceptions == false)
                    return;
            }

            if (Debugger.IsAttached && AnyWindowHandleCreatedOnThisThread() == false)
            {
                try
                {
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                }
                catch (Exception ex)
                {
                    Log.Write(LogMessageSeverity.Information, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Gibraltar Agent was unable to set thread's unhandled exception mode",
"The Gibraltar Agent's call to Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException) threw an exception.  "+
"When a debugger is attached it may interfere with the Agent's ability to set the default UnhandledExceptionMode, so when the "+
"Agent is configured to catch application exceptions and it sees a debugger attached it will instead try to set the "+
"UnhandledExceptionMode on specific threads which register for thread events--which is attempted for the main thread by default.\r\n");
#if DEBUG
                    if (Debugger.IsAttached)
                        Debugger.Break(); // Stop in debugger, ignore in production.
#endif
                }
            }

            Listener.ThreadExceptionSubscribed();
            Application.ThreadException += Application_ThreadException; // This overwrites any previous subscriber!
#if DEBUG
            Log.Trace("Application.ThreadException subscribed on this thread.");
#endif
        }

        private void UnregisterThreadEvents()
        {
            Application.ThreadException -= Application_ThreadException;
            Listener.ThreadExceptionUnsubscribed();
        }

        private void UnregisterDomainEvents()
        {
            Application.ThreadExit -= Application_ThreadExit; // This is not thread-specific subscription.
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
#if DEBUG
            Log.Trace("Unregistering domain events.");
#endif
        }

        private void Application_ThreadExit(object sender, EventArgs e)
        {
            // This gets called when the Application.ThreadContext (internal subclass) gets disposed as the message loop exits.
            // It doesn't mean the actual thread is ending, although most UI threads are structured to do so.
#if DEBUG
            Log.Trace("Application.ThreadExit event received.");
#endif
            //UnregisterThreadEvents(); // Don't need to, it's already automatically disposed by the time we get here.

            // If thread exited, it discarded our subscription and message filter.  Tell the Listener about it.
            Listener.ThreadExitedMessageLoop();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //we have to detach our handlers because these are static events.
            UnregisterDomainEvents();
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            //we have to detach our handlers because these are static events.
            UnregisterDomainEvents();
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ListenerConfiguration config;
            lock (g_Lock)
            {
                config = m_Configuration;
            }

            // We usually will be terminating, as of .NET 2.0, but let's check the event to make sure.
            bool canContinue = (e.IsTerminating == false);

            if (canContinue == false)
                s_FatalException = true;

            if (config.CatchUnhandledExceptions == false)
                return; // We're disabled for this for some reason, so just bail out.

            if (m_UnhandledExceptionSourceProvider == null)
                m_UnhandledExceptionSourceProvider = new MessageSourceProvider("System.AppDomain", "UnhandledException");

            // It's possible to get a non-CLS exception here, so we have to handle that case.
            // If so, we'll wrap it with an exception, but it's never thrown, so it will have no stack trace info.
            // Bug: Ignoring FxCop warning about generic exceptions here for now because it's a non-specified case!
            Exception exception = (e.ExceptionObject as Exception) ?? new Exception("Unwrapped Exception: " + e.ExceptionObject);

            // We want to check our config about invoking the AlertDialog.
            if (config.ReportErrorsToUser)
            {
                //MessageBox.Show(exception.Message, "CurrentDomain UnhandledException report"); //  TODO: REMOVE
                // Always do this as blocking, we're going to die when we return from this event handler.
                DialogResult result = Log.ReportException(m_UnhandledExceptionSourceProvider, exception, null, Category,
                                                          canContinue, true);

                // ToDo: Anything to do after we get the dialog result?
                if (result == DialogResult.Abort)
                {
                    //UnregisterDomainEvents(); // ???

                    // ToDo: Spin here until the AlertDialog restarts the application ???
                }
            }
            else
            {
                // We always want to log UnhandledException events, there's no down side to doing so.  Right?
                Log.RecordException(m_UnhandledExceptionSourceProvider, exception, null, Category, canContinue);
                //MessageBox.Show(exception.Message, "CurrentDomain UnhandledException record"); // TODO: REMOVE
            }

            if (canContinue == false) // e.IsTerminating or ExceptionHandlingMode.LogAndAbort
            {
                IMessageSourceProvider sourceProvider = new ExceptionSourceProvider(exception);
                if (sourceProvider.ClassName == null) // In case it was an unwrapped exception, replaced by one never thrown.
                    sourceProvider = m_UnhandledExceptionSourceProvider;

                Log.EndSession(SessionStatus.Crashed, sourceProvider, "Unhandled exception."); // Dies when we return from here.
                //UnregisterDomainEvents(); // We apparently won't get any further exiting events after this, so unregister now.

                // ToDo: Need to restart the application or hard-kill the process ??? What's done for us in AlertDialog?
            }
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ListenerConfiguration config;
            lock (g_Lock)
            {
                config = m_Configuration;
            }

            // Even if we're configured not to CatchApplicationExceptions, we still do so for our own UI thread,
            // so if we get here, we're going to log them.
            if (m_ThreadExceptionSourceProvider == null)
                m_ThreadExceptionSourceProvider = new MessageSourceProvider("System.Windows.Forms.Application", "ThreadException");

            Exception exception = e.Exception;
            if (exception == null)
            {
                // This should not happen!  But we'd better handle it, just in case.
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break(); // Stop in debugger, ignore in production.
#endif
            }
            else
            {
                ThreadPool.QueueUserWorkItem(DisplayExceptionAsync, new object[] {config, m_ThreadExceptionSourceProvider, exception});
            }
        }

        /// <summary>
        /// Display exception information asynchronously from a thread pool thread.
        /// </summary>
        /// <param name="state"></param>
        private void DisplayExceptionAsync(object state)
        {
            try //we're directly on the thread pool, no exceptions can be thrown back.
            {
                var args = (object[])state;
                var config = (ListenerConfiguration) args[0];
                var sourceProvider = (IMessageSourceProvider)args[1];
                var exception = (Exception)args[2];

                if (config.CatchApplicationExceptions && config.ReportErrorsToUser)
                {
                    // We always canContinue.  This event is not fatal, just continues the message loop.
                    Log.ReportException(m_ThreadExceptionSourceProvider, exception, null, Category, true, false);
                }
                else
                {
                    // We always canContinue.  This event is not fatal, just continues the message loop.
                    Log.RecordException(m_ThreadExceptionSourceProvider, exception, null, Category, true);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Uses reflection to query System.Windows.Forms.NativeWindow whether any window handle has been created.
        /// </summary>
        /// <returns></returns>
        private static bool AnyWindowHandleCreated()
        {
            bool boolReturn;

            // For Mono return false; they don't support this stuff.
            if (Log.IsMonoRuntime)
                return false;

            try
            {
                // NativeWindow class uses a static boolean anyHandleCreatedInApp to lock out changes to the default
                // exception mode once any window handle has been created (and a ThreadStatic one to lock out changes
                // to the thread exception mode once a handle is created on a given thread).  InApp will tell us if any
                // handle has been created on any thread.

                const BindingFlags bindingFlags = BindingFlags.GetField | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic;
                Type nativeWindowType = typeof(NativeWindow);

                object boolObject = nativeWindowType.InvokeMember("anyHandleCreatedInApp", bindingFlags, null, null, null);
                boolReturn = (bool)boolObject; // Cast it to the expected bool type.
            }
            catch
            {
                boolReturn = false; // We couldn't read it, so assume that it is not set?
            }

            return boolReturn;
        }

        /// <summary>
        /// Uses reflection to query System.Windows.Forms.NativeWindow whether any window handle has been created on this thread.
        /// </summary>
        /// <returns></returns>
        private static bool AnyWindowHandleCreatedOnThisThread()
        {
            bool boolReturn;

            try
            {
                // NativeWindow class uses a ThreadStatic boolean anyHandleCreated to lock out changes to the thread's
                // exception mode once any window handle has been created on that thread.

                const BindingFlags bindingFlags = BindingFlags.GetField | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic;
                Type nativeWindowType = typeof(NativeWindow);

                object boolObject = nativeWindowType.InvokeMember("anyHandleCreated", bindingFlags, null, null, null);
                boolReturn = (bool)boolObject; // Cast it to the expected bool type.
            }
            catch
            {
                boolReturn = true; // We couldn't read it, so assume that it is set?
            }

            return boolReturn;
        }

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Call the underlying implementation
            Dispose(true);

            // SuppressFinalize because there won't be anything left to finalize
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual releasing of managed and unmanaged resources.
        /// Most usage should instead call Dispose(), which will call Dispose(true) for you
        /// and will suppress redundant finalization.
        /// </summary>
        /// <param name="releaseManaged">Indicates whether to release managed resources.
        /// This should only be called with true, except from the finalizer which should call Dispose(false).</param>
        protected virtual void Dispose(bool releaseManaged)
        {
            if (releaseManaged)
            {
                // Free managed resources here (normal Dispose() stuff, which should itself call Dispose(true))
                // Other objects may be referenced in this case

                //we have to detach our handler because the application exit event is a static event
                UnregisterDomainEvents();
                UnregisterThreadEvents(); // ToDo: Is this a good idea?  It can only work for the current thread!
            }
            // Free native resources here (alloc's, etc)
            // May be called from within the finalizer, so don't reference other objects here
        }

        #endregion
    }
}
