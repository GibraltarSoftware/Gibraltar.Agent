
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

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Email server connection configuration.
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Initialize the email configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        public EmailConfiguration(EmailElement configuration)
        {
            //copy the configuration
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the email configuration from an XML document
        /// </summary>
        public EmailConfiguration(XmlNode gibraltarNode)
        {
            //create an element object so we have something to draw defaults from.
            EmailElement baseline = new EmailElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("email");

            //copy the provided configuration
            if (node != null)
            {
                Server = AgentConfiguration.ReadValue(node, "server", baseline.Server);
                Port = AgentConfiguration.ReadValue(node, "port", baseline.Port);
                UseSsl = AgentConfiguration.ReadValue(node, "useSsl", baseline.UseSsl);
                User = AgentConfiguration.ReadValue(node, "user", baseline.User);
                Password = AgentConfiguration.ReadValue(node, "password", baseline.Password);
                MaxMessageSize = AgentConfiguration.ReadValue(node, "maxMessageSize", baseline.MaxMessageSize);
                FromAddress = AgentConfiguration.ReadValue(node, "fromAddress", baseline.FromAddress);
            }
        }

        #region Public Properties and Methods

        /// <summary>
        /// The full DNS name of the server to use for exchanging email
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Optional.  The user to authenticate to the server with.  If provided a password is required.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Optional.  The password to authenticate to the server with.  Only used if a user is provided.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Optional.  The TCP/IP port to connect to the server on.  If not specified the default (25) will be used.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// When true any email will be encrypted using SSL.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Optional.  The maximum size an email message can be when submitted to this server.  If not specified then 10MB will be assumed.
        /// </summary>
        public int MaxMessageSize { get; set; }

        /// <summary>
        /// Optional.  A default from email address to use for messages that don't specify an address.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            EmailElement baseline = new EmailElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("email");
            AgentConfiguration.WriteValue(newNode, "server", Server ?? string.Empty, baseline.Server);
            AgentConfiguration.WriteValue(newNode, "port", Port, baseline.Port);
            AgentConfiguration.WriteValue(newNode, "useSsl", UseSsl, baseline.UseSsl);
            AgentConfiguration.WriteValue(newNode, "user", User ?? string.Empty, baseline.User);
            AgentConfiguration.WriteValue(newNode, "password", Password ?? string.Empty, baseline.Password);
            AgentConfiguration.WriteValue(newNode, "maxMessageSize", MaxMessageSize, baseline.MaxMessageSize);
            AgentConfiguration.WriteValue(newNode, "fromAddress", FromAddress, baseline.FromAddress);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        /// <summary>
        /// Clone the provided configuration object
        /// </summary>
        /// <param name="configuration"></param>
        public void Load(EmailConfiguration configuration)
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

            stringBuilder.AppendLine("Mail Server Connection Configuration:");
            stringBuilder.AppendFormat("  Server: {0} Port: {1} Use SSL:{2}\r\n", Server, Port, UseSsl);

            if (string.IsNullOrEmpty(Password) == false)
            {
                stringBuilder.AppendFormat("  User: {0} Password: (Protected Value)\r\n", User);
            }

            stringBuilder.AppendFormat("  Maximum Message Size: {0}\r\n", MaxMessageSize);

            return stringBuilder.ToString();
        }

        #endregion

        #region Internal Properties and Methods

        /// <summary>
        /// Rationalize values to consistent, sane items
        /// </summary>
        public void Sanitize()
        {
            if (string.IsNullOrEmpty(Server))
                Server = null;

            if (string.IsNullOrEmpty(User))
                User = null;

            if (string.IsNullOrEmpty(Password))
                Password = null;

            if (MaxMessageSize <= 0)
                MaxMessageSize = 10;
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(EmailElement configuration)
        {
            //copy the configuration
            Server = configuration.Server;
            User = configuration.User;
            Password = configuration.Password;
            Port = configuration.Port;
            UseSsl = configuration.UseSsl;
            MaxMessageSize = configuration.MaxMessageSize;
            FromAddress = configuration.FromAddress;
        }

        private void Initialize(EmailConfiguration configuration)
        {
            //copy the configuration
            Server = configuration.Server;
            User = configuration.User;
            Password = configuration.Password;
            Port = configuration.Port;
            UseSsl = configuration.UseSsl;
            MaxMessageSize = configuration.MaxMessageSize;
            FromAddress = configuration.FromAddress;
        }

        #endregion
    }
}
