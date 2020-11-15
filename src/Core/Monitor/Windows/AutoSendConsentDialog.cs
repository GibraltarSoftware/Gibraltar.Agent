
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Gibraltar.Data;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows
{
    /// <summary>
    /// Presents a dialog to allow the user to opt in or out of consenting to auto sending data for the current process
    /// </summary>
    public partial class AutoSendConsentDialog : Form
    {
        private const string ProductNameToken = "{ProductName}";
        private const string CeipNameToken = "{CeipName}";
        private const string CompanyNameToken = "{CompanyName}";

        private readonly string m_ReferenceTitle;
        private readonly string m_ReferenceCaption;
        private readonly string m_ReferenceDescription;
        private readonly string m_ReferenceLinkCaption;

        private Icon m_DisplayIcon;
        private string m_ProductName;
        private string m_CompanyName;
        private string m_CeipName;
        private string m_PrivacyPolicyUrl;

        /// <summary>
        /// Create a new consent dialog.  Dialogs are single use.
        /// </summary>
        public AutoSendConsentDialog()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);

            //we want to explicitly set the OS font to several elements that should be sized that way.
            lblDescription.Font = FormTools.OSFont;
            linkPrivacyPolicy.Font = FormTools.OSFont;
            optOptIn.Font = FormTools.OSFont;
            optOptOut.Font = FormTools.OSFont;

            m_ReferenceTitle = ((Form)this).Text; //cast to avoid virtual call in constructor
            m_ReferenceCaption = lblCaption.Text;
            m_ReferenceDescription = lblDescription.Text;
            m_ReferenceLinkCaption = linkPrivacyPolicy.Text;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Get the latest consent information for the current application and display it so it can be updated.
        /// </summary>
        /// <param name="consent"></param>
        /// <param name="privacyPolicyUrl"></param>
        /// <param name="serviceName"></param>
        /// <param name="companyName"></param>
        public DialogResult UpdateConsent(AutoSendConsent consent, string privacyPolicyUrl, string serviceName, string companyName)
        {
            if (consent == null)
                throw new ArgumentNullException(nameof(consent));

            //find the right current consent 
            SessionSummary summary = Log.SessionSummary; //I like short paths

            //set all of our member values.
            m_ProductName = summary.Product;
            m_CompanyName = companyName;
            m_CeipName = serviceName;
            m_PrivacyPolicyUrl = privacyPolicyUrl;
            
            //update the text from references
            Text = PerformSubstitution(m_ReferenceTitle);
            lblCaption.Text = PerformSubstitution(m_ReferenceCaption);
            lblDescription.Text = PerformSubstitution(m_ReferenceDescription);

            //if we don't have a link url, hide the link.      
            if (string.IsNullOrEmpty(privacyPolicyUrl))
            {
                linkPrivacyPolicy.Visible = false;
            }
            else
            {
                linkPrivacyPolicy.Text = PerformSubstitution(m_ReferenceLinkCaption);
                linkPrivacyPolicy.Visible = true;
            }

            //and select the right current value.
            if (consent.SelectionMade == false)
            {
                optOptIn.Checked = false;
                optOptOut.Checked = false;
            }
            else
            {
                if (consent.OptIn)
                {
                    optOptIn.Checked = true;
                }
                else
                {
                    optOptOut.Checked = true;
                }
            }

            //set up our initial validity state
            ValidateData();

            //and display our modal dialog.
            DialogResult result = ShowDialog();
            if (result == DialogResult.OK)
            {
                //save the consent result.
                consent.SelectionMade = true;
                consent.UserPrompts = 0; //reset this counter
                consent.OptIn = optOptIn.Checked;
                consent.IncludeDetails = consent.OptIn; //at the moment we don't support distinguishing.
            }

            return result;
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //see if we can find a better icon for our parent.
            Icon parentIcon = null;

            //try to get our application icon - it'll be the most familiar to the end user.
            try
            {
                parentIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }

            //if we got it, go ahead and set it.
            if (parentIcon != null)
            {
                Icon = parentIcon;
                m_DisplayIcon = parentIcon;
                //icon is rendered in OnPaint
            }
            else
            {
                //it's always a good idea to have a form icon.
                Icon = SystemIcons.Question;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_DisplayIcon != null)
            {
                //we move the message flow layout left edge to leave room for the icon
                messageFlowLayout.Left = messageFlowLayout.Margin.Left + m_DisplayIcon.Width + messageFlowLayout.Margin.Left;
                e.Graphics.DrawIcon(m_DisplayIcon, messageFlowLayout.Margin.Left, messageFlowLayout.Margin.Top);
            }
            else
            {
                //we want to make source the flow layout is set to the right left & top.
                Point location = new Point(messageFlowLayout.Margin.Top, messageFlowLayout.Margin.Right);
                if (messageFlowLayout.Location != location) //avoid gratuitous sets.
                {
                    messageFlowLayout.Location = location;

                    //and now make sure we fill the area.
                    messageFlowLayout.Width = Width - (messageFlowLayout.Left + messageFlowLayout.Margin.Right);
                }
            }
        }


        #endregion

        #region Private Properties and methods


        private string PerformSubstitution(string original)
        {
            string returnVal = original;
            returnVal = returnVal.Replace(ProductNameToken, m_ProductName);
            returnVal = returnVal.Replace(CompanyNameToken, m_CompanyName);
            returnVal = returnVal.Replace(CeipNameToken, m_CeipName);

            return returnVal;
        }

        private void ValidateData()
        {
            bool isValid = true;

            //they must have an option to be selected to be valid.
            if ((optOptIn.Checked == false) && (optOptOut.Checked == false))
                isValid = false;

            btnSave.Enabled = isValid;
        }

        #endregion

        #region Event Handlers

        private void optOptIn_CheckedChanged(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void optOptOut_CheckedChanged(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void linkPrivacyPolicy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //we have to navigate to this link.
            if (string.IsNullOrEmpty(m_PrivacyPolicyUrl) == false)
            {
                SessionSummary summary = Log.SessionSummary;
                Version version = summary.ApplicationVersion;

                //we don't know how many parts the URL is...
                string fullUrl = string.Format("{0}?productName={1}&applicationName={2}&majorVer={3}&minorVer={4}",
                    m_PrivacyPolicyUrl, summary.Product, summary.Application, version.Major, version.Minor);

                if (version.Build >= 0)
                {
                    fullUrl += string.Format("&build={0}", version.Build);

                    if (version.Revision >= 0)
                    {
                        fullUrl += string.Format("&revisionVer={0}", version.Revision);
                    }
                }

                try
                {
                    Process.Start(fullUrl);
                }
                catch (Exception ex)
                {
                    if (!Log.SilentMode) Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, AutoSendConsent.LogCategory, "Unable to display privacy policy web page", "While attempting to display the web page an exception was thrown.\r\nComplete URL: {0}\r\nException: {1}", fullUrl, ex.Message);
                }
            }
        }

        private void messageFlowLayout_Resize(object sender, EventArgs e)
        {            
            //Adjust our height to make sure we have room for the data.
            int clientHeight =  messageFlowLayout.Top + messageFlowLayout.Height + messageFlowLayout.Margin.Bottom + buttonPanel.Height; //this is how much we need INSIDE
            Height = (Height - ClientSize.Height) + clientHeight; //the first part determines the current amount of chrome in use.
        }

        #endregion
    }
}
