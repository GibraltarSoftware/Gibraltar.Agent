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
using System;
using System.Collections.Generic;
using System.Globalization;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Implements a basic wizard processing engine.
    /// </summary>
    public class WizardEngine
    {
        private readonly List<IUIWizardInputPage> m_WizardPages = new List<IUIWizardInputPage>();
        private readonly string m_WizardTitle;

        private IUIWizardPage m_CurrentPage;
        private int m_CurrentPageIndex;
        private IUIWizardFinishPage m_FinishedPage;
        private bool m_Locked;
        private string m_Title;
        private string m_Caption;
        private string m_Instructions;

        //to correctly generate some of our events we have to track status between pages.
        private bool m_NextEnabled;
        private bool m_PreviousEnabled;
        private bool m_FinishEnabled;
        private IUIWizardSequencer m_Sequencer;

        /// <summary>
        /// Raised whenever the current page is changed.
        /// </summary>
        public event EventHandler CurrentPageChanged;

        /// <summary>
        /// Raised whenever the current page's ability to move next changes status.
        /// </summary>
        public event EventHandler NextEnabledChanged;

        /// <summary>
        /// Raised whenever the current page's ability to move previous changes status.
        /// </summary>
        public event EventHandler PreviousEnabledChanged;

        /// <summary>
        /// Raised whenever the current page's ability to move to the finished page changes status.
        /// </summary>
        public event EventHandler FinishEnabledChanged;

        /// <summary>
        /// Raised whenever the display title for the wizard changes.
        /// </summary>
        public event EventHandler TitleChanged;

        /// <summary>
        /// Raised whenever the display title for the wizard changes.
        /// </summary>
        public event EventHandler CaptionChanged;

        /// <summary>
        /// Raised when the wizard has finished.
        /// </summary>
        public event EventHandler<AsyncTaskResultEventArgs> Finished;

        /// <summary>
        /// Create a new wizard sequencer.
        /// </summary>
        /// <param name="title">The caption for the process performed by the wizard (used to help generate the title)</param>
        public WizardEngine(string title)
        {
            m_WizardTitle = title;
            m_CurrentPageIndex = -1;
        }

        #region Private Class Default Sequencer

        /// <summary>
        /// The default sequencer used when a sequencer isn't specified - it just goes forward and back.
        /// </summary>
        private class DefaultSequencer : IUIWizardSequencer
        {
            private WizardEngine m_Engine;
            private object m_Request;

            /// <summary>
            /// Called by the wizard engine to initialize the sequencer.
            /// </summary>
            /// <param name="engine"></param>
            /// <param name="request"></param>
            public void Initialize(WizardEngine engine, object request)
            {
                m_Engine = engine;
                m_Request = request;
            }

            /// <summary>
            /// Move to the next page of the wizard from the current page, if possible
            /// </summary>
            /// <returns>The new page the wizard moved to</returns>
            /// <remarks>If moving next isn't valid at this point an exception will be thrown.</remarks>
            public IUIWizardPage MoveNext()
            {
                if (m_Engine.CurrentPageIndex < m_Engine.Pages.Count - 1)
                    return m_Engine.Pages[m_Engine.CurrentPageIndex + 1];

                return m_Engine.FinishedPage;
            }

            /// <summary>
            /// Move to the previous page of the wizard from the current page, if possible
            /// </summary>
            /// <returns>The new page the wizard moved to</returns>
            /// <remarks>If moving previous isn't valid at this point an exception will be thrown.</remarks>
            public IUIWizardInputPage MovePrevious()
            {
                if ((m_Engine.CurrentPageIndex > 0) && (m_Engine.CurrentPage is IUIWizardInputPage))
                    return m_Engine.Pages[m_Engine.CurrentPageIndex - 1];

                return null;
            }
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// Add a new page to the end of the wizard sequence.
        /// </summary>
        /// <param name="newPage"></param>
        /// <remarks>Pages can't be added to the wizard once the wizard starts</remarks>
        public void AddPage(IUIWizardInputPage newPage)
        {
            if (m_Locked)
                throw new InvalidOperationException("The wizard is currently read-only and can't have any pages changed.");

            m_WizardPages.Add(newPage);
        }

        /// <summary>
        /// Set the one and only finished page for the end of the wizard
        /// </summary>
        /// <param name="newPage">The new finished page.</param>
        /// <remarks>A wizard must have a finished page and can only have one.  It can't be changed once the wizard 
        /// is started.</remarks>
        public void SetFinishedPage(IUIWizardFinishPage newPage)
        {
            if (m_Locked)
                throw new InvalidOperationException("The wizard is currently read-only and can't have any pages changed.");

            m_FinishedPage = newPage;
        }

        /// <summary>
        /// Set the one and only sequencer used to determine what page to transition to next.
        /// </summary>
        /// <param name="sequencer"></param>
        public void SetSequencer(IUIWizardSequencer sequencer)
        {
            if (m_Locked)
                throw new InvalidOperationException("The wizard is currently read-only and can't have its sequencer changed.");

            m_Sequencer = sequencer;
        }

        /// <summary>
        /// Start the wizard at the beginning of the sequence.
        /// </summary>
        public void Start()
        {
            //if no one has set up a sequencer, create and initialize the default sequencer.
            if (m_Sequencer == null)
            {
                DefaultSequencer newSequencer = new DefaultSequencer();
                SetSequencer(newSequencer);
                newSequencer.Initialize(this, null);
            }

            //now make sure we're valid
            if (m_FinishedPage == null)
            {
                throw new InvalidOperationException("There is no finished page defined for the wizard.");
            }

            //we're valid, so lock in our wizard so we don't take any changes after this.
            m_Locked = true;

            //and go to the first page.
            if (m_WizardPages.Count > 0)
            {
                SetCurrentPage(m_WizardPages[0]);
            }
            else
            {
                SetCurrentPage(m_FinishedPage);
            }
        }

        /// <summary>
        /// Move to the next page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving next isn't valid at this point an exception will be thrown.</remarks>
        public void MoveNext()
        {
            //is it legal to move next?
            if (m_CurrentPage == null)
            {
                throw new InvalidOperationException("There is no current page to move next from.");
            }

            if (m_CurrentPage.IsValid == false)
            {
                throw new InvalidOperationException("The current page does not allow moving next at this time.");
            }

            try
            {
                bool valid = ((IUIWizardInputPage)m_CurrentPage).Store();

                if (valid)
                {
                    IUIWizardPage nextPage = m_Sequencer.MoveNext();

                    SetCurrentPage(nextPage);
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.User Interface", "Wizard error moving to next page",
                          "While asking page {0} to move next it reported an exception.  The wizard will not proceed to the next page.\r\nException ({1}): {2}",
                          m_CurrentPage.Name, ex.GetType().FullName, ex.Message);
            }
        }

        /// <summary>
        /// Move to the previous page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving previous isn't valid at this point an exception will be thrown.</remarks>
        public void MovePrevious()
        {
            //is it legal to move next?
            if (m_CurrentPage == null)
            {
                throw new InvalidOperationException("There is no current page to move previous from.");
            }

            if ((m_CurrentPage is IUIWizardFinishPage) || (m_CurrentPageIndex == 0))
            {
                throw new InvalidOperationException("The current page does not allow moving previous at this time.");
            }

            try
            {
                IUIWizardPage previousPage = m_Sequencer.MovePrevious();

                //otherwise we're good to go.
                SetCurrentPage(previousPage);
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.User Interface", "Wizard error moving to previous page",
                          "While asking page {0} to move previous it reported an exception.  The wizard will not proceed to the previous page.\r\nException ({1}): {2}",
                          m_CurrentPage.Name, ex.GetType().FullName, ex.Message);
            }
        }


        /// <summary>
        /// The current page (either an input page or a finished page)
        /// </summary>
        public IUIWizardPage CurrentPage { get { return m_CurrentPage; } }

        /// <summary>
        /// The finished page set in the wizard.
        /// </summary>
        public IUIWizardFinishPage FinishedPage { get { return m_FinishedPage; } }

            /// <summary>
        /// The numeric index of the CurrentPage within the Pages collection, if it is an input page.
        /// </summary>
        /// <remarks>If the wizard hasn't started or is on the finished page then the current page is not in the pages collection and -1 will be returned.</remarks>
        public int CurrentPageIndex
        {
            get
            {
                return m_CurrentPageIndex;              
            }
        }

        /// <summary>
        /// Indicates if there is a current page or not.
        /// </summary>
        /// <remarks>If the wizard has not started there is no current page.</remarks>
        public bool CurrentPageIsNull { get { return (m_CurrentPage == null); } }

        /// <summary>
        /// Indicates if the current page can transition to the next page at this time.
        /// </summary>
        public bool NextEnabled { get { return m_NextEnabled; } }

        /// <summary>
        /// Indicates if the current page can transition to the previous page at this time.
        /// </summary>
        public bool PreviousEnabled { get { return m_PreviousEnabled; } }

        /// <summary>
        /// Indicates if the current page can transition directly to the finished page.
        /// </summary>
        public bool FinishEnabled { get { return m_FinishEnabled; } }

        /// <summary>
        /// A display title for the wizard in its current state.  Typically used in the title bar or form text.
        /// </summary>
        public string Title { get { return m_Title; } }

        /// <summary>
        /// End user caption for the current page of the wizard.
        /// </summary>
        public string Caption { get { return m_Caption; } }

        /// <summary>
        /// End user instructions for completing this page of the wizard.
        /// </summary>
        public string Instructions { get { return m_Instructions; } }

        /// <summary>
        /// The list of all input wizard pages registered.
        /// </summary>
        public List<IUIWizardInputPage> Pages { get { return m_WizardPages; } }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Called to raise the CurrentPageChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnCurrentPageChanged()
        {
            EventHandler tempEvent = CurrentPageChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the NextEnabledChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnNextEnabledChanged()
        {
            EventHandler tempEvent = NextEnabledChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the PreviousEnabledChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnPreviousEnabledChanged()
        {
            EventHandler tempEvent = PreviousEnabledChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the FinishEnabledChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnFinishEnabledChanged()
        {
            EventHandler tempEvent = FinishEnabledChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the Finished event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnFinished(AsyncTaskResultEventArgs e)
        {
            EventHandler<AsyncTaskResultEventArgs> tempEvent = Finished;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, e);
            }
        }

        /// <summary>
        /// Called to raise the TitleChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnTitleChanged()
        {
            EventHandler tempEvent = TitleChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the CaptionChanged event.
        /// </summary>
        /// <remarks>If overriding this method, be sure to call the base implementation or the event will not be raised.</remarks>
        protected virtual void OnCaptionChanged()
        {
            EventHandler tempEvent = CaptionChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to calculate a title for the current page of the wizard.
        /// </summary>
        /// <returns></returns>
        protected virtual string CalculateTitle()
        {
            //two fragments to the title, and we aren't guaranteed they're all there.
            //<Wizard Title> <Progress>

            string title = m_WizardTitle ?? string.Empty;
            if (m_WizardPages.Count > 0)
            {
                //a few different routes depending on whether we're in a user input phase or completion phase
                if (m_CurrentPage is IUIWizardFinishPage)
                {
                    title += string.Format(FileSystemTools.UICultureFormat, " Step {0} of {1}", m_WizardPages.Count + 1, m_WizardPages.Count + 1);                    
                }
                else
                {
                    title += string.Format(FileSystemTools.UICultureFormat, " Step {0} of {1}", m_CurrentPageIndex + 1, m_WizardPages.Count + 1);                    
                }
            }

            return title;
        }

        #endregion

        #region Private Properties and Methods

        private void SetCaption()
        {
            m_Caption = m_CurrentPage.Title;
            m_Instructions = m_CurrentPage.Instructions;

            OnCaptionChanged();
        }

        private void SetTitle()
        {
            string originalTitle = m_Title;
            m_Title = CalculateTitle();
            m_Instructions = m_CurrentPage.Instructions;

            if (string.Compare(m_Title, originalTitle) != 0)
            {
                OnTitleChanged();
            }
        }

        private void SetCurrentPage(IUIWizardPage newPage)
        {
            if (newPage == null)
                throw new ArgumentNullException(nameof(newPage));

            //unwire our previous event handler
            if (m_CurrentPage != null)
            {
                m_CurrentPage.IsValidChanged -= CurrentPage_IsValidChanged;
            }

            m_CurrentPage = newPage;

            if (m_CurrentPage is IUIWizardInputPage)
                m_CurrentPageIndex = m_WizardPages.IndexOf((IUIWizardInputPage)m_CurrentPage);

            //and wire up these event handlers.
            m_CurrentPage.IsValidChanged += CurrentPage_IsValidChanged;

            try
            {
                m_CurrentPage.OnEnter();
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.User Interface", "Wizard error upon entering new page",
                          "Unable to enter wizard page {0} due to an exception.  Page will be displayed anyway, but erratic user results can be expected.\r\nException ({1}): {2}",
                          m_CurrentPage.Name, ex.GetType().FullName, ex.Message);
            }

            //do our generic stuff we do every time the current page changes
            SetTitle();
            SetCaption();

            //now things get a little different because we want to route the "move next" option to be either finish or move next
            //depending on where we are in the wizard flow. 
            if (m_CurrentPage is IUIWizardFinishPage)
            {
                SetCanMovePrevious(false);
                SetCanMoveNext(false);
                SetCanFinish(m_CurrentPage.IsValid);
            }
            else
            {
                SetCanMovePrevious(true);
                SetCanMoveNext(m_CurrentPage.IsValid);
                SetCanFinish(false);                
            }

            OnCurrentPageChanged();

            //if this is the finished page then we need to actually DO the finish.
            if (m_CurrentPage is IUIWizardFinishPage)
            {
                AsyncTaskResultEventArgs result = ((IUIWizardFinishPage)m_CurrentPage).Finish();
                OnFinished(result);
            }
        }


        private void SetCanMovePrevious(bool newValue)
        {
            //but what are we REALLY going to allow...
            bool effectiveValue = newValue;
            if (m_CurrentPage == null)
                effectiveValue = false;

            if (m_CurrentPageIndex == 0)
                effectiveValue = false;

            if (effectiveValue != m_PreviousEnabled)
            {
                m_PreviousEnabled = effectiveValue;
                OnPreviousEnabledChanged();
            }
        }

        private void SetCanMoveNext(bool newValue)
        {
            //but what are we REALLY going to allow...
            bool effectiveValue = newValue;
            if (m_CurrentPage == null)
                effectiveValue = false;

            if (ReferenceEquals(m_CurrentPage, m_FinishedPage))
                effectiveValue = false;

            if (effectiveValue != m_NextEnabled)
            {
                m_NextEnabled = effectiveValue;
                OnNextEnabledChanged();
            }
        }

        private void SetCanFinish(bool newValue)
        {
            //but what are we REALLY going to allow...
            bool effectiveValue = newValue;
            if (m_CurrentPage == null)
                effectiveValue = false;

            if (effectiveValue != m_FinishEnabled)
            {
                m_FinishEnabled = effectiveValue;
                OnFinishEnabledChanged();
            }
        }

        #endregion

        #region Event Handlers

        private void CurrentPage_IsValidChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, m_CurrentPage))
            {
                SetCanMoveNext(m_CurrentPage.IsValid);
            }
        }

        #endregion
    }
}
