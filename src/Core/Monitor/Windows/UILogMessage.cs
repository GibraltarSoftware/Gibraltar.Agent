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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Gibraltar.Monitor;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows
{
#pragma warning disable 1591
    /// <summary>
    /// A control to display information about a specific LogMessage.
    /// </summary>
    public partial class UILogMessage : UserControl
    {
        private static readonly string[] LineBreakTokens = new[] { "\r\n", "\n\r", "\n", "\r" };

        private static int LocationRowIndex = 1;

        private ILogMessage m_LogMessage;
        private bool m_DesignerCleared;
        private bool m_ShowThreadInfo;
        private bool m_ShowLocationInfo;
        private bool m_ShowDescriptionPreformatted;

        /// <summary>
        /// Create a new UILogMessage.
        /// </summary>
        public UILogMessage()
        {
            InitializeComponent();

//            FormTools.ApplyOSFont(this);
            
            //All of our controls should use the OS font as-is, so we just set that.
            lblCategoryName.Font = FormTools.OSFont;
            lblClassName.Font = FormTools.OSFont;
            lblLocation.Font = FormTools.OSFont;
            lblMethodName.Font = FormTools.OSFont;
            lblThreadId.Font = FormTools.OSFont;
            lblThreadName.Font = FormTools.OSFont;
            lblThreadType.Font = FormTools.OSFont;

            txtCategoryName.Font = FormTools.OSFont;
            txtClassName.Font = FormTools.OSFont;
            txtDescription.Font = FormTools.OSFont;
            txtLocation.Font = FormTools.OSFont;
            txtMethodName.Font = FormTools.OSFont;
            txtThreadId.Font = FormTools.OSFont;
            txtThreadName.Font = FormTools.OSFont;
            txtThreadType.Font = FormTools.OSFont;

            //set our defaults so we're ready for whatever property sets happen by the form designer
            m_ShowThreadInfo = true;
            m_ShowLocationInfo = true;
        }


        #region Public Properties and Methods

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
                SetDisplayLogMessage(value);
            }
        }

        [Description("Indicates if thread information should be displayed")]
        [DefaultValue("true")]
        public bool ShowThreadInfo
        {
            get
            {
                return m_ShowThreadInfo;
            }
            set
            {
                SetDisplayThreadInfo(value);
            }
        }

        [Description("Indicates if source code location information should be displayed")]
        [DefaultValue("true")]
        public bool ShowLocationInfo
        {
            get
            {
                return m_ShowLocationInfo;
            }
            set
            {
                SetDisplayLocationInfo(value);
            }
        }

        [Description("Indicates if the description text area should use a fixed width font and suppress line wrap")]
        [DefaultValue("false")]
        public bool ShowDescriptionPreformatted
        {
            get
            {
                return m_ShowDescriptionPreformatted;
            }
            set
            {
                SetDisplayDescriptionPreformatted(value);
            }
        }

        #endregion    

        #region Private Properties and Methods

        private void SetDisplayBackColor()
        {
            //set it as the background of everything we display that isn't transparent
            txtCategoryName.BackColor = BackColor;
            txtClassName.BackColor = BackColor;
            txtDescription.BackColor = BackColor;
            txtLocation.BackColor = BackColor;
            txtMethodName.BackColor = BackColor;
            txtThreadId.BackColor = BackColor;
            txtThreadName.BackColor = BackColor;
            txtThreadType.BackColor = BackColor;
        }

        private void SetDisplayLogMessage(ILogMessage logMessage)
        {
            //make sure this is a new message before we bother doing anything for performance reasons
            if (m_LogMessage == logMessage && m_DesignerCleared)
            {
                return;
            }
            
            //OK, we're going to display it. Proceed!
            m_LogMessage = logMessage;

            //handle the case where it's all null!
            if (m_LogMessage == null)
            {
                txtCategoryName.Text = string.Empty;
                txtClassName.Text = string.Empty;
                txtMethodName.Text = string.Empty;
                txtLocation.Text = string.Empty;
                locationPanel.Visible = false;
                txtThreadId.Text = string.Empty;
                txtThreadName.Text = string.Empty;
                txtThreadType.Text = string.Empty;
                txtDescription.Lines = null;
            }
            else
            {
                //we have a log message - now display it!

                // First, a little tweaking of the message details...
                string messageDetails = m_LogMessage.Caption;
                if (string.IsNullOrEmpty(messageDetails))
                {
                    // Make sure it's sane, just in case.
                    messageDetails = string.Empty;
                }
                else
                {

                    messageDetails = messageDetails.Trim();

                    //if we are going to append description we want to be sure there's some form of punctuation.
                    if (string.IsNullOrEmpty(m_LogMessage.Description) == false)
                    {
                        char lastChar = messageDetails[messageDetails.Length - 1];

                        if ((lastChar != '.') && (lastChar != ':') && (lastChar != '-'))
                            messageDetails += ":"; //add a nice colon to make it spiffy.
                    }

                    messageDetails += "\r\n"; //for easier selection (when there's just caption) and as a line break for caption/description.
                }

                //now add on the description
                if (string.IsNullOrEmpty(m_LogMessage.Description) == false)
                {
                    messageDetails += m_LogMessage.Description;

                    // Append a final line-break, if there wasn't one.  Makes text selection easier.
                    char lastChar = messageDetails[messageDetails.Length - 1];
                    if (lastChar != '\r' && lastChar != '\n')
                        messageDetails += "\r\n"; //no colon here because it's the end.
                }

                // We have to split the message text into an array of lines, or it won't handle plain \n linebreaks.
                string[] lines = messageDetails.Split(LineBreakTokens, StringSplitOptions.None);

                txtDescription.Lines = lines; // Then set it from the lines array, to set it with the correct line breaks.

                txtCategoryName.Text = m_LogMessage.CategoryName;

                //not all messages are equal, don't just blindly assume we'll have a call info object.
                if (m_LogMessage.HasMethodInfo == false)
                {
                    txtClassName.Text = "(unknown)";
                    txtMethodName.Text = "(unknown)";
                    locationPanel.Visible = false;
                }
                else
                {
                    txtClassName.Text = m_LogMessage.ClassName;
                    txtMethodName.Text = m_LogMessage.MethodName;
                }

                //see if we actually have a line number & file name.  If file name is empty, hide it.
                if (m_LogMessage.HasSourceLocation == false)
                {
                    locationPanel.Visible = false;
                }
                else
                {
                    locationPanel.Visible = m_ShowLocationInfo;

                    //only specify the line number if we really got one. Otherwise it looks stupid.
                    if (m_LogMessage.LineNumber > 0)
                    {
                        txtLocation.Text = string.Format(CultureInfo.InvariantCulture, "Line {0} of file '{1}'",
                                                         m_LogMessage.LineNumber, m_LogMessage.FileName);
                    }
                    else
                    {
                        txtLocation.Text = m_LogMessage.FileName;
                    }
                }

                //not all messages are equal, don't just blindly assume we'll have a thread info object.
                if (!m_LogMessage.HasThreadInfo)
                {
                    txtThreadId.Text = "(unknown)";
                    txtThreadName.Text = "(unknown)";
                    txtThreadType.Text = "(unknown)";
                }
                else
                {
                    txtThreadId.Text = m_LogMessage.ThreadId.ToString(CultureInfo.InvariantCulture);
                    txtThreadName.Text = m_LogMessage.ThreadName;

                    string threadType = m_LogMessage.IsBackground ? "Background" : "Foreground";

                    if (m_LogMessage.IsThreadPoolThread)
                    {
                        threadType += ", Pooled";
                    }

                    txtThreadType.Text = threadType;
                }
            }

            m_DesignerCleared = true; // Enable efficiency short-circuit.
        }

        private void SetDisplayLocationInfo(bool visible)
        {
            //don't process it if there are no changes, that can lead to bad performance.
            if (visible == m_ShowLocationInfo)
                return;

            m_ShowLocationInfo = visible;

            locationPanel.Visible = m_ShowLocationInfo;

            mainLayoutPanel.RowStyles[LocationRowIndex].Height = m_ShowLocationInfo ? txtLocation.Height : 0;
        }

        private void SetDisplayThreadInfo(bool visible)
        {
            //don't process it if there are no changes, that can lead to bad performance.
            if (visible == m_ShowThreadInfo)
                return;

            m_ShowThreadInfo = visible;

            threadPanel.Visible = m_ShowThreadInfo;
        }

        private void SetDisplayDescriptionPreformatted(bool value)
        {
            if (value == m_ShowDescriptionPreformatted)
                return;

            m_ShowDescriptionPreformatted = value;

            if (m_ShowDescriptionPreformatted)
            {
                txtDescription.Font = FormTools.FixedWidthFont;
                txtDescription.WordWrap = false;
            }
            else
            {
                txtDescription.Font = FormTools.OSFont;
                txtDescription.WordWrap = true;
            }

            //and make sure our checkbox is in sync
            if (chkShowDescriptionPreformatted.Checked != value)
                chkShowDescriptionPreformatted.Checked = value;
        }

        #endregion

        #region Base class overrides

        protected override void OnBackColorChanged(EventArgs e)
        {
            //let the default event happen
            base.OnBackColorChanged(e);

            //but we also need to explicitly track that to our background
            SetDisplayBackColor();
        }

        #endregion

        #region Event Handlers

        private void txtLocation_Click(object sender, EventArgs e)
        {
            //try to open the file
            string fileNamePath = string.Empty; // A default value in case we can't even access it from m_LogMessage.FileName.
            try
            {
                fileNamePath = m_LogMessage.FileName;
                if (File.Exists(fileNamePath))
                {
                    System.Diagnostics.Process.Start(fileNamePath);
                }
            }
            catch(Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, "Gibraltar.User Interface.Session Viewer", "Unable to open source code file",
                          "File: '{0}'\r\nException ({1}): {2}", fileNamePath, ex.GetType().FullName, ex.Message);
            }
        }

        private void UILogMessage_Resize(object sender, EventArgs e)
        {
            bool showCheckbox = (Height > 300);

            mainLayoutPanel.RowStyles[mainLayoutPanel.RowCount - 1].Height = showCheckbox ? 20 : 0;
        }

        private void chkShowDescriptionPreformatted_CheckedChanged(object sender, EventArgs e)
        {
            //transfer the setting through to our property
            ShowDescriptionPreformatted = chkShowDescriptionPreformatted.Checked;
        }

        #endregion
    }
}
