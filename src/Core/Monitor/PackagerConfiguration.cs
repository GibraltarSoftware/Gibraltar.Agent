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
using System.Xml;
using Gibraltar.Agent;

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// The configuration of the packager.
    /// </summary>
    public class PackagerConfiguration
    {
        /// <summary>
        /// Initialize the packager configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal PackagerConfiguration(PackagerElement configuration)
        {
            //copy the configuration
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the packager configuration from an XML document
        /// </summary>
        internal PackagerConfiguration(XmlNode gibraltarNode)
        {
            //create an element object so we have something to draw defaults from.
            PackagerElement baseline = new PackagerElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("packager");

            //copy the provided configuration
            if (node != null)
            {
                HotKey = AgentConfiguration.ReadValue(node, "hotKey", baseline.HotKey);
                AllowEmail = AgentConfiguration.ReadValue(node, "allowEmail", baseline.AllowEmail);
                AllowFile = AgentConfiguration.ReadValue(node, "allowFile", baseline.AllowFile);
                AllowRemovableMedia = AgentConfiguration.ReadValue(node, "allowRemovableMedia", baseline.AllowRemovableMedia);
                AllowServer = AgentConfiguration.ReadValue(node, "allowServer", baseline.AllowServer);
                ApplicationName = AgentConfiguration.ReadValue(node, "applicationName", baseline.ApplicationName);
                DestinationEmailAddress = AgentConfiguration.ReadValue(node, "destinationEmailAddress", baseline.DestinationEmailAddress);
                FromEmailAddress = AgentConfiguration.ReadValue(node, "fromEmailAddress", baseline.FromEmailAddress);
                ProductName = AgentConfiguration.ReadValue(node, "productName", baseline.ProductName);
            }
        }

        #region Public Properties and Methods

        /// <summary>
        /// The default HotKey configuration string for the packager.
        /// </summary>
        public const string DefaultHotKey = PackagerElement.DefaultHotKey;

        /// <summary>
        /// The key sequence used to pop up the packager.
        /// </summary>
        public string HotKey { get; set; }

        /// <summary>
        /// When true the user will be allowed to save the package to a file.
        /// </summary>
        public bool AllowFile { get; set; }

        /// <summary>
        /// When true the user will be allowed to save the package directly to the root of a removable media volume
        /// </summary>
        public bool AllowRemovableMedia { get; set; }

        /// <summary>
        /// When true the user will be allowed to send sessions to a session data server
        /// </summary>
        public bool AllowServer { get; set; }

        /// <summary>
        /// When true the user will be allowed to send the package via email
        /// </summary>
        public bool AllowEmail { get; set; }

        /// <summary>
        /// The email address to use as the sender&apos;s address
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        public string FromEmailAddress { get; set; }

        /// <summary>
        /// The address to send the email to.
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        public string DestinationEmailAddress { get; set; }

        /// <summary>
        /// The product name to use instead of the current application.
        /// </summary>
        /// <remarks>Primarily used in the Packager.exe.config file to specify the end-user product and application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Gibraltar for the product.
        /// <para>To limit the package to one application within a product specify the applicationName as well
        /// as the productName.  Specifying just the product name will cause the package to contain all applications
        /// for the specified product.</para></remarks>
        public string ProductName { get; set; }

        /// <summary>
        /// The application name to use instead of the current application.
        /// </summary>
        /// <remarks><para>Primarily used in the Packager.exe.config file to specify the end-user application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Gibraltar for the application.</para>
        /// <para>Application name is ignored if product name is not also specified.</para></remarks>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            PackagerElement baseline = new PackagerElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("packager");
            AgentConfiguration.WriteValue(newNode, "hotKey", HotKey ?? string.Empty, baseline.HotKey);
            AgentConfiguration.WriteValue(newNode, "productName", ProductName ?? string.Empty, baseline.ProductName);
            AgentConfiguration.WriteValue(newNode, "applicationName", ApplicationName ?? string.Empty, baseline.ApplicationName);
            AgentConfiguration.WriteValue(newNode, "allowEmail", AllowEmail, baseline.AllowEmail);
            AgentConfiguration.WriteValue(newNode, "allowFile", AllowFile, baseline.AllowFile);
            AgentConfiguration.WriteValue(newNode, "allowRemovableMedia", AllowRemovableMedia, baseline.AllowRemovableMedia);
            AgentConfiguration.WriteValue(newNode, "allowServer", AllowServer, baseline.AllowServer);
            AgentConfiguration.WriteValue(newNode, "destinationEmailAddress", DestinationEmailAddress ?? string.Empty, baseline.DestinationEmailAddress);
            AgentConfiguration.WriteValue(newNode, "fromEmailAddress", FromEmailAddress ?? string.Empty, baseline.FromEmailAddress);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        #endregion

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (string.IsNullOrEmpty(HotKey))
                HotKey = DefaultHotKey;

            if (string.IsNullOrEmpty(ProductName))
                ProductName = null;

            if (string.IsNullOrEmpty(ApplicationName))
                ApplicationName = null;

            if (string.IsNullOrEmpty(FromEmailAddress))
                FromEmailAddress = null;

            if (string.IsNullOrEmpty(DestinationEmailAddress))
                DestinationEmailAddress = null;
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(PackagerElement configuration)
        {
            //copy the configuration
            HotKey = configuration.HotKey;
            AllowEmail = configuration.AllowEmail;
            AllowFile = configuration.AllowFile;
            AllowRemovableMedia = configuration.AllowRemovableMedia;
            AllowServer = configuration.AllowServer;
            ApplicationName = configuration.ApplicationName;
            DestinationEmailAddress = configuration.DestinationEmailAddress;
            FromEmailAddress = configuration.FromEmailAddress;
            ProductName = configuration.ProductName;            
        }

        #endregion

    }
}
