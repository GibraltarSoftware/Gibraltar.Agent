
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Data;
using Gibraltar.Messaging.Net;
using Gibraltar.Monitor.Windows.Internal;
using Gibraltar.Messaging;
using Gibraltar.Server.Client;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

#pragma warning disable 1591

namespace Gibraltar.Monitor.Windows
{
    /// <summary>
    /// A real time viewer of log messages for the current application.
    /// </summary>
    [DefaultProperty("UnhandledExceptionBehavior")]
    [DefaultEvent("MessageChanged")]
    public partial class LiveLogViewer : UserControl, IComponent
    {
        /// <summary>
        /// The default number of messages the viewer will hold at one time.
        /// </summary>
        public const int DefaultMaxMessages = 10000;
        internal const string LogCategory = "Viewer.Network";

        private const string GibraltarProductName = "Gibraltar";
        private const string GibraltarApplicationName = "Application Monitor";

        private readonly Queue<ILogMessage> m_MessageQueue = new Queue<ILogMessage>();
        private volatile bool m_PendingInQueue; // Only update inside lock.  Safe to read outside of lock.
        private readonly object m_QueueLock = new object();
        private readonly bool m_InProcess;
        private readonly bool m_InternalGibraltar;
        private readonly IRemoteViewerConnection m_RemoteConnection;
        private ToolStripTextBoxDisposeHelper m_ToolStripTextBoxDisposeHelper;

        private Font m_ActiveSearchFont;
        private bool m_AutoDocked;
        private bool m_EnteringSearchText;
        private bool m_GetPriorMessages;
        private bool m_MessagesStarted;
        private bool m_ViewerEnabled;
        private bool m_RemoteConnected;

        private bool m_AutoScroll = true;
        private bool m_DetailsCollapsed;
        private LogMessageSeverity m_DefaultFilterLevel = LogMessageSeverity.Verbose;
        private LogMessageSeverity m_FilterLevel = LogMessageSeverity.Verbose; //the actual filter level in play
        private GridViewer m_GridViewer;
        private bool m_ShowToolBar;
        private bool m_ShowVerboseMessages = true;
        private Font m_InactiveSearchFont;
        private ILogMessage m_DetailedLogMessage;
        private bool m_OnLoadProcessed;
        private bool m_IdleEventRegistered;
        private bool m_FirstMessages = true; //indicates if we've processed the first messages yet (we use that for session header processing)

        private delegate void ActionSetAutoScrollDelegate(bool value);
        private delegate void ActionSetMinSeverityDelegate(LogMessageSeverity minSeverity);

        /// <summary>
        /// Raised whenever this LiveLogViewer receives a new log message, allowing the client to block it from being
        /// included for display to users.
        /// </summary>
        /// <remarks><para>Each LiveLogViewer instance filters independently with its own event.  To filter the main
        /// Gibraltar Live Viewer (raised via hotkey unless disabled), see the Log.LiveViewerMessageFilter event.</para>
        /// <para>The Message property of the event args provides the log message in consideration, and the Cancel property
        /// allows the message to be displayed (false, the default) or blocked (true).  The sender parameter of the event
        /// will identify the specific LiveLogViewer instance.</para></remarks>
        public event Listener.MessageFilterEventHandler MessageFilter;

        /// <summary>
        /// This event gets raised when the status message changes (most recent non-verbose caption).
        /// </summary>
        /// <remarks>The <see cref="LiveLogViewer">LiveLogViewer</see> tracks a status message for the
        /// session based on the Caption of the most recent log message whose severity is greater than
        /// <see cref="LogMessageSeverity.Verbose">Verbose</see>.  This event is fired when a new message of severity
        /// <see cref="LogMessageSeverity.Information">Information</see> or greater is received by the
        /// <see cref="LiveLogViewer">LiveLogViewer</see>, and the
        /// <see cref="MessageChangedEventArgs">MessageChangedEventArgs</see> contains will contain a
        /// <see cref="MessageChangedEventArgs.Message">Message</see> property with the new status message.
        /// </remarks>
        public event MessageChangedEventHandler MessageChanged;

        /// <summary>
        /// Initialize a Gibraltar Live Log Viewer control.
        /// </summary>
        /// <remarks>The control will only see log messages starting with its own creation.</remarks>
        public LiveLogViewer()
            : this(false, true)
        {
            // Redirect to the boolean version.
        }

        /// <summary>
        /// Initialize a Gibraltar Live Log Viewer control.
        /// </summary>
        /// <remarks>If getBuffer is true, the control will be initialized from a (finite) buffer of recent log messages,
        /// possibly back to the start of the application.  If getBuffer is false, the control will only see log
        /// messages starting with its own creation.</remarks>
        /// <param name="internalGibraltar">True if created for internal Gibraltar usage, false if created by client code.</param>
        /// <param name="inProcess">For monitoring the current in-process session.</param>
        public LiveLogViewer(bool internalGibraltar, bool inProcess)
            :this(internalGibraltar, inProcess, null)
        {
        }

