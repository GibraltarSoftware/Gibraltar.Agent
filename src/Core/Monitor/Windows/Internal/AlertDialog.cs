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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// An exception display dialog for one or many exceptions.
    /// </summary>
    internal partial class AlertDialog : Form
    {
        private const string CloseAsContinueCaption = "&Continue";
        private const string CloseAsCloseCaption = "&Close";
        private const string CloseAsExitCaption = "&Exit";
        private const string InstructionOneException = "{0} has encountered an unexpected problem";
        private const string InstructionMultipleException = "{0} has encountered some unexpected problems";
        private const string InstructionAddendumCantContinue = " and can't continue";

        private readonly object m_Lock = new object();
        private readonly List<ExceptionDisplayRequest> m_Exceptions = new List<ExceptionDisplayRequest>();

        private int m_MinimumHeaderHeight; //really the bottom of the graphic in the upper right so we know what we have to be below.
        private bool m_HaveAnyBlockingRequests;
        private bool m_HaveAnyCantContinueRequests;
        private bool m_Expanded;
        private bool m_IgnoreAllProblems;

        private static bool g_AlertDialogPreviouslyClosed;

        private delegate void DisplayExceptionInvoker(ExceptionDisplayRequest newRequest);

        /// <summary>
        /// Create a new alert dialog.  A new dialog is required each time that it is closed.
        /// </summary>
        public AlertDialog()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);
        }

        #region Public Properties and Methods

        /// <summary>
        /// Restore the form to the front of the Z order.
        /// </summary>
        /// <remarks>This must be called from the UI thread for this form.  The form will be restored from a Minimized
        /// state to Normal (but Maximized left unchanged), pushed to the front, and ensured to be Visible.</remarks>
        internal void RestoreToFront()
        {
            // For a minimized window, restore it.
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            // This little bit with TopMost ensures that the window, if already open,
            // but beneath other windows, will be raised to the front.
            TopMost = true; // Raise us to the front...
            Visible = true; // Make sure we're visible.
            // Alert dialog is intended to stick to the front, at least for now.
        }

        /// <summary>
        /// Display the provided exception request, showing the dialog if necessary
        /// </summary>
        /// <param name="newRequest">The request to be displayed.</param>
        public void DisplayException(ExceptionDisplayRequest newRequest)
        {
            if (newRequest == null)
                throw new ArgumentNullException(nameof(newRequest));

            if (InvokeRequired)
            {
                DisplayExceptionInvoker method = DisplayException;
                Invoke(method, new object[] { newRequest });
            }
            else
            {
                ActionDisplayException(newRequest);
            }
        }

        /// <summary>
        /// Attempt to marshall to the client UI thread to perform an Application.Exit() (asynchronously via BeginInvoke).
        /// </summary>
        public void InvokeApplicationExit()
        {
            Form clientForm = Listener.ClientFormsActive(); // Get the "first" client form still open.
            if (clientForm == null)
                clientForm = this; // Use ourselves if there aren't any client forms to invoke to.

            if (clientForm.IsHandleCreated)
                clientForm.BeginInvoke(new MethodInvoker(Application.Exit)); // Do this from the client UI thread if possible.
            else
                Application.Exit(); // Try anyway?
        }

        /// <summary>
        /// Attempt to marshall to the client UI thread to perform an Application.Restart() (asynchronously via BeginInvoke).
        /// </summary>
        public void InvokeApplicationRestart()
        {
            Form clientForm = Listener.ClientFormsActive(); // Get the "first" client form still open.
            if (clientForm == null)
                clientForm = this; // Use ourselves if there aren't any client forms to invoke to.

            if (clientForm.IsHandleCreated)
                clientForm.BeginInvoke(new MethodInvoker(Application.Restart)); // Do this from the client UI thread if possible.
            else
                Application.Restart(); // Try anyway?
        }

        #endregion
        
        #region Protected Properties and Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible == false)
            {
                //see if we somehow missed our various hide options.
                lock (m_Lock)
                {
                    if (m_Exceptions.Count > 0)
                        ActionHideDisplay(DialogResult.Cancel, false); //This only happens when we aren't going through one of our explicit actions, because the explicit ones already do it.
                }
            }

            base.OnVisibleChanged(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            bool invoking = false;
            try
            {
                // The .NET framework doesn't properly marshal this event when Application.Exit() is called, so be careful.
                if (InvokeRequired && IsHandleCreated)
                {
                    invoking = true;
                    Invoke(new FormClosedEventHandler(DoFormClosed), new object[] { this, e }); // Synchronous Invoke.
                }
                else
                {
                    DoFormClosed(this, e);
                }
            }
            catch (Exception ex)
            {
                if (!Log.SilentMode)
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Error Manager", null, // Error closing GEM (format the caption)
                              "Error closing GEM: {0}\r\n" +
                              "An error occurred while attempting to handle the FormClosed event in the Gibraltar Error Manager.  " +
                              "Handler call: {1}\r\nGEM Form state: {2}\r\n", ex.Message, invoking ? "Invoke" : "Direct",
                              IsHandleCreated ? "Still Open" : "Closed");
            }

            base.OnFormClosed(e);
        }

        private void DoFormClosed(object sender, FormClosedEventArgs e)
        {
            //see if we somehow missed our various hide options.
            lock (m_Lock)
            {
                if (m_Exceptions.Count > 0)
                {
                    //This only happens when we aren't going through one of our explicit actions, because the explicit ones already do it.
                    CloseReason reason = e.CloseReason;
                    ActionHideDisplay(DialogResult.Cancel, LiveViewerForm.ReasonIsAppExiting(reason));
                }
                else
                {
                    Listener.SuppressAlerts = m_IgnoreAllProblems; // Gets copied by ActionHideDisplay(), but we didn't call that.
                }
            }
        }

        //private delegate Icon GetIconDelegate();

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

            //Go a-huntin' for a form.
            if (ParentForm == null)
            {
                //see if we can get an icon from the main application.
                foreach (Form currentForm in Application.OpenForms)
                {
                    if ((currentForm.InvokeRequired)  && (currentForm.Icon != null))
                    {
                        //it's a possible candidate - it isn't on our thread....
                        try
                        {
                            parentIcon = new Icon(currentForm.Icon, currentForm.Icon.Size); //we don't invoke because that would deadlock if the UI is calling us...
                            //                        Form clientForm = currentForm; //since this isn't involved in the iteration it makes the anonymous delegate compile more definitively.
                            //                        parentIcon = currentForm.Invoke((GetIconDelegate)(() => new Icon(clientForm.Icon, clientForm.Icon.Size))) as Icon;
                        }
// ReSharper disable EmptyGeneralCatchClause
                        catch
// ReSharper restore EmptyGeneralCatchClause
                        {
                        }
                    }

                    if (parentIcon != null)
                        break;
                }
            }
            else
            {
                parentIcon = ParentForm.Icon;
            }

            //if we didn't get an icon from a loaded form, grab it from the executable itself
            if (parentIcon == null)
            {
                try
                {
                    parentIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            //and set it to our best case result.
            Icon = parentIcon ?? SystemIcons.Exclamation;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //display the error icon for the current OS
            int locationX = (messageFlowLayout.Left - SystemIcons.Error.Width) / 2;
            int locationY = (messageFlowLayout.Top + (lblMainInstruction.Height / 2)) - (SystemIcons.Error.Height / 2);
            locationY = Math.Min(messageFlowLayout.Top, locationY); //if we have a short label we might have slipped above this, we don't want to do that.

            m_MinimumHeaderHeight = locationY + SystemIcons.Error.Height; //so we never go above this when we're drawing the exception grid.

            e.Graphics.DrawIcon(SystemIcons.Error, locationX, locationY);
        }

        #endregion

        #region Private Properties and Methods

        private void ActionDisplayException(ExceptionDisplayRequest newRequest)
        {
            lock (m_Lock)
            {
                //add the exception request to the list to display.
                m_Exceptions.Add(newRequest);
                exceptionListViewer.AddException(newRequest);

                chkReportProblems.Checked = Log.SendSessionsOnExit; //set it to our current status.
                string message = string.Empty;
                chkReportProblems.Visible = Log.CanSendSessionsOnExit(ref message); //he giveth, and he taketh away...

                m_HaveAnyCantContinueRequests = (m_HaveAnyCantContinueRequests || (newRequest.CanContinue == false));
                m_HaveAnyBlockingRequests = (m_HaveAnyBlockingRequests || newRequest.ThreadBlocked);

                //if this is the FIRST we need to update our message to reflect it.
                if (m_Exceptions.Count == 1)
                {
                    Text = Log.SessionSummary.Application;
                    lblMainInstruction.Text = string.Format(FileSystemTools.UICultureFormat, InstructionOneException, Log.SessionSummary.Application)
                                              + (m_HaveAnyCantContinueRequests ? InstructionAddendumCantContinue : string.Empty);
                    lblErrorSummary.Text = newRequest.Exception.Message;
                }
                else if (m_Exceptions.Count > 1)
                {
                    //we are transitioning from 1 to 2, change our label to reflect the dual case.
                    lblMainInstruction.Text = string.Format(FileSystemTools.UICultureFormat, InstructionMultipleException, Log.SessionSummary.Application)
                                              + (m_HaveAnyCantContinueRequests ? InstructionAddendumCantContinue : string.Empty);
                    lblErrorSummary.Text = string.Empty;
                }

                //adjust the size of our controls to handle the new data.
                SizeControl();

                btnClose.Text = m_HaveAnyCantContinueRequests ? CloseAsExitCaption : 
                    m_HaveAnyBlockingRequests ? CloseAsContinueCaption : CloseAsCloseCaption;

                //finally, show the dialog and make us the foreground window. 
                if (Visible == false)
                {
                    Show();
                }
                else
                {
                    //move to the foreground
                    TopMost = true;

                    TopMost = m_HaveAnyBlockingRequests || m_HaveAnyCantContinueRequests; // so if we do we stay topmost, otherwise we lose it.
                }
            }
        }

        private void ActionHideDisplay(DialogResult result, bool appIsExiting)
        {
            lock (m_Lock)
            {
                Listener.SuppressAlerts = m_IgnoreAllProblems; // Takes effect when we go away.

                g_AlertDialogPreviouslyClosed = true; //so next time in we know.

                //set the result for each exception we have.
                foreach (ExceptionDisplayRequest request in m_Exceptions)
                {
                    request.Result = result;
                }
                m_HaveAnyBlockingRequests = false; // They aren't blocking anymore; they all just got released.

                //Reset our user interface state.
                m_Exceptions.Clear();
                exceptionListViewer.Clear();

                // In the case that the dialog is being closed not as part of an overall app exit...
                if (appIsExiting == false)
                {
                    // And we have a can't-continue request but none that come from an unhandled exception...
                    if (m_HaveAnyCantContinueRequests && Net.ExceptionListener.FatalExceptionHasOccurred == false)
                    {
                        // Then in that limited case we need to trigger an Application.Exit() for them, and EndSession as Crashed.
                        Log.EndSession(SessionStatus.Crashed, null, "User Exiting Application after Exception");
                        InvokeApplicationExit(); // Invoke the Application.Exit() call asynchronously on the client UI thread.
                    }
                    // Otherwise, the application is already exiting, or it doesn't need to.

                    // Don't issue a Close when already exiting, it'll loop around and could deadlock!

                    if (IsHandleCreated) // Only Close if we aren't already closed!
                        Close(); // We no longer need to stay hidden, just close all the way so we don't keep the app up forever.
                }
            }
        }

        private void ActionClose()
        {
            lock (m_Lock)
            {
                if (m_HaveAnyCantContinueRequests) // Is this actually an Exit?
                {
                    Log.WriteMessage(LogMessageSeverity.Information, LogWriteMode.Queued, "Gibraltar", "Exception", null, null, null, null,
                                     "User Exiting Application after Exception", "The user has elected to close the alert dialog and exit the application.");
                    Log.EndSession(SessionStatus.Crashed, null, "User Exiting Application after Exception");
                }
                else
                {
                    Log.WriteMessage(LogMessageSeverity.Information, LogWriteMode.Queued, "Gibraltar", "Exception", null, null, null, null,
                                     "User Continuing Execution after Exception", "The user has elected to close the alert dialog and continue execution.");
                }

                ActionHideDisplay(DialogResult.Ignore, false); //closest analog to what close means.
            }
        }

        private void ActionRestart()
        {
            lock(m_Lock)
            {
                Log.WriteMessage(LogMessageSeverity.Critical, LogWriteMode.Queued, "Gibraltar", "Exception", null, null, null, null,
                    "User Restarting Application after Exception", "The user has elected to restart the application after viewing exceptions in the alert dialog.");
                Log.EndSession(SessionStatus.Crashed, null, "User Restarting Application after Exception");
                ActionHideDisplay(DialogResult.Abort, false); //closest analog to what restart means.

                //we have been getting an odd failure with this, lets log it right here.
                try
                {
                    //Application.Restart();
                    InvokeApplicationRestart(); // Invoke the Application.Restart() call asynchronously on the client UI thread.
                }
                catch (Exception ex)
                {
                    Log.WriteMessage(LogMessageSeverity.Warning, LogWriteMode.Queued, "Gibraltar", "Exception", null, null, ex, null, 
                        "Unable to trigger application restart", "An internal framework exception has prevented the application restart from completing.");
                }
            }
        }

        private void SizeControl()
        {
            string statusMessage = string.Empty;

            //when we are expanded we don't bother with the summary because it's redundant.
            lblErrorSummary.Visible = !m_Expanded;
            chkReportProblems.Visible = (m_Expanded && Log.CanSendSessionsOnExit(ref statusMessage));
            chkIgnoreAllProblems.Visible = (m_Expanded || g_AlertDialogPreviouslyClosed); //if we have already displayed once OR are in detail view.

            //resize the message flow panel
            messageFlowLayout.PerformLayout();
            int preferredHeight = messageFlowLayout.GetPreferredSize(new Size(messageFlowLayout.Width, 0)).Height; //calculate auto height preserving width
            if (messageFlowLayout.Height != preferredHeight)
            {
                messageFlowLayout.Height = preferredHeight;
            }

            exceptionListViewer.Visible = m_Expanded;
            exceptionListViewer.Top = Math.Max(messageFlowLayout.Top + messageFlowLayout.Height + messageFlowLayout.Margin.Bottom, m_MinimumHeaderHeight) + exceptionListViewer.Margin.Top;
            Control referenceControl = exceptionListViewer.Visible ? (Control)exceptionListViewer : messageFlowLayout;

            int contentTop = referenceControl.Top + referenceControl.Height + referenceControl.Margin.Bottom;

            //to adjust the bottom we have to change the overall form size since it's docked.  this will move down stuff that's stuck to the bottom.
            int height = contentTop + chkIgnoreAllProblems.Margin.Top + chkIgnoreAllProblems.Height + chkIgnoreAllProblems.Margin.Bottom + buttonPanel.Height;
            if (chkReportProblems.Visible)
                height += chkReportProblems.Margin.Top + chkReportProblems.Height + chkReportProblems.Margin.Bottom;

            Size = SizeFromClientSize(new Size(ClientSize.Width, height)); //we have to do it this way because we know the INNER area, not the outer area.
        }

        #endregion

        #region Event Handlers

        private void lblShowHideDetails_MouseEnter(object sender, EventArgs e)
        {
            lblShowHideDetails.ImageKey= (m_Expanded ? "arrow_up_color" : "arrow_down_color");
        }

        private void lblShowHideDetails_MouseLeave(object sender, EventArgs e)
        {
            lblShowHideDetails.ImageKey = (m_Expanded ? "arrow_up_bw" : "arrow_down_bw");
        }

        private void lblShowHideDetails_MouseUp(object sender, MouseEventArgs e)
        {
            lblShowHideDetails.ImageKey = (m_Expanded ? "arrow_up_color" : "arrow_down_color");
        }

        private void lblShowHideDetails_MouseDown(object sender, MouseEventArgs e)
        {
            lblShowHideDetails.ImageKey = (m_Expanded ? "arrow_up_color_pressed" : "arrow_down_color_pressed");
        }

        private void lblShowHideDetails_Click(object sender, EventArgs e)
        {
            m_Expanded = !m_Expanded;
            lblShowHideDetails.Text = (m_Expanded ? "        Hide details" : "        Show details");

            SizeControl(); //does all of the other UI changes necessary for expand change.
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            ActionRestart();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ActionClose();
        }

        private void chkIgnoreAllProblems_CheckedChanged(object sender, EventArgs e)
        {
            m_IgnoreAllProblems = chkIgnoreAllProblems.Checked; // This won't take effect until we're closed.
        }

        private void chkReportProblems_CheckedChanged(object sender, EventArgs e)
        {
            Log.SendSessionsOnExit = chkReportProblems.Checked;
        }

        #endregion
    }
}
