
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Gibraltar.Monitor;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;
using Manoli.Utils.CSharpFormat;

#endregion

namespace Gibraltar.Monitor.Windows
{
#pragma warning disable 1591
    public partial class UISourceViewer : UserControl
    {
        private readonly Dictionary<string, string> m_FormattedSource = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string m_CurrentSourceCodeFile;
        private int m_CurrentLineNumber;

        ILogMessage m_LogMessage;

        private string m_Title;
        private bool m_SourceStale;

        public event EventHandler TitleChanged;

        public UISourceViewer()
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
                SetDisplaySourceCode(value);
            }
        }

        #endregion

        #region Protected Properties and Methods

        protected virtual string CalculateTitle(string sourceCodeFileNamePath, int lineNumber)
        {
            string newTitle;

            if (string.IsNullOrEmpty(sourceCodeFileNamePath))
            {
                newTitle = "Source Not Available";
            }
            else
            {
                //so just give the count
                if (lineNumber > 0)
                {
                    newTitle = string.Format(CultureInfo.CurrentCulture, "{0} ({1:N0})", Path.GetFileName(sourceCodeFileNamePath), lineNumber);
                }
                else
                {
                    newTitle = Path.GetFileName(sourceCodeFileNamePath);
                }
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

            if (m_SourceStale && Visible) // Don't go rendering unless we're actually now Visible.
                RenderSourceCode();
        }

        #endregion

        #region Private properties and Methods

        private static string FormatSourceCode(string fileNamePath)
        {
            string formattedSourceCode;

            using (StreamReader sourceCodeFile = File.OpenText(fileNamePath))
            {
                string rawSourceCode = sourceCodeFile.ReadToEnd();

                //now, can we format it up?
                string fileExtension = Path.GetExtension(fileNamePath);
                SourceFormat codeFormatter;
                switch (fileExtension)
                {
                    case ".cs":
                        codeFormatter = new CSharpFormat();
                        break;
                    case ".vb":
                        codeFormatter = new VisualBasicFormat();
                        break;
                    case ".sql":
                        codeFormatter = new TsqlFormat();
                        break;
                    case ".html":
                    case ".htm":
                    case ".aspx":
                    case ".xml":
                        codeFormatter = new HtmlFormat();
                        break;
                    case ".js":
                        codeFormatter = new JavaScriptFormat();
                        break;
                    default:
                        //HTML format is better than nothing....
                        codeFormatter = new HtmlFormat();
                        break;
                }

                try
                {
                    codeFormatter.LineNumbers = true;
                    codeFormatter.Alternate = true;
                    codeFormatter.EmbedStyleSheet = true; 
                    formattedSourceCode = codeFormatter.FormatCode(rawSourceCode);
                }
                catch
                {
                    //just switch back to unformatted
                    formattedSourceCode = rawSourceCode;
                }
            }

            return formattedSourceCode;
        }

        /// <summary>
        /// Calculate the title for the current log message
        /// </summary>
        private void SetDisplayTitle(string sourceCodeFileNamePath, int lineNumber)
        {
            //calculate the current title
            string newTitle = CalculateTitle(sourceCodeFileNamePath, lineNumber);

            //and if it's different than what we have, set that and notify those that subscribe to our event
            if (newTitle != m_Title)
            {
                m_Title = newTitle;

                OnTitleChanged();
            }
        }

        /// <summary>
        /// Display the source code associated with the provided log message.
        /// </summary>
        /// <param name="logMessage">The log message to display or null to clear display</param>
        private void SetDisplaySourceCode(ILogMessage logMessage)
        {
            //Is this the same object we already have?  We want to skip
            //our work if so to improve performance
            if (m_LogMessage == logMessage)
            {
                //all done
                return;
            }

            m_LogMessage = logMessage;

            string sourceCodeFileNamePath = null;
            int lineNumber = 0;
            if ((m_LogMessage != null) && (m_LogMessage.HasSourceLocation))
            {
                sourceCodeFileNamePath = m_LogMessage.FileName;
                lineNumber = m_LogMessage.LineNumber;
            }

            //did we find a source code file?
            if (string.IsNullOrEmpty(sourceCodeFileNamePath) == false)
            {
                //we don't render the source code now if we're not visible.
                if (Visible)
                {
                    RenderSourceCode();
                }
                else
                {
                    m_SourceStale = true;
                }
            }
            else
            {
                //no information available = we need to hide our source code viewer and show our NO Information display
                m_CurrentSourceCodeFile = null;
                m_CurrentLineNumber = 0;
                lblNoSource.Visible = true;
                sourcePreview.Visible = false;
            }

            SetDisplayTitle(sourceCodeFileNamePath, lineNumber); //whenever it might change, we call this to make sure we're up to date.
        }

        private void RenderSourceCode()
        {
            m_SourceStale = false;
            string sourceCodeFileNamePath = null;
            int lineNumber = 0;
            if ((m_LogMessage != null) && (m_LogMessage.HasSourceLocation))
            {
                sourceCodeFileNamePath = m_LogMessage.FileName;
                lineNumber = m_LogMessage.LineNumber;
            }

            //did we find a source code file?
            if (string.IsNullOrEmpty(sourceCodeFileNamePath) == false)
            {
                //is it what we're already displaying?
                if (sourceCodeFileNamePath.Equals(m_CurrentSourceCodeFile) == false)
                {
                    //OK, see if it's in cache or if we can load the file.
                    string formattedSourceCode;
                    if (m_FormattedSource.TryGetValue(sourceCodeFileNamePath, out formattedSourceCode) == false)
                    {
                        //we don't have it - we'll have to load and format it.
                        if (File.Exists(sourceCodeFileNamePath))
                        {
                            formattedSourceCode = FormatSourceCode(sourceCodeFileNamePath);
                            m_FormattedSource.Add(sourceCodeFileNamePath, formattedSourceCode);
                        }
                    }

                    if (string.IsNullOrEmpty(formattedSourceCode) == false)
                    {
                        sourcePreview.DocumentText = formattedSourceCode;
                        lblNoSource.Visible = false;
                        sourcePreview.Visible = true;
                    }
                    else
                    {
                        lblNoSource.Visible = true;
                        sourcePreview.Visible = false;
                    }

                    //and set this as the new source code file, no line number yet.
                    m_CurrentSourceCodeFile = sourceCodeFileNamePath;
                    m_CurrentLineNumber = 0;
                }

                //now at this point we're on the right file - all we have to do is search for the line number.
                if ((sourcePreview.Visible) && (lineNumber > 0))
                {
                    //unhighlight the current highlighted line
                    if (m_CurrentLineNumber != 0)
                    {
                        SetLineHighlight(m_CurrentLineNumber, false);
                    }

                    //and highlight the new line
                    m_CurrentLineNumber = lineNumber;
                    SetLineHighlight(m_CurrentLineNumber, true);
                }
            }
        }

        private void SetLineHighlight(int lineNumber, bool highlight)
        {
            string divName = LineNumberToElementId(lineNumber);

            //now try to find it in the web browser
            try
            {
                if (sourcePreview.Document != null)
                {
                    HtmlElement currentLine = sourcePreview.Document.GetElementById(divName);

                    if (currentLine != null)
                    {
                        string currentClass = currentLine.GetAttribute("className"); //really.  it's not class.
                        if ((string.IsNullOrEmpty(currentClass) == false) && (currentClass.StartsWith("alt")))
                        {
                            //this is our alternating line
                            currentClass = highlight ? "alt-highlight" : "alt";
                        }
                        else
                        {
                            currentClass = highlight ? "highlight" : string.Empty;
                        }

                        currentLine.SetAttribute("className", currentClass);

                        //and finally, if this is a set-to-highlight call we need to make sure this is in view.
                        if (highlight)
                        {
                            // Ideally we want to show a few lines ahead as well as lines before the highlight.
                            // Let's aim for about 2/3 above and 1/3 below.
                            int previewHeight = sourcePreview.ClientRectangle.Height; // Height of the display area.
                            int lineHeight = currentLine.OffsetRectangle.Height; // Height of the individual (non-wrapping) line.
                            int playHeight = (previewHeight - lineHeight); // Pretend it's a bit smaller (horizontal scrollbar...).
                            HtmlElement targetLine = null;
                            if (playHeight > 0 && lineHeight > 0) // Sanity check.  No funny business or divide-by-zero.
                            {
                                int linesBack = (2 * playHeight) / (3 * lineHeight); // Compute 2/3 of total viewable lines, rounded down.

                                int targetLineNumber = (lineNumber > linesBack) ? (lineNumber - linesBack) : 1;
                                string targetId = LineNumberToElementId(targetLineNumber);
                                targetLine = sourcePreview.Document.GetElementById(targetId);
                            }

                            if (targetLine != null) // Did we find the targeted line?
                                targetLine.ScrollIntoView(true); // Align the targeted line to the top of the view.
                            else
                                currentLine.ScrollIntoView(false); // Punt!  Align the highlighted line to the bottom of the view.
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static string LineNumberToElementId(int lineNumber)
        {
            string divName = string.Format(CultureInfo.InvariantCulture, "line{0}", lineNumber);

            return divName;
        }

        #endregion

        #region Event Handlers

        private void sourcePreview_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (m_CurrentLineNumber > 0)
            {
                //and highlight the new line
                SetLineHighlight(m_CurrentLineNumber, true);                
            }
        }

        #endregion
    }
}