        /// <summary>
        /// Initialize a Gibraltar Live Log Viewer control.
        /// </summary>
        /// <remarks>If getBuffer is true, the control will be initialized from a (finite) buffer of recent log messages,
        /// possibly back to the start of the application.  If getBuffer is false, the control will only see log
        /// messages starting with its own creation.</remarks>
        /// <param name="internalGibraltar">True if created for internal Gibraltar usage, false if created by client code.</param>
        /// <param name="inProcess">For monitoring the current in-process session.</param>
        /// <param name="remoteConnection">An interface to the remote computer, if out of process.</param>
        public LiveLogViewer(bool internalGibraltar, bool inProcess, IRemoteViewerConnection remoteConnection)
        {
            InitializeComponent();

            //this is necessary to workaround a defect in the textbox unbinding from events.
            ToolStripTextBoxDisposeHelper.CreateToolStripDisposeHelpers(statusBarToolStrip);

            // We have this show in the designer, but we want it collapsed by default until it actually exists.
            //messageDetailsSplitContainer.Panel2Collapsed = false;
            threadsViewSplitContainer.Panel2Collapsed = true; // TODO: Change this to use button hooks once implemented.

            m_InternalGibraltar = internalGibraltar;
            m_InProcess = inProcess;
            m_RemoteConnection = remoteConnection;

            //create our viewer
            InitializeViewer();
            ToolStripProfessionalRenderer rndr = new ToolStripProfessionalRenderer();
            rndr.ColorTable.UseSystemColors = true;
            statusBarToolStrip.Renderer = rndr;

            InitializeFilter();
            ShowOrHideDetailColumns();
            ShowOrHideMessageDetail();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (m_OnLoadProcessed == false) //in a user control it's possible for OnLoad to be raised multiple times.  really.
            {
                m_OnLoadProcessed = true;

                // After this point, we should bail if it's in the Designer.  Check the _DesignMode...
                if (_DesignMode)
                    return;

                // Okay, after this, we can be pretty sure we're in a real application environment, so start things up.
                m_ViewerEnabled = true; // We need true by default if the config is null.
                downloadToolStripButton.Visible = m_InternalGibraltar && m_InProcess; // Show it for GLV, not for LLV.
                if (m_InProcess)
                {
                    Log.IsLoggingActive(false); // This should prod Loupe to load its config!

                    // If there is a config file, apply those settings instead of
                    // the values defined in the form designer
                    ViewerMessengerConfiguration configuration = null;
                    if (m_InternalGibraltar)
                    {
                        try
                        {
                            configuration = Log.Configuration.Viewer;
                            ApplyConfigurationSettings(configuration);
                        }
                            // ReSharper disable EmptyGeneralCatchClause
                        catch
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                            //we failed to get the configuration, not much we can do.
                        }
                    }
                }

                //set the correct OS font for ourself and our controls now that we've created everything.
                FormTools.ApplyOSFont(this);

                if (m_InProcess)
                {
                    // Subscribe this viewer with the MonitorMessenger
                    if (m_GetPriorMessages)
                    {
                        ILogMessage[] buffer = MonitorMessenger.GetMessageBuffer(this);
                        ProcessNewMessages(buffer); // Bring in the old log messages before we go to our queue.
                    }
                    else
                    {
                        MonitorMessenger.Register(this); // Register without getting buffer.
                    }
                }

                if (m_RemoteConnection != null)
                {
                    m_RemoteConnection.PropertyChanged += RemoteConnection_PropertyChanged;

                    if ((m_GetPriorMessages) && (m_RemoteConnection.IsConnected))
                    {
                        //we need to load up the messages before we proceed...
                        ILogMessage[] buffer = m_RemoteConnection.GetMessageBuffer();
                        ProcessNewMessages(buffer); // Bring in the old log messages before we go to our queue.
                    }

                    //now that we've loaded the buffer, be ready for any new messages to show up.
                    m_RemoteConnection.MessageAvailable += RemoteConnection_MessageAvailable;
                    m_RemoteConnection.Connect();
                }


                m_MessagesStarted = true;

                // Don't start the polling timer until everything else is done, including processing the initial buffer.
                StartMessageDisplay();
            }
        }

        #region Public Properties and Methods

        [Browsable(false)]
        public Guid SessionId
        {
            get
            {
                if (m_RemoteConnection == null)
                {
                    return Guid.Empty;
                }

                return m_RemoteConnection.Id;
            }
        }

        /// <summary>
        /// Specifies whether to retrieve buffered messages from before the creation of this instance or start fresh.
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether to retrieve buffered messages from before the creation of this instance or start fresh.")]
        [DefaultValue(false)]
        public bool GetPriorMessages
        {
            get
            {
                return m_GetPriorMessages;
            }
            set
            {
                if (m_InProcess && m_MessagesStarted && (m_GetPriorMessages != value))
                {
                    // We need to change the initial setting after it's already started!
                    if (value)
                    {
                        MonitorMessenger.Unregister(this);
                        lock (m_QueueLock)
                        {
                            m_MessageQueue.Clear();
                            m_PendingInQueue = false;
                        }
                        ActionClear();

                        ILogMessage[] buffer = MonitorMessenger.GetMessageBuffer(this);
                        ProcessNewMessages(buffer);
                    }
                    else
                    {
                        MonitorMessenger.Unregister(this);
                        lock (m_QueueLock)
                        {
                            m_MessageQueue.Clear();
                            m_PendingInQueue = false;
                        }
                        ActionClear();

                        MonitorMessenger.Register(this);
                    }
                }

                m_GetPriorMessages = value;
            }
        }

        /// <summary>
        /// Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.")]
        [DefaultValue(DefaultMaxMessages)]
        public int MaxMessages { get { return m_GridViewer.MaxMessages; } set { m_GridViewer.MaxMessages = value; } }

