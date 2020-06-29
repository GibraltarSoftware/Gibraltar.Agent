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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

namespace Gibraltar.Monitor.Windows.Internal
{
    internal partial class UIMessageDetails : UserControl
    {
        private ILogMessage m_LogMessage;
        private bool m_AnyDetailsXmlSeen;
        private bool m_AnySourceCodeSeen;

        public UIMessageDetails()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);
            messageTabControl.TabPages.Remove(sourceCodePanel); // Hide this unless/until we need it.
            messageTabControl.TabPages.Remove(detailXmlPanel); // Hide this unless/until we need it.
            messageTabControl.SelectedTab = detailPanel; // Make sure this is selected by default.
        }

        /// <summary>
        /// Get or set the current log message that is being displayed
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ILogMessage LogMessage
        {
            get
            {
                return m_LogMessage;
            }
            set
            {
                SetDisplayMessage(value);
            }
        }

        /// <summary>
        /// Indicates if the description text area should use a fixed width font and suppress line wrap
        /// </summary>
        [Description("Indicates if the description text area should use a fixed width font and suppress line wrap")]
        [DefaultValue("false")]
        public bool ShowDescriptionPreformatted
        {
            get
            {
                return logMessage.ShowDescriptionPreformatted;
            }
            set
            {
                logMessage.ShowDescriptionPreformatted = value;
            }
        }

        private void DisposeLooseTabs(bool disposing)
        {
            if (disposing)
            {
                if (sourceCodePanel != null && sourceCodePanel.IsDisposed == false)
                {
                    if (messageTabControl.Controls.Contains(sourceCodePanel) == false)
                        sourceCodePanel.Dispose();
                    // Otherwise, messageTabControl should dispose it itself.
                }

                if (detailXmlPanel != null && detailXmlPanel.IsDisposed == false)
                {
                    if (messageTabControl.Controls.Contains(detailXmlPanel) == false)
                        detailXmlPanel.Dispose();
                    // Otherwise, messageTabControl should dispose it itself.
                }
            }
        }

        /// <summary>
        /// Displays extended information for the provided row's data object or nothing if no row is provided.
        /// </summary>
        /// <param name="message">The log message object to display or null to clear the current detail display</param>
        private void SetDisplayMessage(ILogMessage message)
        {
            m_LogMessage = message;

            if (m_LogMessage == null)
            {
                //nothing to see here, 
                //Log.Write(LogMessageSeverity.Information, "Gibraltar.User Interface.Session Viewer", "Display message details called with no message.", null);
                logMessage.LogMessage = null;
                exceptionDetail.ExceptionInfo = null;
                detailsXmlViewer.LogMessage = null;
                sourceCodeViewer.LogMessage = null;
            }
            else
            {
                bool suspendedLayout = false;

                // If this message has XML details enable our XML Details viewer panel.
                if (m_AnyDetailsXmlSeen == false && string.IsNullOrEmpty(m_LogMessage.Details) == false)
                {
                    messageTabControl.SuspendLayout();
                    suspendedLayout = true;

                    m_AnyDetailsXmlSeen = true;
                    messageTabControl.TabPages.Insert(2, detailXmlPanel); // Add the detail XML panel in the desired position.
                }

                // If this message has source code enable our Source Code viewer panel.
                if (m_AnySourceCodeSeen == false && m_LogMessage.HasSourceLocation)
                {
                    if (suspendedLayout == false)
                    {
                        messageTabControl.SuspendLayout();
                        suspendedLayout = true;
                    }

                    m_AnySourceCodeSeen = true;
                    messageTabControl.TabPages.Add(sourceCodePanel); // Add the source code panel now that we need it.
                }

                if (suspendedLayout)
                    messageTabControl.ResumeLayout(true);

                //display the details for this row.
                logMessage.LogMessage = m_LogMessage;
                exceptionDetail.ExceptionInfo = m_LogMessage.Exception;
                detailsXmlViewer.LogMessage = m_LogMessage;
                sourceCodeViewer.LogMessage = m_LogMessage;
            }
        }

        private void exceptionDetail_TitleChanged(object sender, EventArgs e)
        {
            if (messageTabControl.TabPages.Contains(exceptionPanel) == false)
                return;

            //we have to update the caption for the exception panel
            exceptionPanel.Text = exceptionDetail.Title;

            //and a little hokiness...  
            if (exceptionDetail.ExceptionInfo != null)
            {
                //make it our red flag
                exceptionPanel.ImageKey = "flag_red";
            }
            else
            {
                //nothing to see here.. move along
                exceptionPanel.ImageKey = "flag_green";
            }
        }

        private void detailsXmlViewer_TitleChanged(object sender, EventArgs e)
        {
            //we have to update the caption for the details panel
            if (messageTabControl.TabPages.Contains(detailXmlPanel))
                detailXmlPanel.Text = detailsXmlViewer.Title;
        }

        private void sourceCodeViewer_TitleChanged(object sender, EventArgs e)
        {
            //we have to update the caption of the source code panel
            if (messageTabControl.TabPages.Contains(sourceCodePanel))
                sourceCodePanel.Text = sourceCodeViewer.Title;
        }

        private void messageTabControl_Selected(object sender, TabControlEventArgs e)
        {
            //if the user is switching to/from the source panel we need to show/hide the viewer so it knows 
            //whether it should render the source code or not.
            if (ReferenceEquals(e.TabPage, sourceCodePanel))
            {
                sourceCodeViewer.Visible = true; // The sourceCodePanel is the selected tab, so we need to display.
            }
            else
            {
                if (sourceCodeViewer.Visible) // If we're not already hidden...
                {
                    sourceCodeViewer.Visible = false; // We need to hide the sourceCodeViewer so it doesn't waste time rendering.
                }
            }
        }
    }
}
