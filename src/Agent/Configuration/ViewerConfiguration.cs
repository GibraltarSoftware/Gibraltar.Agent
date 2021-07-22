

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// Configuration information for the Live Viewer.
    /// </summary>
    public sealed class ViewerConfiguration
    {
        private readonly Messaging.ViewerMessengerConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the viewer from the application configuration.
        /// </summary>
        /// <param name="configuration"></param>
        internal ViewerConfiguration(Messaging.ViewerMessengerConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The default HotKey configuration string.
        /// </summary>
        public const string DefaultHotKey = ViewerElement.DefaultHotKey;

        /// <summary>
        /// The key sequence used to pop up the live viewer.
        /// </summary>
        public string HotKey
        {
            get { return m_WrappedConfiguration.HotKey; }
            set { m_WrappedConfiguration.HotKey = value; }
        }

        /// <summary>
        /// Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.
        /// </summary>
        public int MaxMessages
        {
            get { return m_WrappedConfiguration.MaxMessages; }
            set { m_WrappedConfiguration.MaxMessages = value; }
        }

        /// <summary>
        /// If false Suppresses the display of verbose messages.
        /// </summary>
        public bool ShowVerboseMessages
        {
            get { return m_WrappedConfiguration.ShowVerboseMessages; }
            set { m_WrappedConfiguration.ShowVerboseMessages = value; }
        }

        /// <summary>
        /// Caption for the live viewer form.
        /// </summary>
        public string FormTitleText
        {
            get { return m_WrappedConfiguration.FormTitleText; }
            set { m_WrappedConfiguration.FormTitleText = value; }
        }

        /// <summary>
        /// Specifies the default value for the filter.  If not set, no messages will be filtered.
        /// </summary>
        public LogMessageSeverity DefaultFilterLevel
        {
            get { return (LogMessageSeverity)m_WrappedConfiguration.DefaultFilterLevel; }
            set { m_WrappedConfiguration.DefaultFilterLevel = (Loupe.Extensibility.Data.LogMessageSeverity)value; }
        }

        /// <summary>
        /// Causes each of the message severity filter buttons to operate independently like in Visual Studio.
        /// </summary>
        public bool EnableIndependentSeverityFilters
        {
            get { return m_WrappedConfiguration.EnableIndependentSeverityFilters; }
            set { m_WrappedConfiguration.EnableIndependentSeverityFilters = value; }
        }

        /// <summary>
        /// Specifies whether the Show Details button should be visible in the toolbar.
        /// </summary>
        public bool ShowDetailsButton
        {
            get { return m_WrappedConfiguration.ShowDetailsButton; }
            set { m_WrappedConfiguration.ShowDetailsButton = value; }
        }

        /// <summary>
        /// Specifies whether the grid includes developer details about threads and calling method.
        /// </summary>
        public bool ShowDetailsInGrid
        {
            get { return m_WrappedConfiguration.ShowDetailsInGrid; }
            set { m_WrappedConfiguration.ShowDetailsInGrid = value; }
        }

        /// <summary>
        /// Specifies whether tooltips include developer details about threads and calling method.
        /// </summary>
        public bool ShowDetailsInTooltips
        {
            get { return m_WrappedConfiguration.ShowDetailsInTooltips; }
            set { m_WrappedConfiguration.ShowDetailsInTooltips = value; }
        }

        /// <summary>
        /// Specifies whether the severity filter buttons should display message counts next to the icon.
        /// </summary>
        public bool ShowMessageCounters
        {
            get { return m_WrappedConfiguration.ShowMessageCounters; }
            set { m_WrappedConfiguration.ShowMessageCounters = value; }
        }

        /// <summary>
        /// Caption text for Run button.
        /// </summary>
        public string RunButtonText
        {
            get { return m_WrappedConfiguration.RunButtonText; }
            set { m_WrappedConfiguration.RunButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Run button should display caption text next to the icon.
        /// </summary>
        public bool RunButtonTextVisible
        {
            get { return m_WrappedConfiguration.RunButtonTextVisible; }
            set { m_WrappedConfiguration.RunButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Pause button.
        /// </summary>
        public string PauseButtonText
        {
            get { return m_WrappedConfiguration.PauseButtonText; }
            set { m_WrappedConfiguration.PauseButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Pause button should display caption text next to the icon.
        /// </summary>
        public bool PauseButtonTextVisible
        {
            get { return m_WrappedConfiguration.PauseButtonTextVisible; }
            set { m_WrappedConfiguration.PauseButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Reset Search button.
        /// </summary>
        public string ResetSearchButtonText
        {
            get { return m_WrappedConfiguration.ResetSearchButtonText; }
            set { m_WrappedConfiguration.ResetSearchButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Reset Search button should display caption text next to the icon.
        /// </summary>
        public bool ResetSearchButtonTextVisible
        {
            get { return m_WrappedConfiguration.ResetSearchButtonTextVisible; }
            set { m_WrappedConfiguration.ResetSearchButtonTextVisible = value; }
        }

        /// <summary>
        /// Caption text for Clear Messages button.
        /// </summary>
        public string ClearMessagesButtonText
        {
            get { return m_WrappedConfiguration.ClearMessagesButtonText; }
            set { m_WrappedConfiguration.ClearMessagesButtonText = value; }
        }

        /// <summary>
        /// Specifies whether the Clear Messages button should display caption text next to the icon.
        /// </summary>
        public bool ClearMessagesButtonTextVisible
        {
            get { return m_WrappedConfiguration.ClearMessagesButtonTextVisible; }
            set { m_WrappedConfiguration.ClearMessagesButtonTextVisible = value; }
        }

        /// <summary>
        /// Shows or hides the built-in Tool Bar.
        /// </summary>
        public bool ShowToolBar
        {
            get { return m_WrappedConfiguration.ShowToolBar; }
            set { m_WrappedConfiguration.ShowToolBar = value; }
        }

        /// <summary>
        /// Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy.
        /// </summary>
        public bool EnableMultiSelection
        {
            get { return m_WrappedConfiguration.EnableMultiSelection; }
            set { m_WrappedConfiguration.EnableMultiSelection = value; }
        }

        /// <summary>
        /// When true, the viewer will treat all write requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed.</remarks>
        public bool ForceSynchronous
        {
            get { return m_WrappedConfiguration.ForceSynchronous; }
            set { m_WrappedConfiguration.ForceSynchronous = value; }
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be processed by the viewer.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be processed exceeds the
        /// maximum queue length the viewer will switch to a synchronous mode to 
        /// catch up.  This will not cause the application to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        public int MaxQueueLength
        {
            get { return m_WrappedConfiguration.MaxQueueLength; }
            set { m_WrappedConfiguration.MaxQueueLength = value; }
        }

        /// <summary>
        /// When false, the viewer is disabled even if otherwise configured.
        /// </summary>
        /// <remarks>This allows for explicit disable/enable without removing the existing configuration
        /// or worrying about the default configuration.  Disabling this will disable both the live log
        /// viewer and any log viewer control instance will never get data.</remarks>
        public bool Enabled
        {
            get { return m_WrappedConfiguration.Enabled; }
            set { m_WrappedConfiguration.Enabled = value; }
        }

        #endregion

    }
}