        /// <summary>
        /// Specifies the default value for the filter.  If not set, no messages will be filtered.
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies the default value for the filter.  If not set, no messages will be filtered.")]
        [DefaultValue(LogMessageSeverity.Verbose)]
        public LogMessageSeverity DefaultFilterLevel
        {
            get { return m_DefaultFilterLevel; }
            set
            {
                if (m_DefaultFilterLevel == value)
                    return;

                m_DefaultFilterLevel = value;
                InitializeFilter();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LogMessageSeverity FilterLevel
        {
            get { return m_FilterLevel; }
        }

        /// <summary>
        /// Suppresses the collection and display of verbose messages
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("When false suppresses the display of verbose messages")]
        [DefaultValue(true)]
        public bool ShowVerboseMessages
        {
            get { return m_ShowVerboseMessages; }
            set
            {
                m_ShowVerboseMessages = value;
                VerboseToolStripButton.Visible = m_ShowVerboseMessages;
                VerboseToolStripButton.Checked = m_ShowVerboseMessages;
                VerboseToolStripButton.ToolTipText = string.Format("Click to {0} Verbose messages", VerboseToolStripButton.Checked ? "hide" : "show");
                if (m_GridViewer != null)
                {
                    m_GridViewer.ShowVerboseMessages = m_ShowVerboseMessages;
                    m_GridViewer.FilterMessages();
                }
            }
        }

        /// <summary>
        /// Specifies whether the grid includes developer details about threads and calling method
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the grid includes developer details about threads and calling method")]
        [DefaultValue(true)]
        public bool ShowDetailsInGrid
        {
            get { return ShowDetailsToolStripButton.Checked; }
            set
            {
                ShowDetailsToolStripButton.Checked = value;
                ShowOrHideDetailColumns();
            }
        }

        /// <summary>
        /// Specifies whether tooltips include developer details about threads and calling method
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether tooltips include developer details about threads and calling method")]
        [DefaultValue(false)]
        public bool ShowDetailsInTooltips { get { return m_GridViewer.ShowDetailsInTooltips; } set { m_GridViewer.ShowDetailsInTooltips = value; } }

        /// <summary>
        /// Specifies whether the Show Details button should be visible in the toolbar.
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Show Details button should be visible in the toolbar")]
        [DefaultValue(true)]
        public bool ShowDetailsButton
        {
            get { return ShowDetailsToolStripButton.Visible; }
            set
            {
                ShowDetailsToolStripButton.Visible = value;
                DetailsToolStripSeparator.Visible = value;
            }
        }

        /// <summary>
        /// Place-holder property to determine whether to enable the threads view (hard-coded disabled).
        /// </summary>
        [Browsable(false)]
        public bool ThreadsViewEnabled
        {
            get { return false; } // Hard-coded for now.
        }

        /// <summary>
        /// Specifies whether the Run button should display caption text next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Run button should display caption text next to the icon")]
        [DefaultValue(true)]
        public bool RunButtonTextVisible { get { return RunToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; } set { RunToolStripButton.DisplayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image; } }

        /// <summary>
        /// Caption text for Run button
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Caption text for Run button")]
        [DefaultValue("Click to Auto Refresh")]
        public string RunButtonText { get { return RunToolStripButton.Text; } set { RunToolStripButton.Text = value; } }

        /// <summary>
        /// Specifies whether the Pause button should display caption text next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Pause button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool PauseButtonTextVisible { get { return PauseToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; } set { PauseToolStripButton.DisplayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image; } }

        /// <summary>
        /// Caption text for Pause button
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Caption text for Pause button")]
        [DefaultValue("Pause")]
        public string PauseButtonText { get { return PauseToolStripButton.Text; } set { PauseToolStripButton.Text = value; } }

        /// <summary>
        /// Specifies whether the Search button should display caption text next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool SearchButtonTextVisible { get { return SearchToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; } set { SearchToolStripButton.DisplayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image; } }

        /// <summary>
        /// Caption text for Search button
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Caption text for Search button")]
        [DefaultValue("Search")]
        public string SearchButtonText { get { return SearchToolStripButton.Text; } set { SearchToolStripButton.Text = value; } }

        /// <summary>
        /// Specifies whether the Reset Search button should display caption text next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Reset Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool ResetSearchButtonTextVisible { get { return ResetSearchToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; } set { ResetSearchToolStripButton.DisplayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image; } }

        /// <summary>
        /// Caption text for Reset Search button
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Caption text for Reset Search button")]
        [DefaultValue("Reset")]
        public string ResetSearchButtonText { get { return ResetSearchToolStripButton.Text; } set { ResetSearchToolStripButton.Text = value; } }

        /// <summary>
        /// Specifies whether the Clear Search button should display caption text next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the Clear Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool ClearMessagesButtonTextVisible { get { return ClearToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; } set { ClearToolStripButton.DisplayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image; } }

        /// <summary>
        /// Caption text for Clear Messages button
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Caption text for Clear Messages button")]
        [DefaultValue("Clear")]
        public string ClearMessagesButtonText { get { return ClearToolStripButton.Text; } set { ClearToolStripButton.Text = value; } }

        /// <summary>
        /// Specifies whether the severity filter buttons should display message counts next to the icon
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Specifies whether the severity filter buttons should display message counts next to the icon")]
        [DefaultValue(true)]
        public bool ShowMessageCounters
        {
            get { return ErrorToolStripButton.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText; }
            set
            {
                ToolStripItemDisplayStyle displayStyle = value ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
                CriticalToolStripButton.DisplayStyle = displayStyle;
                ErrorToolStripButton.DisplayStyle = displayStyle;
                WarningToolStripButton.DisplayStyle = displayStyle;
                InformationToolStripButton.DisplayStyle = displayStyle;
                VerboseToolStripButton.DisplayStyle = displayStyle;
            }
        }

        /// <summary>
        /// Causes each of the message severity filter buttons to operate independently
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Causes each of the message severity filter buttons to operate independently")]
        [DefaultValue(false)]
        public bool EnableIndependentSeverityFilters { get; set; }

        /// <summary>
        /// Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy")]
        [DefaultValue(true)]
        public bool EnableMultiSelection { get { return m_GridViewer.EnableMultiSelection; } set { m_GridViewer.EnableMultiSelection = value; } }

        /// <summary>
        /// Shows or hides the built-in toolbar
        /// </summary>
        [Browsable(true)]
        [Category("Gibraltar")]
        [Description("Shows or hides the built-in toolbar")]
        [DefaultValue(false)]
        public bool ShowToolBar { get { return m_ShowToolBar; } set { SetShowToolBar(value); } }

        public void Clear()
        {
            ActionClear();
        }

        public void CopyAll()
        {
            ActionCopyAll();
        }

        public void CopySelection()
        {
            ActionCopySelection();
        }

        public void MoveFirst()
        {
            ActionMoveFirst();
        }

        public void MoveLast()
        {
            ActionMoveLast();
        }

        public void SetAutoScroll(bool value)
        {
            ActionSetAutoScroll(value);
        }

        public void Save()
        {
            ActionSave();
        }

        public void SelectAll()
        {
            ActionSelectAll();
        }

        #endregion

        #region Internal Properties and Methods

        internal void QueueViewerMessage(ILogMessage newMessage)
        {
            bool pokePoll;
            lock (m_QueueLock)
            {
                pokePoll = (m_MessageQueue.Count == 0); // If none in the queue, we need to poke a polling check below.

                // Verbose is numerically the highest value, so this comparison only SEEMS backwards.
                if (m_ShowVerboseMessages || newMessage.Severity < LogMessageSeverity.Verbose)
                {
                    m_MessageQueue.Enqueue(newMessage);
                    m_PendingInQueue = true;
                }

                //we need to protect ourself from running out of memory - if the queue is too deep, we drop a message.
                if (MaxMessages > 0 && m_MessageQueue.Count > MaxMessages)
                    m_MessageQueue.Dequeue();

                if (Frozen)
                {
                    UpdateViewStatus();
                    // ActionPoll() won't do anything if it's Frozen, anyway. (Plus this will probably trigger another Idle event.)
                }
                else if (pokePoll)
                {
                    // We need to poke a polling check.
                    if (IsHandleCreated) // Don't try to BeginInvoke if our handle is gone.
                        BeginInvoke(new MethodInvoker(ActionPoll));
                }
            }
        }


        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Gets or sets the LogMessage to display in detail.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        protected ILogMessage DetailedLogMessage
        {
            get { return m_DetailedLogMessage; }
            set
            {
                if (value != m_DetailedLogMessage)
                {
                    m_DetailedLogMessage = value;
                    messageDetails.LogMessage = value;
                }
            }
        }

        /// <summary>
        /// Raised when Visible is assigned or changes state.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            m_GridViewer.Visible = Visible; // Do we need to pass this on ourselves?
            if (Visible)
                m_GridViewer.PerformResize(); // Is this needed?
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent == null)
            {
                UnregisterIdleEvent();
            }
            else
            {
                RegisterIdleEvent();
            }
        }

        #endregion

        #region Private Properties and Methods

        private delegate void ActionDisplayConnectedInvoker(bool isConnected);

        private void ActionDisplayConnected(bool isConnected)
        {
            //make sure we're on the right thread
            if (InvokeRequired)
            {
                ActionDisplayConnectedInvoker invoker = ActionDisplayConnected;
                BeginInvoke(invoker, new object[] {isConnected});
            }
            else
            {
                m_RemoteConnected = isConnected;
                SaveToolStripButton.Enabled = m_RemoteConnected;

                UpdateViewStatus(); //this displays our status label.
            }
        }

        /// <summary>
        /// Start up the packager to save the contents of the session.
        /// </summary>
        private void ActionSave()
        {
            //make sure we're on the right thread
            if (InvokeRequired)
            {
                MethodInvoker invoker = ActionSave;
                BeginInvoke(invoker);
            }
            else
            {
                //now wait...  we need to decide which way to go - remote or local.
                if (m_InProcess)
                {
                    if (UIPackagerDialog.RaisePendingToFront() == false)
                    {
                        // There wasn't one pending, so launch one with appropriate tear-down after it returns.
                        using (UIPackagerDialog newPackager = new UIPackagerDialog())
                        {
                            newPackager.Send(); // This will do a ShowDialog() after setting things up.
                        }
                    }
                }
                else
                {
                    try
                    {
                        m_RemoteConnection.SendToServer(SessionCriteria.ActiveSession);
                    }
                    catch (Exception ex)
                    {
                        Log.ReportException(0, ex, null, LogCategory, true, false);
                    }
                }
            }
        }


        private bool Frozen { get { return m_GridViewer.Frozen; } set { m_GridViewer.Frozen = value; } }

        private bool _DesignMode { get { return (GetService(typeof(IDesignerHost)) != null) || (LicenseManager.UsageMode == LicenseUsageMode.Designtime); } }

        private void UpdateViewStatus()
        {
            if (IsDisposed)
                return; // There's nothing to do if we're disposed.

            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(UpdateViewStatus)); // Kick it on the right thread automatically.
            else
            {
                if (m_GridViewer == null)
                {
                    ViewerStatusToolStripLabel.Text = string.Empty;
                    return;
                }

                int count = m_GridViewer.MessageCount;
                int visibleCount = m_GridViewer.VisibleCount;

                string statusMessage = (visibleCount == count)
                                        ? string.Format("{0:g} messages", count)
                                        : string.Format("{0:g} shown of {1:g} messages", visibleCount, count);


                CriticalToolStripButton.Text = m_GridViewer.CriticalMessageCount.ToString(CultureInfo.InvariantCulture);
                ErrorToolStripButton.Text = m_GridViewer.ErrorMessageCount.ToString(CultureInfo.InvariantCulture);
                WarningToolStripButton.Text = m_GridViewer.WarningMessageCount.ToString(CultureInfo.InvariantCulture);
                InformationToolStripButton.Text = m_GridViewer.InfoMessageCount.ToString(CultureInfo.InvariantCulture);
                VerboseToolStripButton.Text = m_GridViewer.VerboseMessageCount.ToString(CultureInfo.InvariantCulture);

                if ((m_RemoteConnection != null) && (m_RemoteConnection.IsConnected == false))
                {
                    statusMessage = " (Disconnected)";
                }
                else if (Frozen && m_PendingInQueue)
                {
                    statusMessage += string.Format(" (+{0} waiting)", m_MessageQueue.Count); // Is this safe without the lock?
                }

                ViewerStatusToolStripLabel.Text = statusMessage;
            }
        }

        private void InitializeFilter()
        {
            // DefaultFilterLevel is an enum but we use string values for the combobox
            // to improve readability.  So, we need to map the enum to the proper string.
            // FYI, this logic is also invoked at design time also to provide nice visual feedback.
            switch (DefaultFilterLevel)
            {
                case LogMessageSeverity.Critical:
                    ActionSetMinSeverity(LogMessageSeverity.Critical);
                    break;
                case LogMessageSeverity.Error:
                    ActionSetMinSeverity(LogMessageSeverity.Error);
                    break;
                case LogMessageSeverity.Warning:
                    ActionSetMinSeverity(LogMessageSeverity.Warning);
                    break;
                case LogMessageSeverity.Information:
                    ActionSetMinSeverity(LogMessageSeverity.Information);
                    break;
                default:
                    ActionSetMinSeverity(LogMessageSeverity.Verbose);
                    break;
            }

            if (EnableIndependentSeverityFilters)
            {
                CriticalToolStripButton.ToolTipText = string.Format("Click to {0} Critical messages", CriticalToolStripButton.Checked ? "hide" : "show");
                ErrorToolStripButton.ToolTipText = string.Format("Click to {0} Error messages", ErrorToolStripButton.Checked ? "hide" : "show");
                WarningToolStripButton.ToolTipText = string.Format("Click to {0} Warning messages", WarningToolStripButton.Checked ? "hide" : "show");
                InformationToolStripButton.ToolTipText = string.Format("Click to {0} Information messages", InformationToolStripButton.Checked ? "hide" : "show");
                VerboseToolStripButton.ToolTipText = string.Format("Click to {0} Verbose messages", VerboseToolStripButton.Checked ? "hide" : "show");
            }

            m_InactiveSearchFont = SearchToolStripTextBox.Font;
            m_ActiveSearchFont = new Font(m_InactiveSearchFont, FontStyle.Regular);
        }

        private void StartMessageDisplay()
        {
            if (_DesignMode)
                return;

            // We leave these visible for the designer, but they should be hidden
            // when we are running for real.
            RunToolStripButton.Visible = false;
            SearchToolStripButton.Visible = false;
            ResetSearchToolStripButton.Visible = false;

            RegisterIdleEvent();
        }

        private void ShutdownViewer()
        {
            if (m_InProcess)
            {
                MonitorMessenger.Unregister(this);
            }

            UnregisterIdleEvent();

            if (m_RemoteConnection != null)
            {
                m_RemoteConnection.MessageAvailable -= RemoteConnection_MessageAvailable;
                m_RemoteConnection.PropertyChanged -= RemoteConnection_PropertyChanged;
            }
        }

        private void SetShowToolBar(bool value)
        {
            m_ShowToolBar = value;

            statusBarToolStrip.Visible = m_ShowToolBar;
        }

        private void ActionClear()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionClear;
                viewerControl.BeginInvoke(invoker);
            }
            else
            {
                m_GridViewer.Clear();
                DetailedLogMessage = null; // No log message to display, so clear it out.
                ActionSetAutoScroll(true);
                UpdateViewStatus();
            }
        }

        private static void ActionWebpage()
        {
            try
            {
                Log.DisplayWebPage("gibraltar-sds-info.aspx");
            }
            catch // do no harm --> catch all exceptions
            {
                MessageBox.Show("Loupe Live Viewer", "Please check OnLoupe.com for the latest version of Loupe.");
            }
        }

        private void ActionCopyAll()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionCopyAll;
                viewerControl.BeginInvoke(invoker);
            }
            else
            {
                //put whatever's in the grid on the clipboard
                m_GridViewer.CopyAll();
            }
        }

        private void ActionCopySelection()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionCopySelection;
                viewerControl.BeginInvoke(invoker);
            }
            else
            {
                //put whatever's selected in the grid on the clipboard
                m_GridViewer.CopySelection();
            }
        }

        private void ActionMoveFirst()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionMoveFirst;
                viewerControl.BeginInvoke(invoker);
            }
            else
            {
                // Disable autoscrolling whenever user invokes ActionMoveFirst
                ActionSetAutoScroll(false);
                m_GridViewer.MoveFirst();
            }
        }

        private void ActionMoveLast()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionMoveLast;
                viewerControl.BeginInvoke(invoker);
            }
            else
            {
                m_GridViewer.MoveLast();
            }
        }

        private void ActionPoll()
        {
            if (m_PendingInQueue == false) // Fast bail if the MessageQueue is empty.  Don't bother getting the lock.
                return;

            //quick exit:  if we are starting our dispose cycle, don't do anything.
            if (IsDisposed)
                return;

            if (Frozen)
                return;

            try // ToDo: Can this be improved by try/catching more selectively instead of bailing on the whole section?
            {
                //Get the messages as fast as possible from the queue so we can release it.
                ILogMessage[] newMessageArray = null; // Avoid work if there are no new messages.

                lock (m_QueueLock)
                {
                    if (m_AutoScroll == false && MaxMessages > 0 && m_GridViewer.MessageCount >= MaxMessages)
                    {
                        Frozen = true;
                        return;
                    }

                    m_PendingInQueue = false; // We're about to empty the queue if it has any.  If not this should be clear!
                    if (m_MessageQueue.Count > 0)
                    {
                        // The fastest way is to get the entire queue as an array then clear it.
                        newMessageArray = m_MessageQueue.ToArray();
                        m_MessageQueue.Clear();
                    }
                    else
                        return; // Skip ProcessNewMessages() if there aren't any new ones. ???
                }

                ProcessNewMessages(newMessageArray);
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
                //we want to ignore this exception and keep polling
            }
        }


        private void ActionSelectAll()
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                MethodInvoker invoker = ActionSelectAll;
                viewerControl.BeginInvoke(invoker);
            }
            else
                m_GridViewer.SelectAll();
        }


        private void ActionSetAutoScroll(bool value)
        {
            if (m_GridViewer == null)
                return;

            //make sure we're on the right thread
            if (InvokeRequired)
            {
                ActionSetAutoScrollDelegate invoker = ActionSetAutoScroll;
                BeginInvoke(invoker, new object[] {value});
            }
            else
            {
                m_AutoScroll = value;
                if (m_AutoScroll)
                {
                    if (m_GridViewer.Sorted)
                        m_GridViewer.RemoveSort();

                    m_GridViewer.MoveLast(); // Jump to bottom when resuming from pause.
                    m_GridViewer.Frozen = false;
                    m_GridViewer.ClearSelection();
                    OnResize(EventArgs.Empty); // Is this better than calling our m_GridViewer.PerformResize()?

                    ILogMessage message = m_GridViewer.GetCurrentMessage();
                    DetailedLogMessage = message;
                }
                m_GridViewer.AutoScrollMessages = value;
                PauseToolStripButton.Visible = m_AutoScroll;
                RunToolStripButton.Visible = !m_AutoScroll;
            
                // if we have buffered messages we need to explicitly catch up the buffer.
                // in some cases the normal process that would auto-check on next message won't work because the buffer is full.
                if (IsHandleCreated && m_AutoScroll) // Don't try to BeginInvoke if our handle is gone.
                    ActionPoll();
            }
        }


        private void ActionSetMinSeverity(LogMessageSeverity minSeverity)
        {
            //make sure we're on the right thread
            Control viewerControl = m_GridViewer;
            if (viewerControl.InvokeRequired)
            {
                ActionSetMinSeverityDelegate invoker = ActionSetMinSeverity;
                viewerControl.BeginInvoke(invoker, new object[] {minSeverity});
            }
            else
            {
                // Remember that LMS is numerically inverted: Verbose=16, Critical=1. Compares look backwards!
                CriticalToolStripButton.Checked = (minSeverity >= LogMessageSeverity.Critical); // Critical=1
                ErrorToolStripButton.Checked = (minSeverity >= LogMessageSeverity.Error); // Error=2
                WarningToolStripButton.Checked = (minSeverity >= LogMessageSeverity.Warning); // Warning=4
                InformationToolStripButton.Checked = (minSeverity >= LogMessageSeverity.Information); // Information=8
                VerboseToolStripButton.Checked = (minSeverity >= LogMessageSeverity.Verbose); // Verbose=16

                m_GridViewer.ShowCriticalMessages = CriticalToolStripButton.Checked;
                m_GridViewer.ShowErrorMessages = ErrorToolStripButton.Checked;
                m_GridViewer.ShowWarningMessages = WarningToolStripButton.Checked;
                m_GridViewer.ShowInformationMessages = InformationToolStripButton.Checked;
                m_GridViewer.ShowVerboseMessages = VerboseToolStripButton.Checked;

                // Set them all to their most frequent value, then override just the current one (which will toggle).
                CriticalToolStripButton.ToolTipText = "Click to show only Critical severity (narrowest filter)";
                ErrorToolStripButton.ToolTipText = "Click to show only severity of Error and above";
                WarningToolStripButton.ToolTipText = "Click to show only severity of Warning and above";
                InformationToolStripButton.ToolTipText = "Click to show only severity of Information and above";
                VerboseToolStripButton.ToolTipText = "Click to show all messages (no filtering)";

                switch (minSeverity)
                {
                    case LogMessageSeverity.Critical:
                        CriticalToolStripButton.ToolTipText = "Showing only Critical severity (narrowest filter)";
                        break;
                    case LogMessageSeverity.Error:
                        ErrorToolStripButton.ToolTipText = "Click to hide messages with severity of Error and below";
                        break;
                    case LogMessageSeverity.Warning:
                        WarningToolStripButton.ToolTipText = "Click to hide messages with severity of Warning and below";
                        break;
                    case LogMessageSeverity.Information:
                        InformationToolStripButton.ToolTipText = "Click to hide messages with severity of Information and below";
                        break;
                    case LogMessageSeverity.Verbose:
                        VerboseToolStripButton.ToolTipText = "Click to hide messages with Verbose severity";
                        break;
                }

                m_FilterLevel = minSeverity;
            }
        }

        private void ActionSetSearchText(string searchText)
        {
            m_GridViewer.SearchText = searchText;
        }

        private void OnMessageChange(string newMessage)
        {
            MessageChangedEventHandler eventHandler = MessageChanged;
            if (eventHandler != null)
            {
                //invoke the delegate
                MessageChanged(this, new MessageChangedEventArgs(newMessage));
            }
        }

        private void ProcessNewMessages(ILogMessage[] newMessageArray)
        {
            // Protect against null (we WILL get called with it), so just bail early.
            // But don't bail on 0-length array, leave it as a hook to kick us to AutoScroll even without new messages.
            if (newMessageArray == null)
                return; // Nothing to filter;

            // First we want to run the incoming messages through any mandatory client-side filter.
            ILogMessage[] clientFilteredArray = ClientFilterNewMessages(newMessageArray) ?? new ILogMessage[0];

            // Update the status message based on the latest message that isn't verbose.
            ILogMessage statusMessage = null;
            for (int curMessageIndex = clientFilteredArray.GetUpperBound(0);
                 curMessageIndex >= 0;
                 curMessageIndex--)
            {
                ILogMessage currentMessage = clientFilteredArray[curMessageIndex];
                if (currentMessage.Severity != LogMessageSeverity.Verbose)
                {
                    statusMessage = currentMessage;
                    break; //we found our message, we're done
                }
            }

            // Now display the new ones.
            if (m_GridViewer != null)
            {
                m_GridViewer.AddRange(clientFilteredArray);

                if (m_AutoScroll)
                {
                    ILogMessage message = m_GridViewer.GetCurrentMessage();
                    DetailedLogMessage = message; // Display the current LogMessage.
                }
            }

            if (statusMessage != null)
                OnMessageChange(statusMessage.Caption); // Was statusMessage.Message
        }

        private ILogMessage[] ClientFilterNewMessages(ILogMessage[] newMessageArray)
        {
            // Bail early if no messages to filter.
            if (newMessageArray == null || newMessageArray.Length <= 0)
                return newMessageArray; // Nothing to filter;

            Listener.MessageFilterEventHandler filterEventHandler = MessageFilter; // Get any subscribers.
            if (filterEventHandler != null)
            {
                if (newMessageArray.Length > 1)
                {
                    List<ILogMessage> clientFilteredList = new List<ILogMessage>(newMessageArray.Length);
                    foreach (ILogMessage currentMessage in newMessageArray)
                    {
                        bool cancelMessage;
                        try
                        {
                            MessageFilterEventArgs eventArgs = new MessageFilterEventArgs(currentMessage);
                            filterEventHandler(this, eventArgs);
                            cancelMessage = eventArgs.Cancel;
                        }
                        catch
                        {
                            // Can we log exceptions here?  It might create a spam loop....
                            cancelMessage = false;
                        }

                        if (cancelMessage == false)
                            clientFilteredList.Add(currentMessage);
                    }

                    if (clientFilteredList.Count <= 0)
                        return null;

                    newMessageArray = clientFilteredList.ToArray();
                }
                else // Length == 1
                {
                    ILogMessage currentMessage = newMessageArray[0];
                    try
                    {
                        MessageFilterEventArgs eventArgs = new MessageFilterEventArgs(currentMessage);
                        filterEventHandler(this, eventArgs);
                        if (eventArgs.Cancel)
                            return null; // Cancel the one and only message.
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                    }

                    // The single message passed the filter, so it's the whole array.
                }
            }

            return newMessageArray;
        }

        private void ApplyConfigurationSettings(ViewerMessengerConfiguration configuration)
        {
            if (configuration == null)
                return;

            m_ViewerEnabled = configuration.Enabled;
            ClearMessagesButtonText = configuration.ClearMessagesButtonText;
            ClearMessagesButtonTextVisible = configuration.ClearMessagesButtonTextVisible;
            DefaultFilterLevel = configuration.DefaultFilterLevel;
            EnableIndependentSeverityFilters = configuration.EnableIndependentSeverityFilters;
            EnableMultiSelection = configuration.EnableMultiSelection;
            ShowToolBar = configuration.ShowToolBar;
            ShowVerboseMessages = configuration.ShowVerboseMessages;
            MaxMessages = configuration.MaxMessages;
            PauseButtonText = configuration.PauseButtonText;
            PauseButtonTextVisible = configuration.PauseButtonTextVisible;
            ResetSearchButtonText = configuration.ResetSearchButtonText;
            ResetSearchButtonTextVisible = configuration.ResetSearchButtonTextVisible;
            RunButtonText = configuration.RunButtonText;
            RunButtonTextVisible = configuration.RunButtonTextVisible;
            ShowDetailsButton = configuration.ShowDetailsButton;
            ShowDetailsInGrid = configuration.ShowDetailsInGrid;
            ShowDetailsInTooltips = configuration.ShowDetailsInTooltips;
            ShowMessageCounters = configuration.ShowMessageCounters;
        }

        private void InitializeViewer()
        {
            UpdateViewStatus();

            GridViewer viewerControl = new GridViewer();
            m_GridViewer = viewerControl;

            if (m_InProcess)
            {
                m_GridViewer.ColumnVisible(LogMessageColumn.UserName, false);
            }

            //and add it to our form and dock it.
            messageDetailsSplitContainer.Panel1.Controls.Add(viewerControl); // New position in the inner-most split container.
            viewerControl.Dock = DockStyle.Fill;
            messageDetails.LogMessage = null; // Clear the default designer displays in case there's no actual message.

            m_GridViewer.EnableMultiSelection = true;

            m_GridViewer.UserInteraction += m_Viewer_UserInteraction;
            m_GridViewer.ViewChanged += m_Viewer_ViewChanged;
            m_GridViewer.FilterChanged += m_Viewer_FilterChanged;
            m_GridViewer.AutoScrollMessages = m_AutoScroll;
            m_GridViewer.Selection.SelectionChanged += m_GridViewer_Selection_SelectionChanged;
        }

        private void RefreshSearch()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                string searchText = SearchToolStripTextBox.Text;
                ActionSetSearchText(searchText);
                UpdateSearchBackColor();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearSearch()
        {
            SearchToolStripTextBox.Font = m_InactiveSearchFont;
            SearchToolStripTextBox.ForeColor = Color.Gray;
            SearchToolStripTextBox.Text = "Search Messages";
            ResetSearchToolStripButton.Visible = false;
            SearchToolStripButton.Visible = false;
            ActionSetSearchText(null);
        }

        private void UpdateSearchBackColor()
        {
            if (SearchToolStripTextBox.Text == m_GridViewer.SearchText && !string.IsNullOrEmpty(SearchToolStripTextBox.Text))
                SearchToolStripTextBox.BackColor = Color.Cornsilk;
            else
                SearchToolStripTextBox.BackColor = Color.White;

            m_GridViewer.UpdateGridBackColor();
        }

        /// <summary>
        /// Ask the supplied agent to attempt to connect and retrieve live data
        /// </summary>
        /// <param name="state"></param>
        private void AsyncSessionConnect(object state)
        {
            try //we are a thread main method so we can't let exceptions roll
            {
                var session = (IRemoteViewerConnection)state;
                session.Connect();
            }
            catch (Exception ex)
            {
                if (!Log.SilentMode) Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, NetworkClient.LogCategory, "Unable to connect to session data stream", "While attempting to connect the session to the server to get the live stream an exception was thrown, and we are presumed not connected.\r\nException: {0}", ex.Message);
            }
        }

        private void RegisterIdleEvent()
        {
            if (m_IdleEventRegistered)
                return;

            m_IdleEventRegistered = true;
            if (m_ViewerEnabled)
            {
#if USE_POLLING_TIMER
                m_DisplayTimer = new Timer {Interval = 500};
                m_DisplayTimer.Start();
                m_DisplayTimer.Tick += m_DisplayTimer_Tick;
#else
                Application.Idle += Application_Idle;
#endif
            }
        }

        private void UnregisterIdleEvent()
        {
            m_IdleEventRegistered = false;
            Application.Idle -= Application_Idle; //it's always safe to unsubscribe multiple times...

#if USE_POLLING_TIMER
            if (m_DisplayTimer != null)
            {
                m_DisplayTimer.Stop();
                m_DisplayTimer.Tick -= m_DisplayTimer_Tick;
                m_DisplayTimer.Dispose();
            }
#endif            
        }

        #endregion

        #region Event Handlers

        private void m_Viewer_ViewChanged(object sender, EventArgs e)
        {
            //update our display label
            UpdateViewStatus();
        }

        private void m_Viewer_FilterChanged(object sender, EventArgs e)
        {
            if (m_AutoScroll)
            {
                ILogMessage message = m_GridViewer.GetCurrentMessage();
                DetailedLogMessage = message;
            }
            // Otherwise, the selection should already be changed if appropriate.
        }

        private void m_Viewer_UserInteraction(object sender, EventArgs e)
        {
            ActionSetAutoScroll(false);
        }

        private void m_GridViewer_Selection_SelectionChanged(object sender, EventArgs e)
        {
            ILogMessage message = m_GridViewer.GetSelectedMessage();
            if (message != null)
                DetailedLogMessage = message;
        }

        private void RemoteConnection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var remoteConnection = (IRemoteViewerConnection)sender;

            switch (e.PropertyName)
            {
                case "IsAvailable":
                    ThreadPool.QueueUserWorkItem(AsyncSessionConnect, remoteConnection);
                    break;
                case "IsConnected":
                    ActionDisplayConnected(remoteConnection.IsConnected);
                    break;
            }
        }

        private void RemoteConnection_MessageAvailable(object sender, MessageAvailableEventArgs e)
        {
            QueueViewerMessage(e.Message);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ActionPoll();
        }

        private void RunToolStripButton_Click(object sender, EventArgs e)
        {
            ActionSetAutoScroll(true);
        }

        private void PauseToolStripButton_Click(object sender, EventArgs e)
        {
            ActionSetAutoScroll(false);
            Frozen = true;
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            ActionSave();
        }

        private void CopyToolStripButton_Click(object sender, EventArgs e)
        {
            ActionCopyAll();
        }

        private void ClearToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Clear all log messages?", "Clear Messages", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if( result == DialogResult.OK)
                ActionClear();
        }

        private void downloadToolStripButton_Click(object sender, EventArgs e)
        {
            ActionWebpage();
        }

        private void MoveFirstToolStripButton_Click(object sender, EventArgs e)
        {
            ActionMoveFirst();
        }

        private void MoveLastToolStripButton_Click(object sender, EventArgs e)
        {
            ActionMoveLast();
        }

        private void ShowDetailsToolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;
            ShowDetailsInGrid = button.Checked;
            ShowOrHideDetailColumns();
        }

        private void ShowOrHideMessageDetail()
        {
            int minHeight = messageDetailsSplitContainer.Panel2MinSize * 2;

            bool nowCollapse = (messageDetailsSplitContainer.Height < minHeight);

            if (nowCollapse == m_DetailsCollapsed)
                return; //we're already in the right config.

            messageDetailsSplitContainer.Panel2Collapsed = nowCollapse;
            m_DetailsCollapsed = nowCollapse; // Remember for next time.

            if (nowCollapse)
            {
                // If we're autoscrolling, make sure we didn't obscure the last messages when we redisplayed the details panel.
                if (m_DetailsCollapsed && m_GridViewer.AutoScrollMessages)
                    ActionMoveLast();                
            }
        }

        private void ShowOrHideDetailColumns()
        {
            if (m_GridViewer != null)
            {
                m_GridViewer.ColumnVisible(LogMessageColumn.Thread, ShowDetailsInGrid);
                m_GridViewer.ColumnVisible(LogMessageColumn.Method, ShowDetailsInGrid);
            }

            //make sure our toolbar is in sync
            ShowDetailsToolStripButton.Checked = ShowDetailsInGrid;
            if (m_GridViewer != null)
                m_GridViewer.PerformResize();
        }

        private void CriticalToolStripButton_Click(object sender, EventArgs e)
        {
            if (EnableIndependentSeverityFilters)
            {
                m_GridViewer.ShowCriticalMessages = CriticalToolStripButton.Checked;
                CriticalToolStripButton.ToolTipText = string.Format("Click to {0} Error messages", CriticalToolStripButton.Checked ? "hide" : "show");
            }
            else
                ActionSetMinSeverity(LogMessageSeverity.Critical);
            m_GridViewer.FilterMessages();
        }

        private void ErrorToolStripButton_Click(object sender, EventArgs e)
        {
            if (EnableIndependentSeverityFilters)
            {
                m_GridViewer.ShowErrorMessages = ErrorToolStripButton.Checked;
                ErrorToolStripButton.ToolTipText = string.Format("Click to {0} Error messages", ErrorToolStripButton.Checked ? "hide" : "show");
            }
            else
                ActionSetMinSeverity(LogMessageSeverity.Error);
            m_GridViewer.FilterMessages();
        }

        private void WarningToolStripButton_Click(object sender, EventArgs e)
        {
            if (EnableIndependentSeverityFilters)
            {
                m_GridViewer.ShowWarningMessages = WarningToolStripButton.Checked;
                WarningToolStripButton.ToolTipText = string.Format("Click to {0} Warning messages", WarningToolStripButton.Checked ? "hide" : "show");
            }
            else
            {
                if (WarningToolStripButton.Checked || InformationToolStripButton.Checked)
                    ActionSetMinSeverity(LogMessageSeverity.Warning);
                else
                    ActionSetMinSeverity(LogMessageSeverity.Error);
            }
            m_GridViewer.FilterMessages();
        }

        private void InformationToolStripButton_Click(object sender, EventArgs e)
        {
            if (EnableIndependentSeverityFilters)
            {
                m_GridViewer.ShowInformationMessages = InformationToolStripButton.Checked;
                InformationToolStripButton.ToolTipText = string.Format("Click to {0} Information messages", InformationToolStripButton.Checked ? "hide" : "show");
            }
            else
            {
                if (InformationToolStripButton.Checked || VerboseToolStripButton.Checked)
                    ActionSetMinSeverity(LogMessageSeverity.Information);
                else
                    ActionSetMinSeverity(LogMessageSeverity.Warning);
            }
            m_GridViewer.FilterMessages();
        }

        private void VerboseToolStripButton_Click(object sender, EventArgs e)
        {
            if (EnableIndependentSeverityFilters)
            {
                m_GridViewer.ShowVerboseMessages = VerboseToolStripButton.Checked;
                VerboseToolStripButton.ToolTipText = string.Format("Click to {0} Verbose messages", VerboseToolStripButton.Checked ? "hide" : "show");
            }
            else
            {
                if (VerboseToolStripButton.Checked)
                    ActionSetMinSeverity(LogMessageSeverity.Verbose);
                else
                    ActionSetMinSeverity(LogMessageSeverity.Information);
            }
            m_GridViewer.FilterMessages();
        }

        /// <summary>
        /// This method autodocks the control when it is first dropped on a form.
        /// </summary>
        private void CMViewer_ParentChanged(object sender, EventArgs e)
        {
            // This logic is intended to allow the user to disable the auto-docking, if they try.
            if (!m_AutoDocked && Dock == DockStyle.None && Anchor == (AnchorStyles.Top | AnchorStyles.Left))
            {
                Dock = DockStyle.Fill;
                m_AutoDocked = true;
            }
        }

        private void SearchToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Return)
        }

        private void SearchToolStripButton_Click(object sender, EventArgs e)
        {
            RefreshSearch();
        }

        private void SearchToolStripTextBox_Enter(object sender, EventArgs e)
        {
            if (m_EnteringSearchText)
                return;

            try
            {
                m_EnteringSearchText = true;
                if (SearchToolStripTextBox.ForeColor != Color.Black)
                {
                    SearchToolStripTextBox.Font = m_ActiveSearchFont;
                    SearchToolStripTextBox.ForeColor = Color.Black;
                    SearchToolStripTextBox.Text = "";
                }
            }
            finally
            {
                m_EnteringSearchText = false;
            }
        }

        private void SearchToolStripTextBox_Leave(object sender, EventArgs e)
        {
            if (m_EnteringSearchText)
                return;

            try
            {
                m_EnteringSearchText = true;
                if (string.IsNullOrEmpty(SearchToolStripTextBox.Text))
                {
                    ResetSearchToolStripButton.Visible = false;
                    ClearSearch();
                }
            }
            finally
            {
                m_EnteringSearchText = false;
            }
        }

        private void ClearSearchToolStripButton_Click(object sender, EventArgs e)
        {
            ClearSearch();
            UpdateSearchBackColor();
        }

        private void SearchToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (m_EnteringSearchText)
                return;

            ResetSearchToolStripButton.Visible = !string.IsNullOrEmpty(SearchToolStripTextBox.Text);

            RefreshSearch();
            //UpdateSearchBackColor(); // RefreshSearch() already does this.
            SearchToolStripTextBox.Focus();
        }

        private void messageDetailsSplitContainer_Resize(object sender, EventArgs e)
        {
            ShowOrHideMessageDetail();
        }

        private void threadsViewSplitContainer_Resize(object sender, EventArgs e)
        {
            int minWidth = threadsViewSplitContainer.Panel2MinSize * 2;
            if (threadsViewSplitContainer.Width < minWidth)
                threadsViewSplitContainer.Panel2Collapsed = true; // Can't display if the whole control is too short.
            else
                threadsViewSplitContainer.Panel2Collapsed = (ThreadsViewEnabled == false);
        }

        #endregion
    }

    /// <summary>
    /// Delegate for handling the message changed event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MessageChangedEventHandler(object sender, MessageChangedEventArgs e);
}
