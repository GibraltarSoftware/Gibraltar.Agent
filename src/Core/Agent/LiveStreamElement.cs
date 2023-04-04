
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// Live session data stream configuration
    /// </summary>
    public class LiveStreamElement : LoupeElementBase
    {
        /// <inheritdoc />
        public LiveStreamElement() : base("LOUPE__LIVESTREAM__")
        {

        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "enabled");
            LoadEnvironmentVariable(environmentVars, "agentPort");
            LoadEnvironmentVariable(environmentVars, "clientPort");
            LoadEnvironmentVariable(environmentVars, "useSsl");
            LoadEnvironmentVariable(environmentVars, "certificateName");
        }


        /// <summary>
        /// The default port for agents to connect on
        /// </summary>
        public const int DefaultAgentPort = 29971;

        /// <summary>
        /// The default port for clients (e.g. Analyst) to connect on
        /// </summary>
        public const int DefaultClientPort = 29970;

        /// <summary>
        /// Indicates if live stream is available at all in the agent.  Defaults to true.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = false, IsRequired = false)]
        public bool Enabled
        {
            get => ReadBoolean("enabled");
            set => this["enabled"] = value;
        }

        /// <summary>
        /// The port number to listen for inbound agent connections on
        /// </summary>
        [ConfigurationProperty("agentPort", DefaultValue = DefaultAgentPort, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int AgentPort
        {
            get => ReadInt("agentPort");
            set => this["agentPort"] = value;
        }

        /// <summary>
        /// The port number to listen for inbound Analyst/other client connections on
        /// </summary>
        [ConfigurationProperty("clientPort", DefaultValue = DefaultClientPort, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int ClientPort
        {
            get => ReadInt("clientPort");
            set => this["clientPort"] = value;
        }

        /// <summary>
        /// Indicates if the connection should be encrypted with Ssl. 
        /// </summary>
        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public bool UseSsl
        {
            get => ReadBoolean("useSsl");
            set => this["useSsl"] = value;
        }

        /// <summary>
        /// The name of the SSL Certificate to use if Ssl is enabled.
        /// </summary>
        [ConfigurationProperty("certificateName", DefaultValue = "", IsRequired = false)]
        public string CertificateName
        {
            get => ReadString("certificateName");
            set => this["certificateName"] = value;
        }
    }
}
