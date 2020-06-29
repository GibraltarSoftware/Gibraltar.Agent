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
using System.Text;
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Monitor;

#endregion File Header

namespace Gibraltar.Messaging
{
    /// <summary>
    /// Configuration information for server live communications
    /// </summary>
    public class LiveStreamConfiguration
    {
        /// <summary>
        /// Create a new live stream configuration from the xml live stream configuration
        /// </summary>
        /// <param name="configuration"></param>
        public LiveStreamConfiguration(LiveStreamElement configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Initialize(configuration);
        }

        /// <summary>
        /// Create a new live stream configuration from the provided Xml object
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public LiveStreamConfiguration(XmlNode gibraltarNode)
        {
            //create an element object so we have something to draw defaults from.
            LiveStreamElement baseline = new LiveStreamElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("liveStream");

            //copy the provided configuration
            if (node != null)
            {
                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                AgentPort = AgentConfiguration.ReadValue(node, "agentPort", baseline.AgentPort);
                ClientPort = AgentConfiguration.ReadValue(node, "clientPort", baseline.ClientPort);
                UseSsl = AgentConfiguration.ReadValue(node, "useSsl", baseline.UseSsl);
                CertificateName = AgentConfiguration.ReadValue(node, "certificateName", baseline.CertificateName);
            }
        }

        #region Public Properties and Methods

        /// <summary>
        /// Indicates if notifications will be sent for a server issue.  Defaults to true.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The port number to listen for inbound agent connections on
        /// </summary>
        public int AgentPort { get; set; }

        /// <summary>
        /// The port number to listen for inbound Analyst/other client connections on
        /// </summary>
        public int ClientPort { get; set; }

        /// <summary>
        /// Indicates if the connection should be encrypted with Ssl. 
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// The name of the SSL Certificate to use if Ssl is enabled.
        /// </summary>
        public string CertificateName { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            LiveStreamElement baseline = new LiveStreamElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("liveStream");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "agentPort", AgentPort, baseline.AgentPort);
            AgentConfiguration.WriteValue(newNode, "clientPort", ClientPort, baseline.ClientPort);
            AgentConfiguration.WriteValue(newNode, "useSsl", UseSsl, baseline.UseSsl);
            AgentConfiguration.WriteValue(newNode, "certificateName", CertificateName, baseline.CertificateName);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        /// <summary>
        /// Clone the provided configuration
        /// </summary>
        /// <param name="configuration"></param>
        public void Load(LiveStreamConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Initialize(configuration);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            //write out our configuration..

            stringBuilder.AppendLine("Live Stream Configuration:");
            stringBuilder.AppendFormat("  Enabled: {0}\r\n", Enabled);
            stringBuilder.AppendFormat("  Agent Port: {0}\r\n", AgentPort);
            stringBuilder.AppendFormat("  Client Port: {0}\r\n", ClientPort);
            stringBuilder.AppendFormat("  Use SSL: {0}\r\n", UseSsl);
            stringBuilder.AppendFormat("  Certificate Name: {0}\r\n", CertificateName);

            return stringBuilder.ToString();
        }

        #endregion

        #region Internal Properties and Methods

        /// <summary>
        /// Rationalize values to consistent, sane items
        /// </summary>
        public void Sanitize()
        {
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(LiveStreamElement configuration)
        {
            //copy the provided configuration
            Enabled = configuration.Enabled;
            AgentPort = configuration.AgentPort;
            ClientPort = configuration.ClientPort;
            UseSsl = configuration.UseSsl;
            CertificateName = configuration.CertificateName;
        }

        private void Initialize(LiveStreamConfiguration configuration)
        {
            //copy the provided configuration
            Enabled = configuration.Enabled;
            AgentPort = configuration.AgentPort;
            ClientPort = configuration.ClientPort;
            UseSsl = configuration.UseSsl;
            CertificateName = configuration.CertificateName;
        }

        #endregion
    }
}
