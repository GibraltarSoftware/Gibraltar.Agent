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
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// A control to show an individual exception with its TypeName, Message, and StackTrace (and possibly Source).
    /// </summary>
    internal partial class UIException : UserControl
    {
        private const string LogCategory = "Gibraltar.User Interface.Session Viewer";

        private static readonly string[] LineBreakTokens = new[] {"\r\n", "\n\r", "\n", "\r"};
        private static readonly char[] LineBreakChars = new[] {'\r', '\n'};
        private static readonly string LineBreak = "\r\n"; // Used for character search, must contain both!
        //private static readonly char[] WhiteSpaceChars = new[] {' ', '\t', '\r', '\n'};
        //private static readonly string WhiteSpace = " \t\r\n";

        private IExceptionInfo m_DisplayException; 
        private int m_ExceptionIndex;
        private int m_CurrentWidth;
        private MethodInvoker m_CopyDelegate;

        // These are assumed not to change after the control is initialized.
        private readonly int m_RectangleWidth;
        private readonly int m_TypeMessageMarginWidth;
        private readonly int m_StackTraceMarginWidth;

        /// <summary>
        /// Raised when the user wants to put the entire exception collection on the clipboard
        /// </summary>
        public event EventHandler CopyAll;

        /// <summary>
        /// Construct a blank UIException control.
        /// </summary>
        public UIException()
        {
            InitializeComponent();
            FormTools.ApplyOSFont(this);

            m_RectangleWidth = panelLeft.Width;
            m_TypeMessageMarginWidth = txtTypeMessage.Margin.Left + txtTypeMessage.Margin.Right + m_RectangleWidth;
            m_StackTraceMarginWidth = lblStackTrace.Margin.Left + lblStackTrace.Margin.Right + m_RectangleWidth;

            // Clear out the default text (so we had something to look at in the designer) until we get an Exception assigned.
            SuspendLayout();
            txtTypeMessage.Text = string.Empty;
            lblStackTrace.Text = string.Empty;
            lblStackTrace.Links.Clear();
            ResumeLayout(true);
        }

        /// <summary>
        /// Gets or sets the ExceptionIndex value for the ExceptionInfo object being displayed on this control.
        /// (1 for the innermost--ie. original--exception).
        /// </summary>
        [Browsable(false)]
        public int ExceptionIndex
        {
            get { return m_ExceptionIndex; }
            set
            {
                if (value != m_ExceptionIndex)
                {
                    m_ExceptionIndex = value;
                    lblExceptionIndex.Text = value.ToString();
                    lblExceptionIndex.Visible = (value != 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ExceptionInfo object being displayed on this control.
        /// </summary>
        [Browsable(false)]
        public IExceptionInfo DisplayException // Or should this be a method call?
        {
            get { return m_DisplayException; }
            set
            {
                if (value != m_DisplayException)
                {
                    if (m_DisplayException != null)
                        lblStackTrace.Links.Clear();

                    m_DisplayException = value;
                    SuspendLayout();
                    if (value == null)
                    {
                        txtTypeMessage.Text = string.Empty;
                        lblStackTrace.Text = string.Empty;
                        ExceptionIndex = 0; // Blank out the index.
                    }
                    else
                    {
                        string message = value.Message ?? String.Empty; // Shouldn't be null, but make sure.
                        message = string.Join(LineBreak, message.Trim().Split(LineBreakTokens, StringSplitOptions.None));

                        string typeMessage = string.Format(string.IsNullOrEmpty(value.Source) ? "{1}\r\n{2}" : "{0} : {1}\r\n{2}",
                                                           value.Source, value.TypeName, message.Trim());
                        string stackTrace = (m_DisplayException.StackTrace ?? string.Empty).TrimEnd();

                        txtTypeMessage.Text = typeMessage;
                        lblStackTrace.Text = stackTrace;

                        ApplyStackTraceLinks(lblStackTrace.Links, stackTrace); 
                    }
                    ResumeLayout(true);
                }

                if (value != null)
                {
                    // Now that we've parsed it check the enabled status on the links. (Also recheck if reassigning the same.)
                    foreach (LinkLabel.Link link in lblStackTrace.Links)
                    {
                        string fileName = link.LinkData as string;
                        // If the file doesn't exist or we can't access it, mark it as disabled.
                        link.Enabled = ((string.IsNullOrEmpty(fileName) == false) && File.Exists(fileName));
                    }
                }
            }
        }

        private void ActionCopy()
        {
            MethodInvoker invoker = m_CopyDelegate;
            if (invoker == null)
                return;

            invoker.Invoke();
        }

        private void ActionCopyMessage()
        {
            Clipboard.Clear();
            Clipboard.SetText(txtTypeMessage.Text);
        }

        private void ActionCopyStackTrace()
        {
            Clipboard.Clear();
            Clipboard.SetText(m_DisplayException.StackTrace);
        }

        /// <summary>
        /// Display the copy button on the upper right of the target control.
        /// </summary>
        private void AssignCopyButton(Control target, MethodInvoker copyDelegate, bool supportCopyAll)
        {
            m_CopyDelegate = copyDelegate;
            target.SuspendLayout();

            btnCopy.Parent = target;
            btnCopy.Left = target.Width - btnCopy.Width - btnCopy.Margin.Right;
            btnCopy.Top = btnCopy.Margin.Top;
            btnCopy.Visible = true;

            if (supportCopyAll)
            {
                btnCopyAll.Parent = target;
                btnCopyAll.Left = btnCopy.Left - btnCopyAll.Width - btnCopyAll.Margin.Right;
                btnCopyAll.Top = btnCopy.Top;
                btnCopyAll.Visible = true;
            }
            else
            {
                btnCopyAll.Visible = false;
            }

            target.ResumeLayout();
        }

        /// <summary>
        /// Check the
        /// </summary>
        /// <param name="relativeLocation"></param>
        /// <returns></returns>
        private bool CheckIfInControl(out Point relativeLocation)
        {
            //if the mouse is no longer within our control then we better hide the button.
            relativeLocation = PointToClient(MousePosition);

            if ((relativeLocation.Y < 0) || (relativeLocation.Y > Height))
            {
                m_CopyDelegate = null;
                btnCopy.Visible = false;
                btnCopyAll.Visible = false;
                return false;
            }

            return true;
        }

        private void ClearCopyButton(Control target)
        {
            if (ReferenceEquals(btnCopy.Parent, target))
            {
                m_CopyDelegate = null;
                btnCopy.Visible = false;
                btnCopyAll.Visible = false;
            }
        }

        /// <summary>
        /// Parse a provided StackTrace string looking for file names and add links to the provided LinkCollection.
        /// </summary>
        /// <param name="links"></param>
        /// <param name="stackTrace"></param>
        private void ApplyStackTraceLinks(LinkLabel.LinkCollection links, string stackTrace)
        {
            links.Clear(); // Clear all links first.
            if (stackTrace == null)
                return;

            int stackEnd = stackTrace.Length;
            int lineStart;
            int lineEnd = 0;
            while (lineEnd < stackEnd && (lineStart = stackTrace.IndexOf("at ", lineEnd, StringComparison.Ordinal)) >= 0)
            {
                // We found the start of a frame of the stackTrace.  Find its end....
                lineEnd = stackTrace.IndexOfAny(LineBreakChars, lineStart);
                if (lineEnd < 0)
                    lineEnd = stackEnd; // We hit the end of the stackTrace, so this is the last line (if at all).

                // We found a frame of the stackTrace, from lineStart to (almost) lineEnd.  Try to parse it further.
                int fileStart = stackTrace.IndexOf(") in ", lineStart, StringComparison.Ordinal);
                if (fileStart < 0 || fileStart >= lineEnd) // Match not found before the end of the line for this frame?
                {
                    int offsetStart = stackTrace.IndexOf(") [", lineStart, StringComparison.Ordinal); // Mono has [offset] between method and fileName.
                    if (offsetStart < 0 || offsetStart >= lineEnd) // Not found before the end of the line for this frame.
                        continue; // Didn't find a match with the [offset] (Mono format), either.  Move on to next frame line.

                    fileStart = stackTrace.IndexOf("] in ", offsetStart, StringComparison.Ordinal);
                    if (fileStart < 0 || fileStart >= lineEnd) // Not found before the end of the line for this frame.
                        continue; // No source file to link here, move on to next frame line.
                }

                fileStart += 5; // Skip past the ") in " match for .NET CLR or "] in " match for Mono.
                // LastIndexOf() start index is the index of the last included character (our lineEnd is just beyond that, so - 1).
                // ...and count is the total character count, which is just the delta from included fileStart to excluded lineEnd.
                int fileEnd = stackTrace.LastIndexOf(":", lineEnd - 1, lineEnd - fileStart, StringComparison.Ordinal); // Find the last ":" on the line.
                if (fileEnd < 0 || fileEnd >= lineEnd)
                {
                    // Hmmm, didn't find a line number delimiter (":"), so assume the fileName goes to the end of the line.
                    fileEnd = lineEnd;
                }

                if (fileEnd > fileStart)
                {
                    // TODO: Create link for fileStart through fileEnd (or lineEnd?) to open file (and jump to that line, if possible).
                    string fileName = stackTrace.Substring(fileStart, (fileEnd - fileStart));
                    LinkLabel.Link link = new LinkLabel.Link(fileStart, (fileEnd - fileStart), fileName);
                    try
                    {
                        links.Add(link); // Add the link to the collection.
                    }
                    catch (Exception ex)
                    {
                        Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, true, LogCategory, "Unable to add one or more links in a stack trace due to " + ex.GetBaseException().GetType(),
                            "We will stop attempting to add in any more links.  There are presently {0} links in the collection.\r\nOriginal Stack Trace: \r\n {1}", links.Count, stackTrace);

                        try
                        {
                            links.Remove(link);
                        }
                        catch (Exception innerEx)
                        {
                            Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, innerEx, true, LogCategory, "Unable to clean up bad link in a stack trace due to " + innerEx.GetBaseException().GetType(),
                                "We were trying to remove the last link we added.  There are presently {0} links in the collection.\r\nOriginal Stack Trace: \r\n {1}", links.Count, stackTrace);
                        }
                        break;
                    }

                    if (links.Count == 31)
                        break; //the link label breaks if you try to add a 33rd link, possibly the 32nd.  You get an Overflow exception.
                }
            }
            FormTools.ApplyOSFont(lblStackTrace);
        }

        /// <summary>
        /// Ask the OS to open the specified file (by full fileNamePath).
        /// </summary>
        /// <param name="fileNamePath">The full path to the file to open.</param>
        private static void OpenFileViaShellExec(string fileNamePath)
        {
            //try to open the file
            try
            {
                if (File.Exists(fileNamePath))
                {
                    System.Diagnostics.Process.Start(fileNamePath);
                }
            }
            catch(Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, LogCategory, "Unable to open source code file",
                          "File: '{0}'\r\nException ({1}): {2}", fileNamePath, ex.GetType().FullName, ex.Message);
            }
        }

        /// <summary>
        /// Handle the LinkClicked event for the stack trace LinkLabel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblStackTrace_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string fileName = e.Link.LinkData as string; // Get the specific fileName for the link they clicked on.
            if (string.IsNullOrEmpty(fileName) == false)
                OpenFileViaShellExec(fileName);
        }

        /// <summary>
        /// Find the index of the start of the next occurrence of a match string within a specified range of a string to search.
        /// </summary>
        /// <param name="match">The match string to look for.</param>
        /// <param name="toSearch">The string to search within.</param>
        /// <param name="start">The index at which to start searching (eg. 0 for start of the string).</param>
        /// <param name="end">The index at which to end searching (eg. Length (or -1) to search to the end of the string).</param>
        /// <returns>The index of the start of the next match found, or -1 if it does not occur within the specified range.</returns>
        private static int NextIndexWithin(string match, string toSearch, int start, int end)
        {
            // TODO: Remove this method once confirmed that the built-in stuff works for this.
            if (end < 0 || end > toSearch.Length)
                end = toSearch.Length;

            int matchLength = match.Length;
            int lastPossibleStart = end - matchLength;
            if (start < 0 || start > lastPossibleStart)
                return -1;

            if (matchLength <= 0)
                return start;

            // Now we know match.Length is at least 1, so this is safe.
            char firstChar = match[0];
            int index;
            for (index = start; index <= lastPossibleStart; index++)
            {
                if (toSearch[index] != firstChar) // Rapid advance until we find the first character at least.
                    continue;

                int scan;
                for (scan = 1; scan < matchLength; scan++)
                {
                    if (match[scan] != toSearch[index + scan])
                        break; // It failed to match a character, so we can't match at this index.
                }

                // Now we need to break the outer search if we found a complete match.
                if (scan >= matchLength)
                    break;
            }

            // If index passed lastPossibleStart then we didn't find it.
            if (index > lastPossibleStart)
                index = -1;

            return index;
        }

        private void OnCopyAll()
        {
            var tempEvent = CopyAll;
            if (tempEvent != null)
                tempEvent.Invoke(this, EventArgs.Empty);
        }

        private void PerformResize()
        {
#if DEBUG_RESIZE
            string exceptionName = (m_DisplayException == null) ? "null" : m_DisplayException.TypeName;
            int index = exceptionName.LastIndexOf('.') + 1;
            if (index > 0 && index < exceptionName.Length)
                exceptionName = exceptionName.Substring(index);
#endif

            int newWidth = ClientSize.Width;
            //newWidth = Math.Max(newWidth, 500); // Don't let it go below a sane size.
            if (newWidth != m_CurrentWidth)
            {
                m_CurrentWidth = newWidth; // Set it early to avoid recursive event cycles.

                panelBottom.Width = newWidth; // TODO: Is this right?

                int typeMessageWidth = newWidth - m_TypeMessageMarginWidth;
                Size typeMessageSize = new Size(typeMessageWidth, 0);
                txtTypeMessage.MaximumSize = typeMessageSize;
                txtTypeMessage.MinimumSize = typeMessageSize;
                
                int stackTraceWidth = newWidth - m_StackTraceMarginWidth;
                Size stackTraceMaxSize = new Size(stackTraceWidth, 0);
                
                try //we have sometimes gotten exceptions here when there are lotsa links on the stack trace
                {
                    lblStackTrace.MaximumSize = stackTraceMaxSize;
                    lblStackTrace.MinimumSize = stackTraceMaxSize;
                }
                catch (OverflowException ex)
                {
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, true, LogCategory, "Unable to adjust stack trace min/max size due to " + ex.GetBaseException().GetType(),
                        "Original Stack Trace: \r\n {0}", lblStackTrace.Text);                    
                }

                //now we need to calculate the height for the text bo to know how much room we have left.
                SizeF size = TextRenderer.MeasureText(txtTypeMessage.Text, txtTypeMessage.Font, txtTypeMessage.ClientRectangle.Size, TextFormatFlags.WordBreak);
                txtTypeMessage.Height = Convert.ToInt32(size.Height);

                int typeMessageHeightNeeded = txtTypeMessage.Height;
                int stackTraceHeightNeeded = lblStackTrace.Height;
#if DEBUG_RESIZE
                System.Diagnostics.Debug.Print("UIException ({0}) resize:  newWidth={1}  typeLength={2}  typeHeightNeeded={3}  "+
                                               "stackLength={4}  stackHeightNeeded={5}", exceptionName, newWidth,
                                               txtTypeMessage.Text.Length, typeMessageHeightNeeded,
                                               lblStackTrace.Text.Length, stackTraceHeightNeeded);
#endif

                // Now that we know how tall it needs to be we need to dynamically adjust our MinimumSize to declare it.

                Size typeMessageMinSize = txtTypeMessage.MinimumSize;
                if (typeMessageMinSize.Height != typeMessageHeightNeeded)
                {
                    typeMessageMinSize = new Size(typeMessageMinSize.Width, typeMessageHeightNeeded);
                    txtTypeMessage.MinimumSize = typeMessageMinSize;
                    txtTypeMessage.Height = typeMessageHeightNeeded; // Set the actual height to match min height.
                }

                int stackTraceLocationX = Padding.Left + lblStackTrace.Margin.Left + m_RectangleWidth;
                int stackTraceLocationY = txtTypeMessage.Location.Y + txtTypeMessage.Height; // Bottom of txtTypeMessage...
                stackTraceLocationY += txtTypeMessage.Margin.Bottom + lblStackTrace.Margin.Top; // ...and allow for both margins.
                Point stackTraceLocation = new Point(stackTraceLocationX, stackTraceLocationY);
                lblStackTrace.Location = stackTraceLocation;

                int uiExceptionHeight = stackTraceLocationY + lblStackTrace.Height;
                uiExceptionHeight += lblStackTrace.Margin.Bottom + Padding.Bottom;
                if (Height != uiExceptionHeight)
                    Height = uiExceptionHeight; // Reset our height to the size needed.
            }
#if DEBUG_RESIZE
            else
            {
                System.Diagnostics.Debug.Print("UIException ({0}) resize:  newWidth={1}  typeLength={2}  No Change",
                                               exceptionName, newWidth, txtTypeMessage.Text.Length);
            }
#endif
        }

        private void UIException_Resize(object sender, EventArgs e)
        {
            PerformResize();
        }

        private void TextChangedHandler(object sender, EventArgs e)
        {
            m_CurrentWidth = 0; // Make sure we recalculate the needed height for the new text.
            //PerformResize(); // And force a resize calculation.
        }

        private void UIException_MouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.GhostWhite;
        }

        private void UIException_MouseLeave(object sender, EventArgs e)
        {
            BackColor = Color.Transparent;
            lblStackTrace.BackColor = Color.Transparent;

            Point relativeLocation;
            CheckIfInControl(out relativeLocation);
        }

        private void UIException_MouseMove(object sender, MouseEventArgs e)
        {
            //see what we're over so we know where to assign our button
            Point relativeLocation;
            if (CheckIfInControl(out relativeLocation))
            {
                return;
            }

            if (relativeLocation.Y < txtTypeMessage.Top + txtTypeMessage.Height)
            {
                AssignCopyButton(txtTypeMessage, ActionCopyMessage, true);
            }
            else
            {
                AssignCopyButton(lblStackTrace, ActionCopyStackTrace, false);
            }
        }

        private void StackTrace_MouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.GhostWhite;
            AssignCopyButton(lblStackTrace, ActionCopyStackTrace, false);
        }

        private void StackTrace_MouseLeave(object sender, EventArgs e)
        {
            BackColor = Color.Transparent;
            lblStackTrace.BackColor = Color.Transparent;
            Point relativeLocation;
            CheckIfInControl(out relativeLocation);
        }

        private void txtTypeMessage_MouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.GhostWhite;
            AssignCopyButton(txtTypeMessage, ActionCopyMessage, true);
        }

        private void txtTypeMessage_MouseLeave(object sender, EventArgs e)
        {
            BackColor = Color.Transparent;
            Point relativeLocation;
            CheckIfInControl(out relativeLocation);
        }

        private void UIException_BackColorChanged(object sender, EventArgs e)
        {
            Color newColor = BackColor;
            if (newColor == Color.Transparent)
                newColor = SystemColors.Window; // TextBox can't be Transparent, so set it back to our default.

            txtTypeMessage.BackColor = newColor;
        }

        private void lblStackTrace_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // TODO: Switch to a TextBox and select/copy the StackTrace?
            // For now, just copy the stack trace automatically when they double-click on it.
            string stackTrace = lblStackTrace.Text;
            if (string.IsNullOrEmpty(stackTrace))
                stackTrace = string.Empty;
            else
                stackTrace += "\r\n"; // Add in a line terminator which it doesn't have (and we'd have trimmed out if it did).

            Clipboard.SetText(stackTrace);
            lblStackTrace.BackColor = SystemColors.Highlight;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            ActionCopy();
        }

        private void btnCopyAll_Click(object sender, EventArgs e)
        {
            OnCopyAll();
        }

        private void btnCopy_MouseEnter(object sender, EventArgs e)
        {
            var button = sender as Control;
            if (button == null)
                return;

            button.Cursor = Cursors.Arrow;
        }
    }
}
