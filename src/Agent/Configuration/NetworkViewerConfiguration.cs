

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// Network Messenger Configuration
    /// </summary>
    public class NetworkViewerConfiguration
    {
        private readonly Messaging.NetworkMessengerConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the network messenger from the application configuration
        /// </summary>
        internal NetworkViewerConfiguration(Messaging.NetworkMessengerConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        /// <summary>
        /// True by default, enables connecting a viewer on the local computer when true.
        /// </summary>
        public bool AllowLocalClients
        {
            get { return m_WrappedConfiguration.AllowLocalClients; }
            set { m_WrappedConfiguration.AllowLocalClients = value; }
        }

        /// <summary>
        /// False by default, enables connecting a viewer from another computer when true.
        /// </summary>
        /// <remarks>Requires a server configuration section</remarks>
        public bool AllowRemoteClients
        {
            get { return m_WrappedConfiguration.AllowRemoteClients; }
            set { m_WrappedConfiguration.AllowRemoteClients = value; }
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be processed by the network messenger
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be processed exceeds the
        /// maximum queue length unsent messages will be dropped.</remarks>
        public int MaxQueueLength
        {
            get { return m_WrappedConfiguration.MaxQueueLength; }
            set { m_WrappedConfiguration.MaxQueueLength = value; }
        }

        /// <summary>
        /// False by default. When false, the network messenger is disabled even if otherwise configured.
        /// </summary>
        /// <remarks>This allows for explicit disable/enable without removing the existing configuration
        /// or worrying about the default configuration.</remarks>
        public bool Enabled
        {
            get { return m_WrappedConfiguration.Enabled; }
            set { m_WrappedConfiguration.Enabled = value; }
        }
    }
}
