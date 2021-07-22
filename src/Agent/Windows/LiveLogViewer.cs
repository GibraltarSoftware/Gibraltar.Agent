
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

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Gibraltar.Agent.Windows
{
    /// <summary>
    /// A real time viewer of log messages for the current application.
    /// </summary>
    /// <remarks>
    /// 	<para>
    ///         The LiveLogViewer control can be used to place a real-time log viewer anywhere
    ///         in your application. You can have multiple log viewers active at the same time.
    ///         Each viewer will display all of the messages of your application that are
    ///         recorded after the viewer is created. Once the number of messages in a viewer
    ///         reaches the <see cref="MaxMessages">MaxMessages</see> count, the oldest
    ///         messages will be dropped.
    ///     </para>
    /// 	<para>When a LiveLogViewer control initializes it will ensure that the Loupe
    ///     Agent is fully initialized. By default, this means the first time a viewer control
    ///     activates in your application the Agent will start monitoring for log messages from
    ///     Trace, Debug, the Console, and performance metrics for your application.</para>
    /// 	<para>Unlike the Loupe Live Viewer, the LiveLogViewer control runs on the same
    ///     thread as your user interface so it will not display updates if your user interface
    ///     is blocked in a long running background operation. The Loupe Live Viewer
    ///     (accessed through a hot key, by default Ctrl-Alt-F5) has its own threads and will
    ///     continue to display messages even if your user interface thread is tied up
    ///     performing work.</para>
    /// 	<para><strong>Status Bar Updates</strong></para>
    /// 	<para>
    ///         The LiveLogViewer is designed to integrate with your application status bar
    ///         through its <see cref="MessageChanged">MessageChanged</see> event. Subscribe to
    ///         this event and send messages to your status bar to have important application
    ///         messages be displayed in the status bar, even if the LiveLogViewer control is
    ///         hidden.
    ///     </para>
    /// </remarks>
    [DefaultProperty("UnhandledExceptionBehavior")]
    [DefaultEvent("MessageChanged")]
    public sealed partial class LiveLogViewer : UserControl
    {
        /// <summary>
        /// The default number of messages the viewer will hold at one time.
        /// </summary>
        public const int DefaultMaxMessages = Monitor.Windows.LiveLogViewer.DefaultMaxMessages;

        private event Log.MessageFilterEventHandler m_MessageFilterEvent;
        private readonly object m_MessageEventLock = new object();

        /// <summary>Create a Loupe Live Log Viewer control.</summary>
        /// <remarks>The control will only see log messages starting with its own creation.</remarks>
        public LiveLogViewer()
        {
            InitializeComponent();

            internalLogViewer.MessageChanged += LiveLogViwer_MessageChange; // So we can propagate these.
        }

        /// <summary>
        /// This event gets raised when the status message changes (most recent non-verbose caption).
        /// </summary>
        /// <remarks>The <see cref="LiveLogViewer">LiveLogViewer</see> tracks a status message for the
        /// session based on the Caption of the most recent log message whose severity is greater than
        /// <see cref="LogMessageSeverity">Verbose</see>.  This event is fired when a new message of severity
        /// <see cref="LogMessageSeverity">Information</see> or greater is received by the
        /// <see cref="LiveLogViewer">LiveLogViewer</see>, and the
        /// <see cref="MessageChangedEventArgs">MessageChangedEventArgs</see> contains will contain a
        /// <see cref="MessageChangedEventArgs.Message">Message</see> property with the new status message.
        /// </remarks>
        public event MessageChangedEventHandler MessageChanged;

        /// <summary>
        /// Raised whenever this LiveLogViewer receives a new log message, allowing the client to block it from being
        /// included for display to users.
        /// </summary>
        /// <remarks><para>Each LiveLogViewer instance filters independently with its own event.  To filter the main
        /// Loupe Live Viewer (raised via hotkey unless disabled), see the Log.LiveViewerMessageFilter event.</para>
        /// <para>The Message property of the event args provides the log message in consideration, and the Cancel property
        /// allows the message to be displayed (false, the default) or blocked (true).  The sender parameter of the event
        /// will identify the specific LiveLogViewer instance.</para></remarks>
        public event Log.MessageFilterEventHandler MessageFilter
        {
            add
            {
                if (value == null)
                    return;

                lock (m_MessageEventLock)
                {
                    if (m_MessageFilterEvent == null)
                    {
                        internalLogViewer.MessageFilter += LiveViewer_MessageFilterEvent;
                    }

                    m_MessageFilterEvent += value;
                }
            }
            remove
            {
                if (value == null)
                    return;

                lock (m_MessageEventLock)
                {
                    if (m_MessageFilterEvent == null)
                        return; // Already empty, no subscriptions to remove.

                    m_MessageFilterEvent -= value;

                    if (m_MessageFilterEvent == null)
                    {
                        internalLogViewer.MessageFilter -= LiveViewer_MessageFilterEvent;
                    }
                }
            }
        }

        #region Public Properties and Methods

        /// <summary>
        /// Specifies whether to retrieve buffered messages from before the creation of this instance.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether to retrieve buffered messages from before the creation of this instance.")]
        [DefaultValue(false)]
        public bool GetPriorMessages { get { return internalLogViewer.GetPriorMessages; } set { internalLogViewer.GetPriorMessages = value; } }

        /// <summary>
        /// Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.")]
        [DefaultValue(DefaultMaxMessages)]
        public int MaxMessages { get { return internalLogViewer.MaxMessages; } set { internalLogViewer.MaxMessages = value; } }

        /// <summary>
        /// Specifies the default value for the filter.  If not set, no messages will be filtered.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies the default value for the filter.  If not set, no messages will be filtered.")]
        [DefaultValue(LogMessageSeverity.Verbose)]
        public LogMessageSeverity DefaultFilterLevel
        {
            get { return (LogMessageSeverity)internalLogViewer.DefaultFilterLevel; }
            set { internalLogViewer.DefaultFilterLevel = (Loupe.Extensibility.Data.LogMessageSeverity)value; }
        }

        /// <summary>
        /// Specifies whether to display verbose messages in this viewer. (They will still be collected by Loupe.)
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("When false suppresses the display of verbose messages")]
        [DefaultValue(true)]
        public bool ShowVerboseMessages
        {
            get { return internalLogViewer.ShowVerboseMessages; }
            set { internalLogViewer.ShowVerboseMessages = value; }
        }

        /// <summary>
        /// Specifies whether to display developer details about threads and calling method in this viewer.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the grid includes developer details about threads and calling method")]
        [DefaultValue(true)]
        public bool ShowDetailsInGrid
        {
            get { return internalLogViewer.ShowDetailsInGrid; }
            set { internalLogViewer.ShowDetailsInGrid = value; }
        }

        /// <summary>
        /// Specifies whether tooltips include developer details about threads and calling method.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether tooltips include developer details about threads and calling method")]
        [DefaultValue(true)]
        public bool ShowDetailsInTooltips
        {
            get { return internalLogViewer.ShowDetailsInTooltips; }
            set { internalLogViewer.ShowDetailsInTooltips = value; }
        }

        /// <summary>
        /// Specifies whether the Show Details button should be visible in the toolbar.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Show Details button should be visible in the toolbar")]
        [DefaultValue(true)]
        public bool ShowDetailsButton
        {
            get { return internalLogViewer.ShowDetailsButton; }
            set { internalLogViewer.ShowDetailsButton = value; }
        }

        /// <summary>
        /// Specifies whether the Run button should display caption text next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Run button should display caption text next to the icon")]
        [DefaultValue(true)]
        public bool RunButtonTextVisible
        {
            get { return internalLogViewer.RunButtonTextVisible; }
            set { internalLogViewer.RunButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Run button.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Caption text for Run button")]
        [DefaultValue("Click to Auto Refresh")]
        public string RunButtonText
        {
            get { return internalLogViewer.RunButtonText; }
            set { internalLogViewer.RunButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Pause button should display caption text next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Pause button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool PauseButtonTextVisible
        {
            get { return internalLogViewer.PauseButtonTextVisible; }
            set { internalLogViewer.PauseButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Pause button.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Caption text for Pause button")]
        [DefaultValue("Pause")]
        public string PauseButtonText
        {
            get { return internalLogViewer.PauseButtonText; }
            set { internalLogViewer.PauseButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Search button should display caption text next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool SearchButtonTextVisible
        {
            get { return internalLogViewer.SearchButtonTextVisible; }
            set { internalLogViewer.SearchButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Search button.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Caption text for Search button")]
        [DefaultValue("Search")]
        public string SearchButtonText
        {
            get { return internalLogViewer.SearchButtonText; }
            set { internalLogViewer.SearchButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Reset Search button should display caption text next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Reset Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool ResetSearchButtonTextVisible
        {
            get { return internalLogViewer.ResetSearchButtonTextVisible; }
            set { internalLogViewer.ResetSearchButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Reset Search button
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Caption text for Reset Search button")]
        [DefaultValue("Reset")]
        public string ResetSearchButtonText
        {
            get { return internalLogViewer.ResetSearchButtonText; }
            set { internalLogViewer.ResetSearchButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Clear Search button should display caption text next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the Clear Search button should display caption text next to the icon")]
        [DefaultValue(false)]
        public bool ClearMessagesButtonTextVisible
        {
            get { return internalLogViewer.ClearMessagesButtonTextVisible; }
            set { internalLogViewer.ClearMessagesButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Clear Messages button.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Caption text for Clear Messages button")]
        [DefaultValue("Clear")]
        public string ClearMessagesButtonText
        {
            get { return internalLogViewer.ClearMessagesButtonText; }
            set { internalLogViewer.ClearMessagesButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the severity filter buttons should display message counts next to the icon.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Specifies whether the severity filter buttons should display message counts next to the icon")]
        [DefaultValue(true)]
        public bool ShowMessageCounters
        {
            get { return internalLogViewer.ShowMessageCounters; }
            set { internalLogViewer.ShowMessageCounters = value; }
        }

        /// <summary>
        /// Allows each of the message severity filter buttons to toggle independently rather than act as a minimum severity setting.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Allows each of the message severity filter buttons to toggle independently rather than act as a minimum severity setting.")]
        [DefaultValue(false)]
        public bool EnableIndependentSeverityFilters
        {
            get { return internalLogViewer.EnableIndependentSeverityFilters; }
            set { internalLogViewer.EnableIndependentSeverityFilters = value; }
        }

        /// <summary>
        /// Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy")]
        [DefaultValue(true)]
        public bool EnableMultiSelection
        {
            get { return internalLogViewer.EnableMultiSelection; }
            set { internalLogViewer.EnableMultiSelection = value; }
        }

        /// <summary>
        /// Shows or hides the built-in toolbar.
        /// </summary>
        [Browsable(true)]
        [Category("Loupe")]
        [Description("Shows or hides the built-in toolbar")]
        [DefaultValue(false)]
        public bool ShowToolBar
        {
            get { return internalLogViewer.ShowToolBar; }
            set { internalLogViewer.ShowToolBar = value; }
        }

        /// <summary>
        /// Clear this viewer's buffer of messages.
        /// </summary>
        public void Clear()
        {
            internalLogViewer.Clear();
        }

        /// <summary>
        /// Copy all of this viewer's buffer of messages into the clipboard.
        /// </summary>
        public void CopyAll()
        {
            internalLogViewer.CopyAll();
        }

        /// <summary>
        /// Copy the current selection into the clipboard.
        /// </summary>
        public void CopySelection()
        {
            internalLogViewer.CopySelection();
        }

        /// <summary>
        /// Move the viewer display to the first log message in its buffer.
        /// </summary>
        public void MoveFirst()
        {
            internalLogViewer.MoveFirst();
        }

        /// <summary>
        /// Move the viewer display to the last log message in its buffer.
        /// </summary>
        public void MoveLast()
        {
            internalLogViewer.MoveLast();
        }

        /// <summary>
        /// Turn auto-scrolling on or off, until the next interactive change.
        /// </summary>
        /// <remarks>This is not a permanent setting but a programmatic hook to change an interactive setting.  If the user
        /// clicks on the grid or scrolls back, auto-scroll is turned off automatically, and it is also turned off if the
        /// user hits the Pause button.  If the user hits the Play button, auto-scroll is activated.  This method allows
        /// an encompassing control to set an initial behavior, for example.</remarks>
        /// <param name="value">True will enable the viewer to automatically scroll new messages into view.
        /// False will leave the viewer displaying the same messages as new ones are added to the buffer.</param>
        public void SetAutoScroll(bool value)
        {
            internalLogViewer.SetAutoScroll(value);
        }

        /// <summary>
        /// Invoke the viewer's default Save operation.
        /// </summary>
        public void Save()
        {
            internalLogViewer.Save();
        }

        /// <summary>
        /// Select the viewer's entire buffer of log messages.
        /// </summary>
        public void SelectAll()
        {
            internalLogViewer.SelectAll();
        }

        #endregion

        #region Private Properties and Methods

        private void OnMessageChange(string newMessage)
        {
            MessageChangedEventHandler eventHandler = MessageChanged;
            if (eventHandler != null)
            {
                //invoke the delegate
                MessageChanged(this, new MessageChangedEventArgs(newMessage));
            }
        }

        #endregion

        #region Event Handlers

        private void LiveLogViwer_MessageChange(object sender, Monitor.Windows.MessageChangedEventArgs e)
        {
            if (e == null)
                return;

            OnMessageChange(e.Message);
        }

        private void LiveViewer_MessageFilterEvent(object sender, Monitor.MessageFilterEventArgs e)
        {
            Log.MessageFilterEventHandler eventHandler = m_MessageFilterEvent;

            if (eventHandler != null)
            {
                LogMessageFilterEventArgs eventArgs = new LogMessageFilterEventArgs(e);
                eventHandler(this, eventArgs);
            }
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
