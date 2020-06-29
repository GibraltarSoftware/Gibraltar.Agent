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
using System.Windows.Forms;
using Gibraltar.Data;
using Gibraltar.Windows.UI;

#endregion File Header

namespace Gibraltar.Monitor.Windows.Internal
{
    internal partial class UIPackagerCriteria : UserControl, IUIWizardInputPage
    {
        private const string m_Title = "Pick Sessions to Send";
        private const string m_Instructions = "Select what sessions should be included in this package.";

        private readonly PackagerConfiguration m_Configuration;
        private PackagerRequest m_Request;
        private bool m_NextEnabled;
        private bool m_AllowActiveSession;

        public UIPackagerCriteria(PackagerConfiguration configuration)
        {
            m_Configuration = configuration;

            InitializeComponent();

            FormTools.ApplyOSFont(this);
        }

        #region Public Properties and Methods

#pragma warning disable 0067
        /// <summary>
        /// Raised whenever the IsValid property changes
        /// </summary>
        public event EventHandler IsValidChanged;

#pragma warning restore 0067


        /// <summary>
        /// Initialize the page to start a new wizard working with the provided request data.
        /// </summary>
        /// <param name="requestData"></param>
        public void Initialize(object requestData)
        {
            m_Request = (PackagerRequest)requestData;
            m_NextEnabled = false; // a safe default value

            //if the request doesn't cover this app we need to change the options we display
            if (m_Request.CoversCurrentApplication == false)
            {
                m_AllowActiveSession = false;
                optJustThisSession.Visible = false;
                lblJustThisSession.Visible = false;
                runningAppNotePanel.Visible = true;

                runningAppNotePanel.Width = mainLayoutPanel.Width - (mainLayoutPanel.Margin.Left) - mainLayoutPanel.Margin.Right;
            }
            else
            {
                m_AllowActiveSession = true;
                optJustThisSession.Visible = true;
                lblJustThisSession.Visible = true;
                runningAppNotePanel.Visible = false;
            }
        }

        /// <summary>
        /// The end-user title for this page in the wizard.
        /// </summary>
        public string Title { get { return m_Title; } }

        /// <summary>
        /// End user instructions for completing this page of the wizard.
        /// </summary>
        public string Instructions { get { return m_Instructions; } }

        /// <summary>
        /// Query if the wizard page allows moving next.
        /// </summary>
        public bool IsValid { get { return m_NextEnabled; } }

        /// <summary>
        /// The user has requested to move next
        /// </summary>
        public bool Store()
        {
            // commit the changes the user has made
            if (optAllSessions.Checked)
            {
                m_Request.Criteria = m_AllowActiveSession ? SessionCriteria.ActiveSession | SessionCriteria.AllSessions : SessionCriteria.AllSessions;
            }
            else if (optJustThisSession.Checked)
            {
                m_Request.Criteria = SessionCriteria.ActiveSession;
            }
            else if (optNewSessions.Checked)
            {
                m_Request.Criteria = m_AllowActiveSession ? SessionCriteria.ActiveSession | SessionCriteria.NewSessions : SessionCriteria.NewSessions;
            }

            return true;
        }

        /// <summary>
        /// The wizard controller is entering this page.
        /// </summary>
        public void OnEnter()
        {
            //what option is the closest match to what's selected?
            if ((m_Request.Criteria & SessionCriteria.AllSessions) == SessionCriteria.AllSessions)
            {
                optAllSessions.Checked = true;
            }
            else if ((m_Request.Criteria & SessionCriteria.NewSessions) == SessionCriteria.NewSessions)
            {
                optNewSessions.Checked = true;
            }
            else if (((m_Request.Criteria & SessionCriteria.ActiveSession) == SessionCriteria.ActiveSession) 
                && optJustThisSession.Visible)
            {
                optJustThisSession.Checked = true;
            }
            else
            {
                //nothing picked yet - go for our default.
                optNewSessions.Checked = true;
                optAllSessions.Checked = false;
                optJustThisSession.Checked = false;
            }

            ValidateData();
        }

        #endregion

        #region Protected properties and Methods

        protected void OnIsValidChanged()
        {
            EventHandler tempEvent = IsValidChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Properties and Methods

        private void ValidateData()
        {
            bool valid = true;

            //one option must be selected
            if ((optAllSessions.Checked == false) && (optJustThisSession.Checked == false) && (optNewSessions.Checked == false))
            {
                valid = false;
            }

            SetNextEnabled(valid);
        }

        private void SetNextEnabled(bool enabled)
        {
            if (enabled == m_NextEnabled)
                return;

            m_NextEnabled = enabled;

            OnIsValidChanged();
        }

        #endregion

        #region Event Handlers

        private void optJustThisSession_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ValidateData();
        }

        private void optNewSessions_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ValidateData();
        }

        private void optAllSessions_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ValidateData();
        }

        #endregion
    }
}
