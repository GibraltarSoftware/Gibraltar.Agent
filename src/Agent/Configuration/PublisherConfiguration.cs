
using System;

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// The configuration of the publisher.
    /// </summary>
    public sealed class PublisherConfiguration
    {
        private readonly Monitor.PublisherConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the publisher from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal PublisherConfiguration(Monitor.PublisherConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Optional.  The name of the product for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        public string ProductName
        {
            get { return m_WrappedConfiguration.ProductName; }
            set { m_WrappedConfiguration.ProductName = value; }
        }

        /// <summary>
        /// Optional.  A description of the application to include with the session information.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// application description from the assemblies that initiate logging.</remarks>
        public string ApplicationDescription
        {
            get { return m_WrappedConfiguration.ApplicationDescription; }
            set { m_WrappedConfiguration.ApplicationDescription = value; }
        }

        /// <summary>
        /// Optional.  The name of the application for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        public string ApplicationName
        {
            get { return m_WrappedConfiguration.ApplicationName; }
            set { m_WrappedConfiguration.ApplicationName = value; }
        }

        /// <summary>
        /// Optional.  The ApplicationType to treat the application as, overriding the Agent's automatic determination.
        /// </summary>
        /// <remarks>This setting is not generally necessary as the Agent will automatically determine the application
        /// type correctly in most typical windows services, console apps, WinForm applications, and ASP.NET applications.
        /// If the automatic determination is unsuccessful or incorrect with a particular application, the correct type
        /// can be configured with this setting to bypass the automatic determination.  However, setting this incorrectly
        /// for the application could have undesirable effects.</remarks>
        public ApplicationType ApplicationType // Note: Should it restrict certain detectable mismatches?
        {
            get { return (ApplicationType)m_WrappedConfiguration.ApplicationType; }
            set { m_WrappedConfiguration.ApplicationType = (Loupe.Extensibility.Data.ApplicationType)value; }
        }

        /// <summary>
        /// Optional.  The version of the application for logging purposes.
        /// </summary>
        /// <remarks><para>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</para></remarks>
        public Version ApplicationVersion
        {
            get { return m_WrappedConfiguration.ApplicationVersion; }
            set { m_WrappedConfiguration.ApplicationVersion = value; }
        }


        /// <summary>
        /// Optional.  The environment this session is running in.
        /// </summary>
        /// <remarks>Environments are useful for categorizing sessions, for example to 
        /// indicate the hosting environment. If a value is provided it will be 
        /// carried with the session data to upstream servers and clients.  If the 
        /// corresponding entry does not exist it will be automatically created.</remarks>
        public string EnvironmentName
        {
            get { return m_WrappedConfiguration.EnvironmentName; }
            set { m_WrappedConfiguration.EnvironmentName = value; }
        }

        /// <summary>
        /// Optional.  The promotion level of the session.
        /// </summary>
        /// <remarks>Promotion levels are useful for categorizing sessions, for example to 
        /// indicate whether it was run in development, staging, or production. 
        /// If a value is provided it will be carried with the session data to upstream servers and clients.  
        /// If the corresponding entry does not exist it will be automatically created.</remarks>
        public string PromotionLevelName
        {
            get { return m_WrappedConfiguration.PromotionLevelName; }
            set { m_WrappedConfiguration.PromotionLevelName = value; }
        }

        /// <summary>
        /// When true, the publisher will treat all publish requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed to every
        /// configured messenger.</remarks>
        public bool ForceSynchronous
        {
            get { return m_WrappedConfiguration.ForceSynchronous; }
            set { m_WrappedConfiguration.ForceSynchronous = value; }
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be published.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be published exceeds the
        /// maximum queue length the log publisher will switch to a synchronous mode to 
        /// catch up.  This will cause the client to block until each new message is published.</remarks>
        public int MaxQueueLength
        {
            get { return m_WrappedConfiguration.MaxQueueLength; }
            set { m_WrappedConfiguration.MaxQueueLength = value; }
        }

        /// <summary>
        /// When true, the Agent will record session data without collecting personally-identifying information.
        /// </summary>
        /// <remarks>In anonymous mode the Agent will not collect personally-identifying information such as user name,
        /// user domain name, host name, host domain name, and the application's command line.  Anonymous mode is disabled
        /// by default, and normal operation will collect this information automatically.</remarks>
        public bool EnableAnonymousMode
        {
            get { return m_WrappedConfiguration.EnableAnonymousMode; }
            set { m_WrappedConfiguration.EnableAnonymousMode = value; }
        }

        /// <summary>
        /// When true, the Agent will include debug messages in logs. Not intended for production use
        /// </summary>
        /// <remarks><para>Normally the Agent will fail silently and otherwise compensate for problems to ensure
        /// that it does not cause a problem for your application. When you are developing your application 
        /// you can enable this mode to get more detail about why th Agent is behaving as it is and resolve
        /// issues.</para>
        /// <para>In debug mode the agent may throw exceptions to indicate calling errors it normally would 
        /// just silently ignore. Therefore, this option is not recommended for consistent production use.</para></remarks>
        public bool EnableDebugMode
        {
            get { return m_WrappedConfiguration.EnableDebugMode; }
            set { m_WrappedConfiguration.EnableDebugMode = value; }
        }

        #endregion
    }
}
