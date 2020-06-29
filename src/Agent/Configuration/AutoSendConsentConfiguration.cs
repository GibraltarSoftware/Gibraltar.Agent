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

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// 	<para>Configures consent requirements for automatic background transmission of session data.</para>
    /// 	<para>If enabled then the server autoSendSessions option will be treated as false until the user grants consent. This is designed to allow users to opt in to
    /// sending data through an embedded user interface. Since an embedded user interface is the only method of providing consent this option only works with Windows
    /// applications.</para>
    /// 	<para>If you enable this option you will need to supply a value for <see cref="AutoSendConsentConfiguration.CompanyName">CompanyName</see>,
    /// <see cref="AutoSendConsentConfiguration.ServiceName">ServiceName</see>, and <see cref="AutoSendConsentConfiguration.PrivacyPolicyUrl">PrivacyPolicyUrl</see>. These provide display values used
    /// in the user interface.</para>
    /// 	<para>To complete the integration into your windows application you <strong>must</strong> add a call to
    /// <see cref="Log.DisplayStartupConsentDialog">Log.DisplayStartupConsentDialog</see> once your application has displayed to prompt end users to make a decision. It will only
    /// display until they make a decision or reach the maximum number of opportunities you specify in
    /// <see cref="AutoSendConsentConfiguration.PromptUserOnStartupLimit">PromptUserOnStartupLimit</see>. To allow users to change their mind after their initial opportunity to opt in
    /// or out add a menu item to call <see cref="Log.DisplayConsentDialog">Log.DisplayConsentDialog</see>.</para>
    /// </summary>
    public sealed class AutoSendConsentConfiguration
    {
        private readonly Monitor.AutoSendConsentConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the auto send consent from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal AutoSendConsentConfiguration(Monitor.AutoSendConsentConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Enables requiring auto send consent.  Defaults to false.
        /// </summary>
        /// <remarks>When false, server autoSendConsent will be used without respect to any consent.</remarks>
        public bool Enabled
        {
            get { return m_WrappedConfiguration.Enabled; }
            set { m_WrappedConfiguration.Enabled = value; }
        }

        /// <summary>
        /// The default value for consent, used until the user indicates their selection
        /// </summary>
        /// <remarks>The user interface will always start with no value selected until the user has made a choice even if this setting is true.</remarks>
        public bool ConsentDefault
        {
            get { return m_WrappedConfiguration.ConsentDefault; }
            set { m_WrappedConfiguration.ConsentDefault = value; }
        }

        /// <summary>
        /// Configures the number of times to prompt the user to make a consent decision when the application starts
        /// </summary>
        /// <remarks>The user will be presented with a dialog to make their selection.  Until they have made their selection
        /// the default opt in value will be applied.</remarks>
        public int PromptUserOnStartupLimit
        {
            get { return m_WrappedConfiguration.PromptUserOnStartupLimit; }
            set { m_WrappedConfiguration.PromptUserOnStartupLimit = value; }
        }

        /// <summary>
        /// A display name for your company to use as part of the consent dialog.
        /// </summary>
        /// <remarks>This value should be the familiar name for your company, not the full legal name as it is used inline in text</remarks>
        public string CompanyName
        {
            get { return m_WrappedConfiguration.CompanyName; }
            set { m_WrappedConfiguration.CompanyName = value; }
        }

        /// <summary>The full URL to the privacy policy for this data.</summary>
        /// <remarks>
        /// 	<para>The URL must not contain any query parameters because the agent will automatically add parameters to reflect the current application and version information
        /// as follows:</para>
        /// 	<list type="bullet">
        /// 		<item>productName: The Log.SessionSummary.ProductName field.</item>
        /// 		<item>applicationName: The Log.SessionSummary.ApplicationName field.</item>
        /// 		<item>majorVer: The first place of the version (X.0.0.0)</item>
        /// 		<item>minorVer: The second place of the version (0.X.0.0)</item>
        /// 		<item>build: The third place of the version (0.0.X.0)</item>
        /// 		<item>revisionVer: The fourth place of the version (0.0.0.X)</item>
        /// 	</list>
        /// 	<para>These values are retrieved from the current log session summary.</para>
        /// </remarks>
        /// <example>
        /// 	<u>
        /// 		<see cref="!:http://www.gibraltarsoftware.com/Support/CEIP-Privacy-Policy.aspx"/>
        /// 	</u>
        /// 	<see cref="!:http://www.GibraltarSoftware.com/"/>
        /// </example>
        public string PrivacyPolicyUrl
        {
            get { return m_WrappedConfiguration.PrivacyPolicyUrl; }
            set { m_WrappedConfiguration.PrivacyPolicyUrl = value; }
        }

        /// <summary>
        /// A display name for the customer service that is receiving the data
        /// </summary>
        /// <remarks>This value should be something like Customer Experience Improvement Program or Customer Support that will describe 
        /// the intent of the service you offer that needs the session information</remarks>
        public string ServiceName
        {
            get { return m_WrappedConfiguration.ServiceName; }
            set { m_WrappedConfiguration.ServiceName = value; }
        }

        #endregion

    }
}
