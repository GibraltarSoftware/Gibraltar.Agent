using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The application configuration information for viewing the session over a network connection
    /// </summary>
    public class NetworkViewerElement : LoupeElementBase
    {
        /// <inheritdoc />
        public NetworkViewerElement() : base("LOUPE__NETWORKVIEWER__")
        {

        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "enabled");
            LoadEnvironmentVariable(environmentVars, "allowLocalClients");
            LoadEnvironmentVariable(environmentVars, "allowRemoteClients");
            LoadEnvironmentVariable(environmentVars, "maxQueueLength");
        }

        /// <summary>
        /// False by default, enables connecting a viewer remotely over a network when true.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled 
        { 
            get => ReadBoolean("enabled");
            set => this["enabled"] = value;
        }

        /// <summary>
        /// True by default, enables connecting a viewer on the local computer when true.
        /// </summary>
        [ConfigurationProperty("allowLocalClients", DefaultValue = true, IsRequired = false)]
        public bool AllowLocalClients 
        { 
            get => ReadBoolean("allowLocalClients");
            set => this["allowLocalClients"] = value;
        }

        /// <summary>
        /// False by default, enables connecting a viewer from another computer when true.
        /// </summary>
        /// <remarks>Requires a server configuration section</remarks>
        [ConfigurationProperty("allowRemoteClients", DefaultValue = false, IsRequired = false)]
        public bool AllowRemoteClients 
        { 
            get => ReadBoolean("allowRemoteClients");
            set => this["allowRemoteClients"] = value;
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be dispatched to viewers.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be written exceeds the
        /// maximum queue length the log writer will switch to a synchronous mode to 
        /// catch up.  This will not cause the client to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        [ConfigurationProperty("maxQueueLength", DefaultValue = 2000, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 50000)]
        public int MaxQueueLength 
        { 
            get => ReadInt("maxQueueLength");
            set => this["maxQueueLength"] = value;
        }
    }
}
