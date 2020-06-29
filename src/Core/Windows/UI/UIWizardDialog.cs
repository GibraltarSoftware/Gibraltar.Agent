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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Windows.Internal;
using Gibraltar.Windows.UI;
using Gibraltar.Windows.UI.Internal;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// A standard wizard framework and dialog.
    /// </summary>
    /// <remarks>This class is not used directly, instead individual wizards inherit from it to create
    /// their specific implementations.</remarks>
    public partial class UIWizardDialog : Form
    {
        /// <summary>
        /// The standard text for the finish button
        /// </summary>
        protected const string FinishText = "&Finish";

        /// <summary>
        /// The standard text for the next button
        /// </summary>
        protected const string NextText = "&Next >";

        /// <summary>
        /// The standard text for the close button
        /// </summary>
        protected const string CloseText = "&Close";

        private readonly WizardEngine m_WizardEngine;
        private readonly string m_Title;

        private object m_Request;
        private UserControl m_DisplayedPage;
        private bool m_CloseOnNext;

        /// <summary>
        /// Designer-required parameterless constructor.
        /// </summary>
        [Obsolete] //so we don't accidentally use it
        public UIWizardDialog()
            : this("UI Wizard Dialog Title")
        {           
        }

        /// <summary>
        /// Create a UIWizard dialog
        /// </summary>
        /// <param name="title">The title to use as the dialog title</param>
        public UIWizardDialog(string title)
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);

            //by default all of our buttons should be disabled; we'll re-enable them through events.
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;

            m_Title = title;

            if (_DesignMode)
                return;

            //set up the wizard controller
            m_WizardEngine = new WizardEngine(title);
            m_WizardEngine.CurrentPageChanged += WizardSequencer_CurrentPageChanged;
            m_WizardEngine.FinishEnabledChanged += WizardSequencer_FinishEnabledChanged;
            m_WizardEngine.NextEnabledChanged += WizardSequencer_NextEnabledChanged;
            m_WizardEngine.PreviousEnabledChanged += WizardSequencer_PreviousEnabledChanged;
            m_WizardEngine.TitleChanged += WizardSequencer_TitleChanged;
            m_WizardEngine.CaptionChanged += WizardSequencer_CaptionChanged;
            m_WizardEngine.Finished += WizardSequencer_Finished;
        }

        /// <summary>
        /// Add a new page to the end of the wizard sequence.
        /// </summary>
        /// <param name="newPage"></param>
        /// <remarks>Pages can't be added to the wizard once the wizard starts</remarks>
        protected void AddPage(IUIWizardInputPage newPage)
        {
            if (newPage == null)
                throw new ArgumentNullException(nameof(newPage));

            newPage.Initialize(m_Request);
            m_WizardEngine.AddPage(newPage);
            ((UserControl)newPage).BackColor = wizardDisplayPanel.BackColor;
            FormTools.ApplyOSFont((UserControl)newPage);
        }

        /// <summary>
        /// Set the one and only sequencer used to determine what page to transition to next.
        /// </summary>
        /// <param name="sequencer"></param>
        protected void SetSequencer(IUIWizardSequencer sequencer)
        {
            if (sequencer == null)
                throw new ArgumentNullException(nameof(sequencer));

            sequencer.Initialize(m_WizardEngine, m_Request);
            m_WizardEngine.SetSequencer(sequencer);
        }

        /// <summary>
        /// Set the one and only finished page for the end of the wizard
        /// </summary>
        /// <param name="newPage">The new finished page.</param>
        /// <remarks>A wizard must have a finished page and can only have one.  It can't be changed once the wizard 
        /// is started.</remarks>
        protected void SetFinishedPage(IUIWizardFinishPage newPage)
        {
            if (newPage == null)
                throw new ArgumentNullException(nameof(newPage));

            newPage.Initialize(m_Request);
            m_WizardEngine.SetFinishedPage(newPage);
            ((UserControl)newPage).BackColor = wizardDisplayPanel.BackColor;
            FormTools.ApplyOSFont((UserControl)newPage);
        }

        #region Protected Properties and Methods

        /// <summary>
        /// Indicates if the dialog should ensure it's a topmost window or not.
        /// </summary>
        protected bool EnsureTopMost { get; set; }

        /// <summary>
        /// Called to set the request that will be used to initialize each item as it is added to the wizard.
        /// </summary>
        /// <param name="request"></param>
        protected void Initialize(object request)
        {
            m_Request = request;
        }

        /// <summary>
        /// Raised when the wizard is about to be shown.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>If you override this event, be sure to add a call to base.OnShown(e) to ensure
        /// that the wizard will start and the dialog will display.</remarks>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_DesignMode)
                return;

            //lock in our wizard
            DisplayWizard();

            //and now that we've displayed as a top most window, we might turn that off so we aren't stuck to the top.
            TopMost = EnsureTopMost;
        }

        #endregion

        #region Private Properties and Methods

        private bool _DesignMode { get { return (GetService(typeof(IDesignerHost)) != null) || (LicenseManager.UsageMode == LicenseUsageMode.Designtime); } }

        private void DisplayPage(IUIWizardPage page)
        {
            if (m_DisplayedPage != null)
            {
                m_DisplayedPage.Visible = false;
                m_DisplayedPage = null;
            }

            m_DisplayedPage = (UserControl)page;
            if (wizardDisplayPanel.Contains(m_DisplayedPage) == false)
            {
                //we need to display the page for the first time.
                wizardDisplayPanel.Controls.Add(m_DisplayedPage);
                m_DisplayedPage.Visible = true;
                m_DisplayedPage.Dock = DockStyle.Fill;
            }
            else
            {
                //it's already displayed, so we just need to set it to visible.
                m_DisplayedPage.Visible = true;
            }
        }

        private void DisplayWizard()
        {
            Text = m_Title; //here to avoid a virtual call in the constructor

            try
            {
                //and set the correct first page.
                m_WizardEngine.Start();
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Verbose, LogWriteMode.Queued, ex,  "Gibraltar.User Interface","Unable to Display Wizard",
                          "While displaying the wizard an exception was thrown ({0}): {1}",
                          ex.GetType().FullName, ex.Message);
            }
        }


        private void OnFinished(AsyncTaskResultEventArgs e)
        {
            //If we have an error OR we aren't auto-close we stop to show the outcome.
            IUIWizardFinishPage finishPage = m_DisplayedPage as IUIWizardFinishPage;
            if ((e.Result == AsyncTaskResult.Error)
                || ((finishPage != null) && (finishPage.AutoClose == false)))
            {
                if (m_DisplayedPage != null)
                {
                    m_DisplayedPage.Visible = false;
                    m_DisplayedPage = null;
                }
                m_DisplayedPage = new UIWizardResultPage(e);
                wizardDisplayPanel.Controls.Add(m_DisplayedPage);
                m_DisplayedPage.Visible = true;
                m_DisplayedPage.Dock = DockStyle.Fill;

                //and we want to let the user explicitly close us
                btnNext.Text = CloseText;
                
                //and switch to close behaviour
                m_CloseOnNext = true;
                btnNext.Enabled = true; //it got disabled when the user started the finish process.
                btnCancel.Enabled = false; //you can't cancel any more, it's all done.
            }
            else
            {
                //when the user finishes we want to wait one second for them to see what happened
                //and then go away.
                tmrAutoClose.Enabled = true;
            }

            //set our dialog result to the next button so others know how the wizard worked out.
            //Why the next button?  because if we use our Dialog result property we close immediately!
            switch (e.Result)
            {
                case AsyncTaskResult.Canceled:
                    btnNext.DialogResult = DialogResult.Cancel;
                    break;
                case AsyncTaskResult.Error:
                    btnNext.DialogResult = DialogResult.Abort;
                    break;
                case AsyncTaskResult.Warning:
                case AsyncTaskResult.Information:
                case AsyncTaskResult.Success:
                    btnNext.DialogResult = DialogResult.OK;
                    break;
                default:
                    btnNext.DialogResult = DialogResult.Abort;
                    break;
            }
        }

        #endregion

        #region Event Handlers

        private void WizardSequencer_PreviousEnabledChanged(object sender, EventArgs e)
        {
            btnPrevious.Enabled = m_WizardEngine.PreviousEnabled;
        }

        private void WizardSequencer_NextEnabledChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = (m_WizardEngine.NextEnabled || m_WizardEngine.FinishEnabled);

            //if next is enabled, finished isn't (at least for now...)
            if (m_WizardEngine.NextEnabled)
            {
                btnNext.Text = NextText;
            }
        }

        private void WizardSequencer_FinishEnabledChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = (m_WizardEngine.NextEnabled || m_WizardEngine.FinishEnabled);

            //we don't have a finished button, just a next button that we repurpose as we go.
            if (m_WizardEngine.FinishEnabled)
            {
                btnNext.Text = FinishText;
            }

            //we don't do anything in the else:  Different controls set the label when they are enabled.
        }

        private void WizardSequencer_Finished(object sender, AsyncTaskResultEventArgs e)
        {
            OnFinished(e);
        }

        private void WizardSequencer_TitleChanged(object sender, EventArgs e)
        {
            Text = m_WizardEngine.Title;
        }

        private void WizardSequencer_CaptionChanged(object sender, EventArgs e)
        {
            lblStepCaption.Text = m_WizardEngine.Caption;
            lblStepDescription.Text = m_WizardEngine.Instructions;
        }

        private void WizardSequencer_CurrentPageChanged(object sender, EventArgs e)
        {
            //make the new current page the displayed page
            DisplayPage(m_WizardEngine.CurrentPage);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            m_WizardEngine.MovePrevious();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            //we do either next or finish depending on what's enabled.
            if (m_CloseOnNext)
            {
                DialogResult = btnNext.DialogResult;
                Close(); //we want to close, not just hide.             
            }
            else
            {
                m_WizardEngine.MoveNext();
            }
        }

        private void tmrAutoClose_Tick(object sender, EventArgs e)
        {
            DialogResult = btnNext.DialogResult;
            Close();//we want to close, not just hide.
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = btnCancel.DialogResult;
            Close(); //we want to close, not just hide.
        }

        #endregion
    }
}
