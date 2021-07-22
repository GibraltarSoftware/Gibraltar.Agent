
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
    /// Configures consent requirements for automatic background transmission of session data
    /// </summary>
    public class AutoSendConsentElement : ConfigurationSection
    {
        /// <summary>
        /// Disables gathering of auto send consent when false.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = false, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// The default value for consent, used until the user indicates their selection
        /// </summary>
        /// <remarks>The user interface will always start with no value selected until the user has made a choice even if this setting is true.</remarks>
        [ConfigurationProperty("consentDefault", DefaultValue = false, IsRequired = false)]
        public bool ConsentDefault { get { return (bool)this["consentDefault"]; } set { this["consentDefault"] = value; } }

        /// <summary>
        /// Configures the number of times to prompt the user to make a consent decision when the application starts
        /// </summary>
        /// <remarks>The user will be presented with a dialog to make their selection.  Until they have made their selection
        /// the default opt in value will be applied.</remarks>
        [ConfigurationProperty("promptUserOnStartupLimit", DefaultValue = 0, IsRequired = false)]
        public int PromptUserOnStartupLimit { get { return (int)this["promptUserOnStartupLimit"]; } set { this["promptUserOnStartupLimit"] = value; } }

        /// <summary>
        /// A display name for your company to use as part of the consent dialog.
        /// </summary>
        /// <remarks>This value should be the familiar name for your company, not the full legal name as it is used inline in text</remarks>
        [ConfigurationProperty("companyName", DefaultValue = "", IsRequired = true)]
        public string CompanyName { get { return (string)this["companyName"]; } set { this["companyName"] = value; } }

        /// <summary>
        /// A display name for the customer service that is receiving the data
        /// </summary>
        /// <remarks>This value should be something like Customer Experience Improvement Program or Customer Support that will describe 
        /// the intent of the service you offer that needs the session information</remarks>
        [ConfigurationProperty("serviceName", DefaultValue = "", IsRequired = true)]
        public string ServiceName { get { return (string)this["serviceName"]; } set { this["serviceName"] = value; } }

        /// <summary>
        /// The full URL to the privacy policy for this data.
        /// </summary>
        /// <remarks>The Url must not contain any query parameters because the agent will automatically add parameters to reflect the current application and version information</remarks>
        [ConfigurationProperty("privacyPolicyUrl", DefaultValue = "", IsRequired = true)]
        public string PrivacyPolicyUrl { get { return (string)this["privacyPolicyUrl"]; } set { this["privacyPolicyUrl"] = value; } }

    }
}
