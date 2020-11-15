
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;
using Manoli.Utils.CSharpFormat;

#endregion

namespace Gibraltar.Monitor.Windows
{
#pragma warning disable 1591
    /// <summary>
    /// Displays the XML detail section of a log messages
    /// </summary>
    public partial class UILogMessageDetail : UserControl
    {
        private ILogMessage m_LogMessage;
        private string m_Title;
        private bool m_DocumentStale;

        public event EventHandler TitleChanged;

        public UILogMessageDetail()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);
        }

        #region Public Properties and Methods

        [Browsable(false)]
        public string Title
        {
            get { return m_Title; }
        }

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
                SetDisplayDetails(value);
            }
        }

        #endregion

        #region Protected Properties and Methods

        protected virtual string CalculateTitle()
        {
            string newTitle;

            if ((m_LogMessage == null) || (string.IsNullOrEmpty(m_LogMessage.Details)))
            {
                newTitle = "No Details Available";
            }
            else
            {
                //so just give the count
                newTitle = "Details";
            }

            return newTitle;
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

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (m_DocumentStale)
                RenderDocument();
        }

        #endregion

        #region Private properties and Methods

        /// <summary>
        /// Place the current log details on the clipboard
        /// </summary>
        private void ActionCopy()
        {
            if (m_LogMessage != null)
            {
                Clipboard.Clear();
                Clipboard.SetText(m_LogMessage.Details);
            }
        }

        /// <summary>
        /// Save the current log details to a file.
        /// </summary>
        private void ActionSaveAs()
        {
            if (m_LogMessage != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = FileDialogFilter;
                    saveFileDialog.Title = "Save Log Message Details";
                    saveFileDialog.FileName = "Detail.xml";
                    
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, m_LogMessage.Details);
                    }
                }
            }            
        }

        private const string FileDialogFilter = "Extensible Markup Language (XML)|*.xml|Any File|*.*";

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
        /// Display the details associated with the provided log message.
        /// </summary>
        /// <param name="logMessage">The log message to display or null to clear display</param>
        private void SetDisplayDetails(ILogMessage logMessage)
        {
            //Is this the same object we already have?  We want to skip
            //our work if so to improve performance
            if (m_LogMessage == logMessage)
            {
                //all done
                return;
            }

            m_LogMessage = logMessage;

            string detailsXml = null;
            if (m_LogMessage != null)
            {
                detailsXml = m_LogMessage.Details;
            }

            //did we find an xml document?
            if (string.IsNullOrEmpty(detailsXml) == false)
            {
                //we don't render the document now if we're not visible.
                if (Visible)
                {
                    RenderDocument();
                }
                else
                {
                    m_DocumentStale = true;
                }
            }
            else
            {
                //no information available = we need to hide our source code viewer and show our NO Information display
                lblNoDetails.Visible = true;
                detailsView.Visible = false;
            }

            SetDisplayTitle(); //whenever it might change, we call this to make sure we're up to date.
        }

        private void RenderDocument()
        {
            m_DocumentStale = false;
            string detailsXml = null;
            if (m_LogMessage != null)
            {
                detailsXml = m_LogMessage.Details;
            }

            //did we find any details?
            if (string.IsNullOrEmpty(detailsXml) == false)
            {
                //OK, try to parse the xml and reformat it to be pretty.  
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, null))
                        {
                            XmlDocument document = new XmlDocument();
                            document.LoadXml(detailsXml);
                            xmlTextWriter.Formatting = Formatting.Indented;
                            document.WriteContentTo(xmlTextWriter);

                            //and force everything to flow through to the lower streams.
                            xmlTextWriter.Flush();
                            stream.Flush();

                            //now we have to re-read it to get the formatted string.
                            stream.Position = 0;

                            string prettyXml;
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                prettyXml = streamReader.ReadToEnd();
                            }
                            detailsXml = prettyXml;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Session Viewer.Log Message Detail", "Unable to pretty print XML, probably because source isn't a valid XML document.", "The Xml format routine threw an exception when attempting to reformat the xml details:\r\n{0}", ex.Message);
                }

                detailsView.Text = detailsXml;
                lblNoDetails.Visible = false;
                detailsView.Visible = true;
            }
            else
            {
                lblNoDetails.Visible = true;
                detailsView.Visible = false;
            }
        }

        #endregion

        #region Event Handlers

        /*
        private void cmdEditCopy_Click(object sender, CommandEventArgs e)
        {
            ActionCopy();
        }

        private void cmdFileSaveAs_Click(object sender, CommandEventArgs e)
        {
            ActionSaveAs();
        }
        */

        #endregion

    }
}
