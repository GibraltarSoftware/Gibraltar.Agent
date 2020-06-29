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
using System.Configuration;
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Monitor;

#endregion File Header

namespace Gibraltar.Messaging
{
    /// <summary>
    /// Network Messenger Configuration
    /// </summary>
    public class NetworkMessengerConfiguration : MessengerConfiguration
    {
        /// <summary>
        /// Initialize the network messenger from the application configuration
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        internal NetworkMessengerConfiguration(string name, NetworkViewerElement configuration)
            : base(name, typeof(NetworkMessenger).AssemblyQualifiedName)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the network messenger from an XML document
        /// </summary>
        internal NetworkMessengerConfiguration(string name, XmlNode gibraltarNode)
            : base(name, typeof(NetworkMessenger).AssemblyQualifiedName)
        {
            //create an element object so we have something to draw defaults from.
            var baseline = new NetworkViewerElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            var node = gibraltarNode.SelectSingleNode("networkViewer");

            //copy the provided configuration
            if (node != null)
            {
                AllowLocalClients = AgentConfiguration.ReadValue(node, "allowLocalClients", baseline.AllowLocalClients);
                AllowRemoteClients = AgentConfiguration.ReadValue(node, "allowRemoteClients", baseline.AllowRemoteClients);
                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                MaxQueueLength = AgentConfiguration.ReadValue(node, "maxQueueLength", baseline.MaxQueueLength);
            }

            Sanitize();
        }

        /// <summary>
        /// True by default, enables connecting a viewer on the local computer when true.
        /// </summary>
        public bool AllowLocalClients { get; set; }

        /// <summary>
        /// False by default, enables connecting a viewer from another computer when true.
        /// </summary>
        /// <remarks>Requires a server configuration section</remarks>
        public bool AllowRemoteClients { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            var baseline = new NetworkViewerElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("sessionFile");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "maxQueueLength", MaxQueueLength, baseline.MaxQueueLength);
            AgentConfiguration.WriteValue(newNode, "allowLocalClients", AllowLocalClients, baseline.AllowLocalClients);
            AgentConfiguration.WriteValue(newNode, "allowRemoteClients", AllowRemoteClients, baseline.AllowRemoteClients);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes?.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (MaxQueueLength <= 0)
                MaxQueueLength = 2000;
            else if (MaxQueueLength > 50000)
                MaxQueueLength = 50000;
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(NetworkViewerElement configuration)
        {
            //copy the configuration from the network viewer element
            AllowLocalClients = configuration.AllowLocalClients;
            AllowRemoteClients = configuration.AllowRemoteClients;

            //and set our base stuff
            Enabled = configuration.Enabled;
            MaxQueueLength = configuration.MaxQueueLength;
        }

        #endregion
    }
}
