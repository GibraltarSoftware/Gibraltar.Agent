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

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Configures consent requirements for automatic background transmission of session data
    /// </summary>
    public class AutoSendConsentConfiguration
    {
        private readonly XmlNode m_GibraltarNode;

        /// <summary>
        /// Initialize from application configuration section
        /// </summary>
        /// <param name="configuration"></param>
        internal AutoSendConsentConfiguration(AutoSendConsentElement configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize from an xml document
        /// </summary>
        internal AutoSendConsentConfiguration(XmlNode gibraltarNode)
        {
            m_GibraltarNode = gibraltarNode;

            //create an element object so we have something to draw defaults from.
            AutoSendConsentElement baseline = new AutoSendConsentElement();
            Initialize(baseline);

            //see if we have any configuration node...
            XmlNode node = gibraltarNode.SelectSingleNode("autoSendConsent");

            //copy the provided configuration
            if (node != null)
            {
                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                ConsentDefault = AgentConfiguration.ReadValue(node, "consentDefault", baseline.ConsentDefault);
                PromptUserOnStartupLimit = AgentConfiguration.ReadValue(node, "promptUserOnStartupLimit", baseline.PromptUserOnStartupLimit);
                CompanyName = AgentConfiguration.ReadValue(node, "companyName", baseline.CompanyName);
                ServiceName = AgentConfiguration.ReadValue(node, "serviceName", baseline.ServiceName);
                PrivacyPolicyUrl = AgentConfiguration.ReadValue(node, "privacyPolicyUrl", baseline.PrivacyPolicyUrl);
            }
        }

        #region Public Properties and Methods


        /// <summary>
        /// True by default, disables gathering of auto send consent when false.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The default value for consent, used until the user indicates their selection
        /// </summary>
        /// <remarks>The user interface will always start with no value selected until the user has made a choice even if this setting is true.</remarks>
        public bool ConsentDefault { get; set; }

        /// <summary>
        /// Configures the number of times to prompt the user to make a consent decision when the application starts
        /// </summary>
        /// <remarks>The user will be presented with a dialog to make their selection.  Until they have made their selection
        /// the default opt in value will be applied.</remarks>
        public int PromptUserOnStartupLimit { get; set; }

        /// <summary>
        /// A display name for your company to use as part of the consent dialog.
        /// </summary>
        /// <remarks>This value should be the familiar name for your company, not the full legal name as it is used inline in text</remarks>
        public string CompanyName { get; set; }

        /// <summary>
        /// The full URL to the privacy policy for this data.
        /// </summary>
        /// <remarks>The Url must not contain any query parameters because the agent will automatically add parameters to reflect the current application and version information</remarks>
        public string PrivacyPolicyUrl { get; set; }


        /// <summary>
        /// A display name for the customer service that is receiving the data
        /// </summary>
        /// <remarks>This value should be something like Customer Experience Improvement Program or Customer Support that will describe 
        /// the intent of the service you offer that needs the session information</remarks>
        public string ServiceName { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            AutoSendConsentElement baseline = new AutoSendConsentElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("autoSendConsent");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "consentDefault", ConsentDefault, baseline.ConsentDefault);
            AgentConfiguration.WriteValue(newNode, "promptUserOnStartupLimit", PromptUserOnStartupLimit, baseline.PromptUserOnStartupLimit);
            AgentConfiguration.WriteValue(newNode, "companyName", CompanyName ?? string.Empty, baseline.CompanyName);
            AgentConfiguration.WriteValue(newNode, "serviceName", ServiceName ?? string.Empty, baseline.ServiceName);
            AgentConfiguration.WriteValue(newNode, "privacyPolicyUrl", PrivacyPolicyUrl ?? string.Empty, baseline.PrivacyPolicyUrl);

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
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(AutoSendConsentElement configuration)
        {
            //copy the provided configuration
            Enabled = configuration.Enabled;
            ConsentDefault = configuration.ConsentDefault;
            PromptUserOnStartupLimit = configuration.PromptUserOnStartupLimit;
            CompanyName = configuration.CompanyName;
            ServiceName = configuration.ServiceName;
            PrivacyPolicyUrl = configuration.PrivacyPolicyUrl;
        }

        #endregion
    }
}
