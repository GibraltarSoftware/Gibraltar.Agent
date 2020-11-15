
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
using System.Configuration;

#endregion File Header

namespace Gibraltar.Agent
{
    /// <summary>
    /// Live session data stream configuration
    /// </summary>
    public class LiveStreamElement : ConfigurationSection
    {
        /// <summary>
        /// The default port for agents to connect on
        /// </summary>
        public const int DefaultAgentPort = 29971;

        /// <summary>
        /// The default port for clients (e.g. Analyst) to connect on
        /// </summary>
        public const int DefaultClientPort = 29970;

        #region Public Properties and Methods

        /// <summary>
        /// Indicates if live stream is available at all in the agent.  Defaults to true.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = false, IsRequired = false)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// The port number to listen for inbound agent connections on
        /// </summary>
        [ConfigurationProperty("agentPort", DefaultValue = DefaultAgentPort, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int AgentPort
        {
            get { return (int)this["agentPort"]; }
            set { this["agentPort"] = value; }
        }

        /// <summary>
        /// The port number to listen for inbound Analyst/other client connections on
        /// </summary>
        [ConfigurationProperty("clientPort", DefaultValue = DefaultClientPort, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int ClientPort
        {
            get { return (int)this["clientPort"]; }
            set { this["clientPort"] = value; }
        }

        /// <summary>
        /// Indicates if the connection should be encrypted with Ssl. 
        /// </summary>
        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public bool UseSsl
        {
            get
            {
                return (bool)this["useSsl"];
            }
            set
            {
                this["useSsl"] = value;
            }
        }

        /// <summary>
        /// The name of the SSL Certificate to use if Ssl is enabled.
        /// </summary>
        [ConfigurationProperty("certificateName", DefaultValue = "", IsRequired = false)]
        public string CertificateName
        {
            get
            {
                return (string)this["certificateName"];
            }
            set
            {
                this["certificateName"] = value;
            }
        }

        #endregion
    }
}
