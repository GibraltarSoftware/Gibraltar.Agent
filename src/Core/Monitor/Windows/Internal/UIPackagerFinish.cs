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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Data;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Monitor.Windows.Internal
{
    internal partial class UIPackagerFinish : UserControl, IUIWizardFinishPage
    {
        private const string m_Title = "Packaging and Sending Data";

        private readonly PackagerConfiguration m_Configuration;

        private PackagerRequest m_Request;
        private ProgressMonitorStack m_ProgressMonitors;
        private bool m_ReadOnly;
        private string m_Instructions;

        //status information when we complete.
        private bool m_Completed;
        private AsyncTaskResultEventArgs m_FinalResults;

        public UIPackagerFinish(PackagerConfiguration configuration)
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
        /// The wizard controller is entering this page.
        /// </summary>
        public void OnEnter()
        {
            //set up our display elements
            string action;
            switch(m_Request.Transport)
            {
                case PackageTransport.File:
                    lblStaticMessage.Text = "Exporting sessions to file...";
                    action = "saved to the file you selected";
                    break;
                case PackageTransport.RemovableMedia:
                    lblStaticMessage.Text = "Exporting sessions to removable media...";
                    action = "saved onto your removable media";
                    break;
                case PackageTransport.Email:
                    lblStaticMessage.Text = "Thank you for submitting this important diagnostic information to us.";
                    action = "packaged and emailed";
                    break;
                case PackageTransport.Server:
                    lblStaticMessage.Text = "Thank you for submitting this important diagnostic information to us.";
                    action = "sent to our server";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //figure out a nice instruction.  Starting with what the inclusion conditions are.
            string criteria;
            if ((m_Request.Criteria & SessionCriteria.NewSessions) == SessionCriteria.NewSessions)
            {
                //we don't have to figure out all possible criteria combinations, just the ones we set.
                criteria = "unsent sessions are";
            }
            else if ((m_Request.Criteria & SessionCriteria.AllSessions) == SessionCriteria.AllSessions)
            {
                criteria = "all sessions are";
            }
            else
            {
                //must be just the active session.
                criteria = "the current session is";
            }

            m_Instructions = string.Format("Please wait while {0} {1}.  Depending on the amount of data this may take a few minutes.", criteria, action);

            lblStatusMessage.Text = string.Empty;
        }

        /// <summary>
        /// Query if the wizard page allows moving next.
        /// </summary>
        public bool IsValid { get { return false; } }

        /// <summary>
        /// Called to have the wizard finish page execute the finish procedure.
        /// </summary>
        /// <returns>The status of the wizard once execution completes.</returns>
        public AsyncTaskResultEventArgs Finish()
        {
            //we want to set our wait cursor and clear it when we leave
            bool priorWaitCursor = Application.UseWaitCursor;   //perhaps our intrepid caller already did it.
            Application.UseWaitCursor = true;

            try
            {
                m_ProgressMonitors = new ProgressMonitorStack("Packaging Sessions");

                //and initialize our different UI elements
                lblStatusMessage.Text = string.Empty; //just so we clear out whatever was there during the designer's construction

                m_ProgressMonitors.Canceled += Monitors_Canceled;
                m_ProgressMonitors.Completed += Monitors_Completed;
                m_ProgressMonitors.Updated += Monitors_Updated;

                //we are going to have the thread pool asynchronously execute the actual packaging
                ThreadPool.QueueUserWorkItem(AsyncPackagerRequest, null);

                //And we need to act like a modal dialog.
                while (m_Completed == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(16);
                }                
            }
            finally
            {
                m_ProgressMonitors.Dispose();
                m_ProgressMonitors = null;

                //set the wait cursor back to whatever the user had it as
                Application.UseWaitCursor = priorWaitCursor;
            }

            return m_FinalResults;
        }

        /// <summary>
        /// Indicates if the wizard should automatically close on successful finish.
        /// </summary>
        public bool AutoClose { get { return true; } }

        #endregion

        #region Protected properties and Methods

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// Perform the packaging according to the user request.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void AsyncPackagerRequest(object stateInfo)
        {
            //we don't use any of the state information.  We just do the processing
            if (!Log.SilentMode) Log.Write(LogMessageSeverity.Verbose, Packager.LogCategory, "Starting to execute send package request for user", null);

            PackageSendEventArgs resultEventArgs;

            try
            {
                Packager workingPackage;
                if (string.IsNullOrEmpty(m_Request.ProductName))
                {
                    workingPackage = new Packager();
                }
                else
                {
                    workingPackage = new Packager(m_Request.ProductName, m_Request.ApplicationName);
                }

                //Send the package
                switch (m_Request.Transport)
                {
                    case PackageTransport.File:
                        resultEventArgs = workingPackage.SendToFile(m_Request.Criteria, true, m_Request.FileNamePath, m_ProgressMonitors);
                        break;
                    case PackageTransport.RemovableMedia:
                        //figure out the right file name to use.
                        string fileNamePath = GenerateFileName(m_Request.FileNamePath, workingPackage.ProductName, workingPackage.ApplicationName);

                        resultEventArgs = workingPackage.SendToFile(m_Request.Criteria, true, fileNamePath, m_ProgressMonitors);
                        break;
                    case PackageTransport.Email:

                        //we MUST have a from address, so work through to what it is...
                        string effectiveFromAddress = m_Request.FromEmailAddress;
                        if (string.IsNullOrEmpty(effectiveFromAddress))
                        {
                            effectiveFromAddress = m_Configuration.FromEmailAddress;
                        }

                        if (string.IsNullOrEmpty(effectiveFromAddress))
                        {
                            effectiveFromAddress = m_Request.DestinationEmailAddress;
                        }

                        //two possibilities:  Either we are overriding server info or we aren't.
                        if (string.IsNullOrEmpty(m_Request.EmailServer))
                        {
                            //lets not take for granted every email server related value is sane, lets force the issue.
                            resultEventArgs = workingPackage.SendEmail(m_Request.Criteria, true, null, effectiveFromAddress, m_Request.DestinationEmailAddress, 
                                string.Empty, 0, null, string.Empty, string.Empty, m_ProgressMonitors);                            
                        }
                        else
                        {
                            //override email server and sub options with what's in the request.
                            resultEventArgs = workingPackage.SendEmail(m_Request.Criteria, true, null, effectiveFromAddress, m_Request.DestinationEmailAddress,
                                 m_Request.EmailServer, m_Request.ServerPort, m_Request.UseSsl, m_Request.EmailServerUser, m_Request.EmailServerPassword, m_ProgressMonitors);
                        }
                        break;
                    case PackageTransport.Server:
                        //arguably there should be some way to figure out if we should purge sent sessions, but that is really not clear...
                        resultEventArgs = workingPackage.SendToServer(m_Request.Criteria, true, false, false, null, false, null, null, 0, false, null, null, m_ProgressMonitors);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                //If we get an exception we just want to create a PackageSEndEventArgs to wrapper it so we can have one common control flow.
                Debug.Assert(false, "Unable to finish packager wizard", "While finishing the packager process, an exception was raised: " + ex);

                string message;
                switch (m_Request.Transport)
                {
                    case PackageTransport.File:
                        message = string.Format(FileSystemTools.UICultureFormat, "Unable to write sessions to disk.\r\nError: {0}", ex.Message);
                        break;
                    case PackageTransport.RemovableMedia:
                        message = string.Format(FileSystemTools.UICultureFormat, "Unable to write sessions to removable media.\r\nError: {0}", ex.Message);
                        break;
                    case PackageTransport.Email:
                        message = string.Format(FileSystemTools.UICultureFormat, "Unable to send sessions via email.\r\nError: {0}", ex.Message);
                        break;
                    case PackageTransport.Server:
                        message = string.Format(FileSystemTools.UICultureFormat, "Unable to send sessions to the server.\r\nError: {0}", ex.Message);
                        break;
                    default:
                        message = string.Format(FileSystemTools.UICultureFormat, "Unable to write sessions to disk.\r\nError: {0}", ex.Message);
                        break;
                }
                resultEventArgs = new PackageSendEventArgs(0, AsyncTaskResult.Error, message, ex);
            }

            m_FinalResults = resultEventArgs;
            m_Completed = true;
            if (!Log.SilentMode) Log.Write(LogMessageSeverity.Verbose, Packager.LogCategory, "Completed executing send package request for user.", "Final Status: {0}. Message: {1}", m_FinalResults.Result, m_FinalResults.Message);
        }

        private void CancelProgress()
        {
            //quick protection from most out-of-order events to avoid exceptions
            if (m_Completed)
                return;

            //we have to check to see if we were called on our thread or, more likely, another thread.
            if (InvokeRequired)
            {
                //we are in fact not on the right thread.
                MethodInvoker d = CancelProgress;

                //There are some timing cases where this invoke could throw an exception - like we're right in the middle of
                //hiding because we're already complete.  We don't want these exceptions.
                try
                {
                    Invoke(d);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception ex)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    //we don't care, but log it in debug mode.
                    GC.KeepAlive(ex);
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Packager", "Exception during Invoke(CancelProgress)", null);
#endif
                }
            }
            else
            {
                //now we're on the right thread!

                //make sure we haven't already completed or canceled.  Ignore out of order events.
                if (m_ReadOnly)
                    return;

                //change our status messages for a second so the user sees it
                packageProgress.Value = packageProgress.Maximum;

                //mark we canceled so we return the right thing to our caller
                m_FinalResults = new AsyncTaskResultEventArgs(AsyncTaskResult.Canceled, "Wizard canceled, no information was sent.");
                m_ReadOnly = true;  //to prevent subsequent events from messing with us.
            }
        }

        private void CompleteProgress()
        {
            //quick protection from most out-of-order events to avoid exceptions
            if (m_Completed)
                return;

            //we have to check to see if we were called on our thread or, more likely, another thread.
            if (InvokeRequired)
            {
                //we are in fact not on the right thread.
                MethodInvoker d = CompleteProgress;

                //There are some timing cases where this invoke could throw an exception - like we're right in the middle of
                //hiding because we're already complete.  We don't want these exceptions.
                try
                {
                    Invoke(d);
                }
                catch (Exception ex)
                {
                    //we don't care, but log it in debug mode.
                    GC.KeepAlive(ex);
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Packager", "Exception during Invoke(CompleteProgress)", null);
#endif
                }
            }
            else
            {
                //now we're on the right thread!

                //make sure we haven't already completed or canceled.  Ignore out of order events.
                if (m_ReadOnly)
                    return;

                //change our status messages for a second so the user sees it
                packageProgress.Value = packageProgress.Maximum;

                m_ReadOnly = true;  //to prevent subsequent events from messing with us.
            }

        }

        private delegate void UpdateProgressCallback(string labelCaption, int completedSteps, int maximumSteps);

        private void UpdateProgress(string labelCaption, int completedSteps, int maximumSteps)
        {
            //quick protection from most out-of-order events to avoid exceptions
            if (m_Completed)
                return;

            //we have to check to see if we were called on our thread or, more likely, another thread.
            if (lblStatusMessage.InvokeRequired)
            {
                //we are in fact not on the right thread.
                UpdateProgressCallback d = UpdateProgress;

                //There are some timing cases where this invoke could throw an exception - like we're right in the middle of
                //hiding because we're already complete.  We don't want these exceptions.
                try
                {
                    Invoke(d, new object[] { labelCaption, completedSteps, maximumSteps });
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception ex)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    //we don't care, but log it in debug mode.
                    GC.KeepAlive(ex);
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Packager", "Exception during Invoke(UpdateProgress)", null);
#endif
                }
            }
            else
            {
                //now we're on the right thread!

                //make sure we haven't already completed or canceled.  Ignore out of order events.
                if (m_ReadOnly)
                    return;

                //We have two display modes - marquee mode when the remaining work is unknown and progress mode
                if (maximumSteps == 0)
                {
                    //we are in marquee mode.
                    lblStatusMessage.Text = labelCaption;
                    lblStatusMessage.Invalidate();
                    packageProgress.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    lblStatusMessage.Text = labelCaption;
                    lblStatusMessage.Invalidate();
                    packageProgress.Style = ProgressBarStyle.Continuous;

                    //watch out for walking backwards
                    Debug.Assert((completedSteps / maximumSteps) >= (packageProgress.Value / packageProgress.Maximum));

                    packageProgress.Maximum = maximumSteps;
                    packageProgress.Value = Math.Min(completedSteps, maximumSteps);
                    packageProgress.Invalidate();   //so we get painted as soon as possible
                }
            }

        }

        /// <summary>
        /// Generates a useful, unique file name for the package in the provided target directory.
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <param name="productName"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        private static string GenerateFileName(string targetDirectory, string productName, string applicationName)
        {
            string packageName;
            string timeStamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(productName))
            {
                packageName = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Log.SessionSummary.HostName, timeStamp);
            }
            else
            {
                if (string.IsNullOrEmpty(applicationName))
                {
                    packageName = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", 
                        productName, Log.SessionSummary.HostName, timeStamp);
                }
                else
                {
                    packageName = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", 
                        productName, applicationName, Log.SessionSummary.HostName, timeStamp);
                }
            }

            //get rid of any illegal characters.
            packageName = FileSystemTools.SanitizeFileName(packageName + "." + Log.PackageExtension);

            string fileNamePath = Path.Combine(targetDirectory, packageName);

            return FileSystemTools.MakeFileNamePathUnique(fileNamePath);
        }

        #endregion

        #region Event Handlers

        private void Monitors_Canceled(object sender, ProgressMonitorStackEventArgs e)
        {
            CancelProgress();
        }

        private void Monitors_Completed(object sender, ProgressMonitorStackEventArgs e)
        {
            CompleteProgress();
        }

        private void Monitors_Updated(object sender, ProgressMonitorStackEventArgs e)
        {
            UpdateProgress(e.StatusMessage, e.CompletedSteps, e.MaximumSteps);
        }

        #endregion
    }
}
