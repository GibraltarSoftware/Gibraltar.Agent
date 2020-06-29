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
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Internal;
using Gibraltar.Monitor.Windows.Internal;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

namespace Gibraltar.Monitor.Windows
{
#pragma warning disable 1591
    /// <summary>
    /// A control to display the details of the Exception attached to a LogMessage (including each InnerException).
    /// </summary>
    public partial class UIExceptionDetail : UserControl
    {
        private const string MenuKeyCopyStackTrace = "fnCopyStackTrace";
        private const string MenuKeyCopyStackTraceDiv = "fnCopyStackTraceDiv";

        IExceptionInfo m_ExceptionInfo;
        private string m_Title;

        private readonly List<UIException> m_ExceptionControls = new List<UIException>();

        public event EventHandler TitleChanged;

        /// <summary>
        /// Create a blank UIExceptionDetail control.
        /// </summary>
        public UIExceptionDetail()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);
        }

        #region Protected Properties and Methods

        protected virtual string CalculateTitle()
        {
            string newTitle;

            if (m_ExceptionInfo == null)
            {
                newTitle = "Exceptions";
            }
            else
            {
                //so just give the count
                int exceptionCount = 0;
                var innerException = m_ExceptionInfo;
                while(innerException != null)
                {
                    exceptionCount++;
                    innerException = innerException.InnerException;
                }

                newTitle = string.Format(CultureInfo.CurrentCulture, "Exceptions ({0:N0})", exceptionCount);
            }

            return newTitle;
        }

        #endregion

        #region Private properties and Methods

        private void ActionCopyAll()
        {
            var exceptionText = new StringBuilder();
            var curException = m_ExceptionInfo;
            while (curException != null)
            {
                if (string.IsNullOrEmpty(curException.Source) == false)
                    exceptionText.AppendFormat("{0}: {1}\r\n", curException.Source, curException.TypeName);
                else
                    exceptionText.AppendLine(curException.TypeName);

                if (string.IsNullOrEmpty(curException.Message) == false)
                    exceptionText.AppendLine(curException.Message);

                if (string.IsNullOrEmpty(curException.StackTrace) == false)
                    exceptionText.AppendLine(curException.StackTrace);

                curException = curException.InnerException;
            }

            Clipboard.SetText(exceptionText.ToString());
        }

        /// <summary>
        /// Calculate the title for the current log message
        /// </summary>
        private void SetDisplayTitle()
        {
            //calculate the current title
            string newTitle = CalculateTitle();

            //and if it's different than what we have, set that and notify those that subscribe to our event
            if (newTitle != m_Title)
            {
                m_Title = newTitle;

                OnTitleChanged();
            }
        }

        /// <summary>
        /// Display the provided exception message.  If it is the same as the current object, do nothing.  
        /// If it is null, clear the current display.
        /// </summary>
        /// <param name="exceptionInfo">The exception message to display or null to clear display</param>
        private void SetDisplayExceptionMessage(IExceptionInfo exceptionInfo)
        {
            //Is this the same object we already have?  We want to skip
            //our work if so to improve performance
            if (m_ExceptionInfo == exceptionInfo)
            {
                if (m_ExceptionInfo == null)
                {
                    //all done
                    return;
                }

                IList<IExceptionInfo> exceptions = LogMessagePacket.ExceptionsList(m_ExceptionInfo);
                if (exceptions.Count == m_ExceptionControls.Count)
                {
#if REVERSE_EXCEPTION_ORDER
                    int exceptionIndex = exceptions.Length - 1;
#else
                    int exceptionIndex = 0;
#endif
                    //they must have updated the session list - recheck the data.
                    foreach (UIException exceptionControl in m_ExceptionControls)
                    {
                        // This doesn't actually change it, but makes it reevaluate the File.Exists() test for each link.
                        exceptionControl.DisplayException = exceptions[exceptionIndex];
                        // Make sure the index label matches.
                        exceptionControl.ExceptionIndex = exceptions.Count - exceptionIndex; // 1 for innermost, last in array.
#if REVERSE_EXCEPTION_ORDER
                        exceptionIndex--;
#else
                        exceptionIndex++;
#endif
                    }

                    SetDisplayTitle(); //whenever it might change, we call this to make sure we're up to date.
                    return; // Nothing more to do, so we can bail here.
                }
                // Otherwise, Uh-oh!  We aren't displaying the right number of exceptions, so we need to recalculate them.
            }

            flowExceptions.SuspendLayout(); // We're going to go nuts with a new layout.

            if (m_ExceptionInfo != null)
            {
                // Clean up any currently displayed exception controls.
                foreach (UIException exceptionControl in m_ExceptionControls)
                {
                    flowExceptions.Controls.Remove(exceptionControl);
                    exceptionControl.CopyAll -= OnExceptionControlCopyAll;
                    exceptionControl.Dispose();
                }
                m_ExceptionControls.Clear();

                // Clear our reference.
                m_ExceptionInfo = null;
            }

            // Set the new message reference.
            m_ExceptionInfo = exceptionInfo;

            if (m_ExceptionInfo != null)
            {
                var exceptions = LogMessagePacket.ExceptionsList(m_ExceptionInfo);
                if (exceptions.Count > 0) // Exceptions to show, or not...
                {
                    lblNoException.Visible = false;
                    flowExceptions.Visible = true;

#if REVERSE_EXCEPTION_ORDER
                    for (int i = exceptions.Length - 1; i >= 0; i--)
#else
                    for (int i = 0; i < exceptions.Count; i++)
#endif
                    {
                        UIException exceptionControl = new UIException();
                        exceptionControl.CopyAll += OnExceptionControlCopyAll;
                        m_ExceptionControls.Add(exceptionControl);
                        flowExceptions.Controls.Add(exceptionControl); // Add the exception control to the flow panel.
                        exceptionControl.DisplayException = exceptions[i];
                        exceptionControl.ExceptionIndex = exceptions.Count - i; // 1 for innermost, last in the array.
                    }

                    PropagateResize(); // Make sure the contained controls we just added get set to the right width.
                }
                else
                {
                    flowExceptions.Visible = false;
                    lblNoException.Visible = true;
                }
            }
            else
            {
                flowExceptions.Visible = false;
                lblNoException.Visible = true;
            }

            flowExceptions.ResumeLayout(true); // We've updated the contents, so now it needs to redo the layout.

            SetDisplayTitle(); //whenever it might change, we call this to make sure we're up to date.
        }

        private void OnExceptionControlCopyAll(object sender, EventArgs e)
        {
            ActionCopyAll();
        }

        protected virtual void OnTitleChanged()
        {
            //save the delegate field in a temporary field for thread safety
            EventHandler tempEvent = TitleChanged;

            if (tempEvent != null)
            {
                tempEvent(this, EventArgs.Empty);
            }
        }

        private void PropagateResize()
        {
            //change the width of everything in the pane to fill width.
            int panelInnerWidth = flowExceptions.Width - flowExceptions.Padding.Left - flowExceptions.Padding.Right;
            foreach (Control control in flowExceptions.Controls)
            {
                control.Width = panelInnerWidth - control.Left - control.Margin.Right;
            }
        }

        #endregion

        #region Public Properties and Methods

        [Browsable(false)]
        public string Title
        {
            get { return m_Title; }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public IExceptionInfo ExceptionInfo
        {
            get
            {
                return m_ExceptionInfo;
            }
            set
            {
                SetDisplayExceptionMessage(value);
            }
        }

        #endregion

        #region Event Handlers

        private void flowExceptions_Resize(object sender, EventArgs e)
        {
            PropagateResize();
        }

        /*
        private void exceptionGrid_ContextMenuCreate(object sender, Gibraltar.Windows.UI.ContextMenuEventArgs e)
        {
            //we need to add in the commands we want to have in the context menu
            UICommand uicCopyStackTrace = new UICommand(MenuKeyCopyStackTrace, "Copy Stack Trace...", CommandType.Command);
            uicCopyStackTrace.TextAlignment = ContentAlignment.MiddleLeft;
            e.ContextMenu.Commands.Add(uicCopyStackTrace);
            uicCopyStackTrace.Click += uicCopyStackTrace_Click;
            uicCopyStackTrace.DefaultItem = Janus.Windows.UI.InheritableBoolean.True;

            e.ContextMenu.Commands.Add(new UICommand(MenuKeyCopyStackTraceDiv, "-", CommandType.Separator));

        }

        private void uicCopyStackTrace_Click(object sender, EventArgs e)
        {
            //copy out the stack trace of the current row.
            UIException senderControl = sender as UIException;
            if (senderControl != null)
            {
                ExceptionInfo currentException = senderControl.DisplayException;

                if (currentException != null)
                {
                    Clipboard.SetText(currentException.StackTrace);
                }
            }
        }
        */

        #endregion
    }
}
