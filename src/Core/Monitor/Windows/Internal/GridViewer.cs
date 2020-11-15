
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevAge.ComponentModel;
using DevAge.Drawing;
using Gibraltar.Monitor.Internal;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;
using SourceGrid;
using SourceGrid.Cells;
using SourceGrid.Cells.Controllers;
using SourceGrid.Cells.Models;
using SourceGrid.Cells.Views;
using DataGrid=SourceGrid.DataGrid;
using Image=System.Drawing.Image;
using SortableHeader=SourceGrid.Cells.Models.SortableHeader;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    internal class GridViewer : DataGrid, IImage
    {
        private const int MinWidthMessageColumn = 300;
        private const int MinWidthMethodColumn = 50;
        private const int MinWidthSourceColumn = 150;
        private const int MinWidthThreadColumn = 50;
        private const int MinWidthTimeColumn = 136;
        private const int MinWidthSeverityColumn = 20;
        private const int MinWidthUserColumn = 100;
        private readonly Image m_ImageCritical;
        private readonly Image m_ImageError;
        private readonly Image m_ImageInfo;
        private readonly Image m_ImageVerbose;
        private readonly Image m_ImageWarning;
        private readonly MyToolTipModel m_ToolTipModel = new MyToolTipModel();

        private readonly DataGridColumn m_SequenceColumn;
        private readonly DataGridColumn m_SeverityColumn;
        private readonly DataGridColumn m_TimeColumn;
        private readonly DataGridColumn m_UserNameColumn;
        private readonly DataGridColumn m_MessageColumn;
        private readonly DataGridColumn m_ThreadNameColumn;
        private readonly DataGridColumn m_MethodColumn;
        private readonly DataGridColumn m_SourceCodeLocationColumn;

        private readonly List<ViewerLogMessageWrapper> m_FilteredLogMessages; //the subset of messages that PASS the filter (those not filtered out)
        private readonly List<ViewerLogMessageWrapper> m_LogMessages;
        private bool m_Frozen;
        private string m_SearchText;
        private bool m_SortAscending;
        private int m_SortColumn;
        private int m_CriticalCount;
        private int m_ErrorCount;
        private int m_WarningCount;
        private int m_InfoCount;
        private int m_VerboseCount;
        private ViewerLogMessageWrapper m_SelectedLogMessage;


        public event EventHandler ViewChanged;
        public event EventHandler UserInteraction;
        public event EventHandler FilterChanged;

        public GridViewer()
        {
            m_LogMessages = new List<ViewerLogMessageWrapper>();
            m_FilteredLogMessages = new List<ViewerLogMessageWrapper>();

            m_ImageCritical = SessionIcons.Critical;
            ((Bitmap)m_ImageCritical).MakeTransparent(Color.Magenta);

            m_ImageError = SessionIcons.Error;
            ((Bitmap)m_ImageError).MakeTransparent(Color.Magenta);

            m_ImageWarning = SessionIcons.Warning;
            ((Bitmap)m_ImageWarning).MakeTransparent(Color.Magenta);
            
            m_ImageInfo = SessionIcons.Information;
            ((Bitmap)m_ImageInfo).MakeTransparent(Color.Magenta);
            
            m_ImageVerbose = SessionIcons.Verbose;
            ((Bitmap)m_ImageVerbose).MakeTransparent(Color.Magenta);

            BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            ((GridViewer)this).BackColor = SystemColors.Window;
            Rows.AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize;

            AutoStretchColumnsToFitWidth = false;
            AutoStretchRowsToFitHeight = false;

            // It turns out the the order of these next two calls is important
            // Changing SelectionMode will reset EnableMultiSelection
            SelectionMode = GridSelectionMode.Row;

            // The Sequence column goes first but is hidden.  It is used as a secondary
            // field for all sorting to ensure that sequence of events is maintained
            m_SequenceColumn = Columns.Add("Sequence", "Sequence", typeof(long));
            m_SequenceColumn.Tag = LogMessageColumn.Sequence;
            m_SequenceColumn.Visible = false;

            // Severity is the first visible column.
            m_SeverityColumn = Columns.Add("Severity", string.Empty, typeof(LogMessageSeverity));
            m_SeverityColumn.Tag = LogMessageColumn.Severity;
            m_SeverityColumn.DataCell.Model.AddModel(new BoundImage());
            m_SeverityColumn.Width = MinWidthSeverityColumn;
            m_SeverityColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;
            m_SeverityColumn.DataCell.View.Border = new RectangleBorder(new BorderLine(SystemColors.Window, 0),
                                                                  new BorderLine(SystemColors.Window, 0));

            // Time is the second first visible column.
            m_TimeColumn = Columns.Add("TimestampDateTime", "Time", typeof(DateTime));
            m_TimeColumn.Tag = LogMessageColumn.Timestamp;
            m_TimeColumn.Width = MinWidthTimeColumn;
            m_TimeColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            TypeConverter dtConverter = new DevAge.ComponentModel.Converter.DateTimeTypeConverter("G");
            m_TimeColumn.DataCell.Editor = new SourceGrid.Cells.Editors.TextBoxUITypeEditor(typeof(DateTime))
                                             {TypeConverter = dtConverter};

            m_UserNameColumn = Columns.Add("UserName", "User", typeof(string));
            m_UserNameColumn.Tag = LogMessageColumn.UserName;
            m_UserNameColumn.Width = MinWidthUserColumn;
            m_UserNameColumn.Visible = true;
            m_UserNameColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            m_MessageColumn = Columns.Add("Message", "Message", typeof(string));
            m_MessageColumn.Tag = LogMessageColumn.Caption;
            m_MessageColumn.Width = MinWidthMessageColumn;
            m_MessageColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            m_ThreadNameColumn = Columns.Add("ThreadName", "Thread", typeof(string)); // ToDo: Needs to be ThreadId ?
            m_ThreadNameColumn.Tag = LogMessageColumn.Thread;
            m_ThreadNameColumn.Width = MinWidthThreadColumn;
            m_ThreadNameColumn.Visible = false; //we turn these on if optional properties are set
            m_ThreadNameColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            m_MethodColumn = Columns.Add("MethodFullName", "Method", typeof(string));
            m_MethodColumn.Tag = LogMessageColumn.Method;
            m_MethodColumn.Width = MinWidthMethodColumn;
            m_MethodColumn.Visible = false; //we turn these on if optional properties are set
            m_MethodColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            m_SourceCodeLocationColumn = Columns.Add("SourceCodeLocation", "Source Code", typeof(string));
            m_SourceCodeLocationColumn.Tag = LogMessageColumn.SourceCodeLocation;
            m_SourceCodeLocationColumn.Width = MinWidthSourceColumn;
            m_SourceCodeLocationColumn.Visible = false; //we turn these on if optional properties are set
            m_SourceCodeLocationColumn.AutoSizeMode = SourceGrid.AutoSizeMode.None;

            // Create a read-only BoundList
            DataSource = new LogMessageBoundList(m_FilteredLogMessages)
                             {
                                 AllowEdit = false,
                                 AllowNew = false,
                                 AllowDelete = false,
                                 AllowSort = true
                             };

            // set up each column referencing the same ToolTip and IToolTipModel
            foreach (DataGridColumn column in Columns)
            {
                SortableHeader model = column.HeaderCell.Model.FindModel(typeof(ISortableHeader)) as SortableHeader;
                //if (model != null)
                //    column.HeaderCell.Model.RemoveModel(model);
                IController controller = column.HeaderCell.Controller.FindController(typeof(SourceGrid.Cells.Controllers.SortableHeader));
                //if (controller != null)
                //    column.HeaderCell.Controller.RemoveController(controller);
                column.HeaderCell.Controller.FindController(typeof(bool));
                column.DataCell.AddController(SourceGrid.Cells.Controllers.ToolTipText.Default);
                column.DataCell.Model.AddModel(m_ToolTipModel);
                column.DataCell.View.TextAlignment = DevAge.Drawing.ContentAlignment.TopLeft;

                ((ViewBase)column.DataCell.View).ImageAlignment = DevAge.Drawing.ContentAlignment.TopLeft;
                column.DataCell.View.WordWrap = true;
            }

            // Initialize tooltip to be responsive and not disappear too fast
            ToolTip.AutoPopDelay = 60000;
            ToolTip.InitialDelay = 200;
            ToolTip.ReshowDelay = 0;
            ToolTip.ShowAlways = false;
            ToolTip.UseAnimation = true;
            ToolTip.UseFading = true;

            MaxMessages = LiveLogViewer.DefaultMaxMessages;
        }

        protected override RowsBase CreateRowsObject()
        {
            return new GridViewerRows(this);
        }

        public bool ShowDetailsInTooltips { get { return m_ToolTipModel.ShowDetails; } set { m_ToolTipModel.ShowDetails = value; } }

        public bool Sorted { get { return m_SortColumn != 0; } }

        public bool EnableMultiSelection { get { return Selection.EnableMultiSelection; } set { Selection.EnableMultiSelection = value; } }

        public bool Frozen { get { return m_Frozen; } set { m_Frozen = value; } }

        public bool AutoScrollMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum number of messages
        /// to retain in the list.  A negative or zero value means that the
        /// list can grow without bound.
        /// </summary>
        public int MaxMessages { get; set; }

        public bool ShowCriticalMessages { get; set; }

        public bool ShowErrorMessages { get; set; }

        public bool ShowWarningMessages { get; set; }

        public bool ShowInformationMessages { get; set; }

        public bool ShowVerboseMessages { get; set; }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                //is it really different than what we already have?  no reason to do 
                //gratuitous work.
                if (string.IsNullOrEmpty(value))
                    value = string.Empty;

                if (m_SearchText == value.ToLower())
                    return;

                m_SearchText = value.ToLower();
                FilterMessages();
            }
        }

        public int MessageCount { get { return m_LogMessages.Count; } }

        public int CriticalMessageCount { get { return m_CriticalCount; } }

        public int ErrorMessageCount { get { return m_ErrorCount; } }

        public int WarningMessageCount { get { return m_WarningCount; } }

        public int InfoMessageCount { get { return m_InfoCount; } }

        public int VerboseMessageCount { get { return m_VerboseCount; } }

        public long MinMessageId { get { return m_LogMessages.Count > 0 ? m_LogMessages[0].Sequence : 0; } }

        public int VisibleCount { get { return m_FilteredLogMessages.Count; } }

        public void ClearSelection()
        {
            m_SelectedLogMessage = null;
            Selection.ResetSelection(false);
        }

        public override void ChangeMouseDownCell(Position p_MouseDownCell, Position p_MouseCell)
        {
            if(p_MouseDownCell.Row > 0)
                m_SelectedLogMessage = m_FilteredLogMessages[p_MouseDownCell.Row - 1];

            base.ChangeMouseDownCell(p_MouseDownCell, p_MouseCell);
        }

        public void FilterMessages()
        {
            DataSource.BeginAddNew();
            m_FilteredLogMessages.Clear();

            foreach (var message in m_LogMessages)
            {
                if (MatchesSeverityFilter(message.Severity) && MatchesSearchText(message.Message))
                    m_FilteredLogMessages.Add(message);
            }

            Prune();

            if (Sorted)
                SortByColumn(m_SortColumn, m_SortAscending);

            DataSource.EndEdit(false);

            // Force a resize to get column widths right (and correct row heights for any wrapping/multi-line messages?).
            PerformResize();
            // We may need to do the resize before showing selected row so that it can calculate scroll position correctly?

            // Update the grid to display the most recently selected row.
            ShowSelectedRow();

            // And we might have changed our message counts.
            OnViewChanged();

            // If we're auto-scrolling, jump to the end.
            if (AutoScrollMessages)
                MoveLast();

            OnFilterChanged();
        }

        /// <summary>
        /// Ensure that the selected row is displayed near the middle of the window.
        /// </summary>
        private void ShowSelectedRow()
        {
            // We need to know how many rows are currently showing to help us center the selected row
            RecalcCustomScrollBars();

            // Our intention is to display the selected row near the center of the window.
            // To accomplish this we will first ensure that the selected row is showing.
            // Then, we will ensure that a row previous row is showing.  The distance to
            // that previous row will be half the number of currently displayed rows.
            int centerRow; // this is the main row we want to see

            // Find the position of the selected row in the list of filtered messages.
            // If it is found, BinarySearch will return the index.  If not, it will return
            // the bitwise complement of the index of the next higher entry.
            int search = m_FilteredLogMessages.BinarySearch(m_SelectedLogMessage, new SequenceComparer());
            bool exactMatch = (search >= 0);

            if (search < 0)
            {
                search = ~search;
                if (search >= m_FilteredLogMessages.Count)
                {
                    // In this case, the selected row is filtered and is past the end of the last displayed row.
                    // In this scenario, we want to just show the last row.
                    centerRow = m_FilteredLogMessages.Count - 1;
                }
                else
                {
                    // In this case, the selected row is filtered, but we have displayed messages nearby.
                    // Center the display around the next higher log message.
                    centerRow = search;
                }
            }
            else
            {
                // In this case, the selected row is displayable.
                // Center the display around the selected message.
                centerRow = search;
            }

            // we always need to increment the center row because the array index is
            // zero-based whereas row numbers are one-based (because row zero is the header)
            centerRow += 1; 

            // Typically, the centerRow will already be showing and this call will do nothing.
            // However, since row height is dynamic, in some situations, the grid will need
            // to scroll a bit to ensure that selected row is displayed.
            ShowCell(new Position(centerRow, 0), true);

            // Now that we have scrolled to the desired row, we want to back up a bit to roughly center it.
            int rowCount = GetVisibleRows(true).Count;
            int edgeRow = centerRow - (rowCount / 2);
            // If the selected row is < half a page from the top, edgeRow might be negative.
            // Guard against that.  We should show the first row in that case. 
            if (edgeRow <= 0)
                edgeRow = 1;

            // Now scroll back up a bit to the identified edge row to try to center the desired centerRow.
            ShowCell(new Position(edgeRow, 0), true);
            ShowCell(new Position(centerRow, 0), true); // And make sure it didn't pathologically scroll too far.

            // If the selected row is visible, select it.  Otherwise, don't show a selected row.
            if (exactMatch)
                Selection.FocusRow(centerRow);
            else
                Selection.ResetSelection(false);
        }

        public void Add(ILogMessage newMessage)
        {
            AddRange(new[] {newMessage});
        }

        public void AddRange(ILogMessage[] newMessages)
        {
            DataSource.BeginAddNew();

            foreach (ILogMessage innerMessage in newMessages)
            {
                var message = new ViewerLogMessageWrapper(innerMessage);
                switch (message.Severity)
                {
                    case LogMessageSeverity.Critical:
                        m_CriticalCount++;
                        break;
                    case LogMessageSeverity.Error:
                        m_ErrorCount++;
                        break;
                    case LogMessageSeverity.Warning:
                        m_WarningCount++;
                        break;
                    case LogMessageSeverity.Information:
                        m_InfoCount++;
                        break;
                    default:
                        m_VerboseCount++;
                        break;
                }

                m_LogMessages.Add(message);

                if (MatchesSeverityFilter(message.Severity) && MatchesSearchText(message.Message))
                    m_FilteredLogMessages.Add(message);
            }

            Prune();
            DataSource.EndEdit(false);

            //if we're in auto-refresh mode, jump to end and auto resize
            if (AutoScrollMessages)
            {
                // In addition to other things(?), this resize appears to compute the row heights for the new rows.
                Columns.StretchToFit();
                // We need to do the MoveLast() after the resize, so the row heights are correct and it scrolls correctly.
                MoveLast();
            }

            //and we've definitely changed our message counts
            OnViewChanged();
        }

        public void PerformResize()
        {
            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Visible == false)
                return; // Quick bail if we aren't actually visible.  We may have no dimensions to compute with.

            base.OnResize(e);

            SuspendLayout();
            SetScrollArea();

            const int bufferWidth = 24;
            int minWidth = MinWidthSeverityColumn + MinWidthTimeColumn + MinWidthMessageColumn + bufferWidth;
            //see if we are displaying detail columns
            DataGridColumn exemplarColumn = GetColumnForEnum(LogMessageColumn.Thread);
            bool showDetails = (exemplarColumn != null) && exemplarColumn.Visible;

            if (showDetails)
                minWidth += MinWidthThreadColumn + MinWidthMethodColumn;

            int availableWidth = ClientSize.Width;
            int extraWidth = Math.Max(availableWidth - minWidth, 0);
            m_SeverityColumn.Width = MinWidthSeverityColumn;
            m_TimeColumn.Width = MinWidthTimeColumn;
            if (showDetails)
            {
                m_MessageColumn.Width = MinWidthMessageColumn + (int)(extraWidth * 0.75);
                m_ThreadNameColumn.Width = MinWidthThreadColumn + (int)(extraWidth * 0.05);
                m_MethodColumn.Width = MinWidthMethodColumn + (int)(extraWidth * 0.20);
            }
            else
            {
                m_MessageColumn.Width = MinWidthMessageColumn + extraWidth;
            }

            Invalidate();
            ResumeLayout(true);
        }

        public void ColumnVisible(LogMessageColumn column, bool visible)
        {
            //see if we even display that column, we might not
            DataGridColumn targetColumn = GetColumnForEnum(column);

            if ((targetColumn != null) && (targetColumn.Visible != visible))
            {
                //The visible property is largely worthless on set; use width because that's all visible does.
                int newWidth;

                //if we are hiding, we might need to save off the width
                if (visible)
                {
                    //now figure out what width to make it as we show it
                    newWidth = DefaultWidth;

                    //override the default width for some of our favorite columns
                    switch (column)
                    {
                        case LogMessageColumn.Method:
                            newWidth = MinWidthMethodColumn;
                            break;
                        case LogMessageColumn.SourceCodeLocation:
                            newWidth = MinWidthSourceColumn;
                            break;
                        case LogMessageColumn.Thread:
                            newWidth = MinWidthThreadColumn;
                            break;
                    }
                }
                else
                    newWidth = 0;

                targetColumn.Visible = visible;
                targetColumn.Width = newWidth;
            }
        }

        public void CopyAll()
        {
            Range range = new Range(1, 1, Rows.Count - 1, Columns.Count - 1);
            RangeRegion region = new RangeRegion(range);
            CopyToClipboard(region);
        }

        public void CopySelection()
        {
            RangeRegion selRegion = Selection.GetSelectionRegion();
            CopyToClipboard(selRegion);
        }

        public void Clear()
        {
            DataSource.BeginAddNew();
            m_LogMessages.Clear();
            m_CriticalCount = 0;
            m_ErrorCount = 0;
            m_WarningCount = 0;
            m_InfoCount = 0;
            m_VerboseCount = 0;
            m_FilteredLogMessages.Clear();
            DataSource.EndEdit(false);
        }

        public ILogMessage GetCurrentMessage()
        {
            ILogMessage message = (m_FilteredLogMessages.Count > 0) ? m_FilteredLogMessages[m_FilteredLogMessages.Count - 1] : null;

            return message;
        }

        public ILogMessage GetSelectedMessage()
        {
            Position activePosition = Selection.ActivePosition;
            int rowIndex = activePosition.Row;

            ILogMessage message = (rowIndex <= 0 || rowIndex > m_FilteredLogMessages.Count) ? null : m_FilteredLogMessages[rowIndex - 1];

            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveLast()
        {
            ShowCell(new Position(Rows.Count, 1), false);
            Selection.ResetSelection(false);
            //Selection.SelectRow(Rows.Count - 1, true);
        }

        public void MoveFirst()
        {
            ShowCell(new Position(1, 1), false);
            Selection.ResetSelection(false);
            //Selection.SelectRow(1, true);
        }

        public void Save(string fileNamePath)
        {
            ActionSave(fileNamePath);
        }

        public void SelectAll()
        {
            for (int row = 1; row < Rows.Count; row++)
                Selection.SelectRow(row, true);
        }

        internal DataGridColumn GetColumnForEnum(LogMessageColumn column)
        {
            foreach (ColumnInfo columnInfo in Columns)
            {
                if ((LogMessageColumn)columnInfo.Tag == column)
                {
                    //this is our friend
                    return (DataGridColumn)columnInfo;
                }
            }

            //It's OK to return nothing
            return null;
        }

        private bool MatchesSearchText(string messageText)
        {
            if (string.IsNullOrEmpty(m_SearchText))
                return true;

            return messageText.ToLower().Contains(m_SearchText);
        }

        private bool MatchesSeverityFilter(LogMessageSeverity severity)
        {
            switch (severity)
            {
                case LogMessageSeverity.Critical:
                    return ShowCriticalMessages;
                case LogMessageSeverity.Error:
                    return ShowErrorMessages;
                case LogMessageSeverity.Warning:
                    return ShowWarningMessages;
                case LogMessageSeverity.Information:
                    return ShowInformationMessages;
                default:
                    return ShowVerboseMessages;
            }
        }

        protected override void OnUserInteraction()
        {
            DisableAutoScrollMessages();
        }

        private void DisableAutoScrollMessages()
        {
            if (AutoScrollMessages)
            {
                AutoScrollMessages = false;
                EventHandler userInteractionHandler = UserInteraction;
                if (userInteractionHandler != null)
                    userInteractionHandler(this, EventArgs.Empty);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)1: // ctrl A
                    if (EnableMultiSelection)
                        SelectAll();
                    break;

                case (char)3: // ctrl C
                    CopySelection();
                    break;
            }
        }

        private void ActionSave(string fileNamePath)
        {
            //delete the file if it exists
            if (File.Exists(fileNamePath))
                File.Delete(fileNamePath);

            Byte[] newFileBytes;

            //create a new file which will be automatically closed when e exit the with block
            using (FileStream stream = File.Create(fileNamePath))
            {
                foreach (ILogMessage curLogMessage in m_LogMessages)
                {
                    DateTimeOffset timeStamp = curLogMessage.Timestamp;
                    string newTextLine = String.Format("{0:0000}-{1:00}-{2:00} {3:00}-{4:00}-{5:00}: {6} - {7}\r\n",
                                                       timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, timeStamp.Minute, timeStamp.Second,
                                                       curLogMessage.Severity, curLogMessage.Caption);
                    newFileBytes = new UTF8Encoding(true).GetBytes(newTextLine);
                    stream.Write(newFileBytes, 0, newFileBytes.Length);
                }
            }
        }

        private void CopyToClipboard(RangeRegion region)
        {
            StringBuilder builder = new StringBuilder();
            int arrayRow = 0;

            for (int i = 0; i < region.Count; i++)
            {
                Range range = region[i];
                for (int row = range.Start.Row; row <= range.End.Row; row++, arrayRow++)
                {
                    int arrayCol = 0;
                    for (int col = 1; col <= range.End.Column; col++, arrayCol++)
                    {
                        ICellVirtual cell = GetCell(row, col);
                        Position position = new Position(row, col);
                        CellContext context = new CellContext(this, position);
                        if (cell.Model.ValueModel.GetValue(context) != null)
                        {
                            string text = cell.Model.ValueModel.GetValue(context).ToString();
                            builder.Append(text);
                        }
                        if (col != range.End.Column)
                            builder.Append('\t');
                    }
                    builder.Append("\x0D\x0A");
                }
            }

            //builder.Append(LogMessage.WelcomeMessage);

            if (builder.Length > 0)
               Clipboard.SetText(builder.ToString());
        }

        /// <summary>
        /// This method is overridden to ensure that whatever column the user chooses to sort on,
        /// we use the ID column as a secondary key to retain sequence of events as much as possible.
        /// </summary>
        protected override void OnSortingRangeRows(SortRangeRowsEventArgs e)
        {
            if (SortByColumn(e.KeyColumn, e.Ascending))
            {
                m_Frozen = e.KeyColumn != 0;
                m_SortColumn = e.KeyColumn;
                m_SortAscending = e.Ascending;
                UpdateGridBackColor();
            }
        }

        private bool SortByColumn(int keyColumn, bool ascending)
        {
            if (DataSource == null || DataSource.AllowSort == false)
                return false;

            PropertyDescriptor idCol = Columns[0].PropertyColumn;
            PropertyDescriptor propertyCol = Columns[keyColumn].PropertyColumn;

            if (propertyCol != null)
            {
                ListSortDirection direction = ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                ListSortDescription[] sortsArray = new ListSortDescription[2];
                sortsArray[0] = new ListSortDescription(propertyCol, direction);
                sortsArray[1] = new ListSortDescription(idCol, direction);

                DataSource.ApplySort(new ListSortDescriptionCollection(sortsArray));
                MoveFirst();
                return true;
            }
            return false;
        }

        public void RemoveSort()
        {
            SourceGrid.Cells.Controllers.SortableHeader controller = Columns[0].HeaderCell.Controller.FindController(typeof(SourceGrid.Cells.Controllers.SortableHeader)) as SourceGrid.Cells.Controllers.SortableHeader;
            if (controller != null)
                controller.SortColumn(new CellContext(this, new Position(0, 0)), true, null);

            m_SortColumn = 0;
            Frozen = false;
            UpdateGridBackColor();
        }

        internal void UpdateGridBackColor()
        {
            // TODO: Fix this to set the BackColor of the grid content itself. (How?)
            return;
            //Color defaultColor = DefaultBackColor;
            //if (string.IsNullOrEmpty(m_SearchText) && m_SortColumn == 0)
            //{
            //    BackColor = SystemColors.Window;
            //}
            //else
            //{
            //    BackColor = Color.Cornsilk;
            //}
        }

        private void Prune()
        {
            if (MaxMessages <= 0)
            {
                //we don't want to do anything - pruning is disabled
                return;
            }

            int removeCount = m_LogMessages.Count - MaxMessages;
            if (removeCount > 0)
            {
                for (int index = 0; index < removeCount; index++)
                {
                    switch (m_LogMessages[index].Severity)
                    {
                        case LogMessageSeverity.Critical:
                            m_CriticalCount--;
                            break;
                        case LogMessageSeverity.Error:
                            m_ErrorCount--;
                            break;
                        case LogMessageSeverity.Warning:
                            m_WarningCount--;
                            break;
                        case LogMessageSeverity.Information:
                            m_InfoCount--;
                            break;
                        default:
                            m_VerboseCount--;
                            break;
                    }
                }

                m_LogMessages.RemoveRange(0, removeCount);

                if (m_LogMessages.Count == 0)
                    m_FilteredLogMessages.Clear();
                else
                {
                    //remove from the filtered list all messages with an older sequence number than
                    //the earliest message we're keeping in the full list.  We have no idea how many
                    //that will be, so figure that out first
                    removeCount = 0;
                    long minSequence = m_LogMessages[0].Sequence;

                    for (int i = 0; i < m_FilteredLogMessages.Count; i++)
                    {
                        if (m_FilteredLogMessages[i].Sequence < minSequence)
                            removeCount++;
                        else
                            break;
                    }

                    //remove all of the old messages in one blow.
                    if (removeCount > 0)
                        m_FilteredLogMessages.RemoveRange(0, removeCount);
                }
            }
        }

        private void OnViewChanged()
        {
            EventHandler viewEvent = ViewChanged;
            if (viewEvent != null)
                viewEvent(this, EventArgs.Empty);
        }

        private void OnFilterChanged()
        {
            EventHandler filterEvent = FilterChanged;
            if (filterEvent != null)
                filterEvent(this, EventArgs.Empty);
        }

        #region IImage Members

        public Image GetImage(CellContext cellContext)
        {
            object cellValue = cellContext.Value;
            if (cellValue == null || cellValue.GetType() != typeof(LogMessageSeverity))
                return m_ImageVerbose; // Ack!  just return something valid.

            LogMessageSeverity severity = (LogMessageSeverity) cellValue;

            switch (severity)
            {
                case LogMessageSeverity.Critical:
                    return m_ImageCritical;

                case LogMessageSeverity.Error:
                    return m_ImageError;

                case LogMessageSeverity.Warning:
                    return m_ImageWarning;

                case LogMessageSeverity.Information:
                    return m_ImageInfo;

                default: // Verbose and anything unrecognized, just show as Verbose.
                    return m_ImageVerbose;
            }
        }

        #endregion

        #region Nested type: BoundImage

        public class BoundImage : IImage
        {
            private static readonly List<BoundImage> m_AllBoundImages = new List<BoundImage>();
            private int m_MyThreadId;

            public BoundImage()
            {
                m_MyThreadId = Thread.CurrentThread.ManagedThreadId;
                m_AllBoundImages.Add(this);
            }

            #region IImage Members
            public Image GetImage(CellContext cellContext)
            {
                object cellValue = cellContext.Value;
                GridViewer grid = (GridViewer) cellContext.Grid;

                if (cellValue == null || cellValue.GetType() != typeof(LogMessageSeverity))
                    return grid.m_ImageVerbose; // Ack!  just return something valid.

                LogMessageSeverity severity = (LogMessageSeverity) cellValue;

                switch (severity)
                {
                    case LogMessageSeverity.Critical:
                        return grid.m_ImageCritical;

                    case LogMessageSeverity.Error:
                        return grid.m_ImageError;

                    case LogMessageSeverity.Warning:
                        return grid.m_ImageWarning;

                    case LogMessageSeverity.Information:
                        return grid.m_ImageInfo;

                    default: // Verbose and anything unrecognized, just show as Verbose.
                        return grid.m_ImageVerbose;
                }
            }
            #endregion
        }

        #endregion

        #region Nested type: LogMessageBoundList

        private class LogMessageBoundList : BoundList<ViewerLogMessageWrapper>
        {
            public LogMessageBoundList(IList<ViewerLogMessageWrapper> list)
                : base(list)
            {
            }

            protected override ViewerLogMessageWrapper OnAddNew()
            {
                return null;
            }
        }

        #endregion

        #region Nested type: MyToolTipModel

        /// <summary>
        /// This helper class extracts all relevant info from the row containing the cell under the cursor.
        /// </summary>
        private class MyToolTipModel : IToolTipText
        {
            public bool ShowDetails { get; set; }

            public MyToolTipModel()
            {
            }

            #region IToolTipText Members

            /// <summary>
            /// This is the method that returns the tooltip text.  In our implementation, we also
            /// tweak the icon and title of the tooltip itself.
            /// </summary>
            public string GetToolTipText(CellContext cellContext)
            {
                // First, let's get references to each cell of the current row
                GridViewer grid = (GridViewer)cellContext.Grid;

                // No point in showing tooltips when the display is not paused or frozen
                // For now, disable tooltips unless they config to enable tooltip details.
                if (ShowDetails == false || (grid.AutoScrollMessages && !grid.Frozen))
                    return null;

                CellContext seqnumCol = new CellContext(grid, new Position(cellContext.Position.Row, 0));
                CellContext typeCol = new CellContext(grid, new Position(cellContext.Position.Row, 1));
                CellContext timeCol = new CellContext(grid, new Position(cellContext.Position.Row, 2));
                CellContext messageCol = new CellContext(grid, new Position(cellContext.Position.Row, 4));
                CellContext threadCol = new CellContext(grid, new Position(cellContext.Position.Row, 5));
                CellContext methodCol = new CellContext(grid, new Position(cellContext.Position.Row, 6));
                CellContext sourceCol = new CellContext(grid, new Position(cellContext.Position.Row, 7));

                // We also need a reference to the tooltip.  In the GridViewer constructor we initialized
                // every column to reference the same MyToolTipModel instance.  This makes it easy now for
                // us to assign values to the ToolTipIcon and ToolTipTitle properties.
                ToolTipText tooltip = SourceGrid.Cells.Controllers.ToolTipText.Default;
                tooltip.ToolTipIcon = ToolTipIcon.None;

                // If we don't also assign the tooltip here, we don't see the change right away
                grid.ToolTip.ToolTipIcon = ToolTipIcon.None;

                // Include key fields in the title followed by the other field values in the actual tooltip text
                long msgIndex = cellContext.Position.Row;
                string title = string.Format("{0} message ({1} of {2}) at {3}",
                                          typeCol.DisplayText, msgIndex, grid.MessageCount, timeCol.DisplayText);
                tooltip.ToolTipTitle = title;

                string message = (ShowDetails == false) ? messageCol.DisplayText :
                                    string.Format("{0}\n\nThread:\t{1}\nMethod:\t{2}\nSource:\t{3}",
                                                  messageCol.DisplayText,
                                                  threadCol.DisplayText,
                                                  methodCol.DisplayText,
                                                  sourceCol.DisplayText);

                return message;
            }

            #endregion
        }

        #endregion

        #region Nested Type SequenceComparer

        /// <summary>
        /// A helper class to compare log message packets by sequence number.
        /// </summary>
        /// <remarks>This may be going away.</remarks>
        private class SequenceComparer : IComparer<ViewerLogMessageWrapper>
        {
            public int Compare(ViewerLogMessageWrapper x, ViewerLogMessageWrapper y)
            {
                if (x == null)
                {
                    // Null is equal to null, but less than everything else.
                    return (y == null) ? 0 : -1;
                }
                else if (y == null)
                {
                    // Everything else is greater than null.
                    return 1;
                }

                // Normally, we compare by sequence number.  Note: We assume this won't overflow!
                return (int)(x.Sequence - y.Sequence);
            }
        }

        #endregion

        #region Nested Type ViewerLogMessageWrapper

        /// <summary>
        /// Extends the ILogMessage interface with some flattened properties for the log viewer.
        /// </summary>
        private class ViewerLogMessageWrapper : ILogMessage
        {
            private readonly ILogMessage m_LogMessage;
            private string m_Message;

            public ViewerLogMessageWrapper(ILogMessage logMessage)
            {
                m_LogMessage = logMessage;
            }


            public int CompareTo(ILogMessage other)
            {
                return m_LogMessage.CompareTo(other);
            }

            public bool Equals(ILogMessage other)
            {
                return m_LogMessage.Equals(other);
            }

            public ISession Session { get { return m_LogMessage.Session; } }

            public Guid Id { get { return m_LogMessage.Id; } }

            public long Sequence { get { return m_LogMessage.Sequence; } }

            public DateTimeOffset Timestamp { get { return m_LogMessage.Timestamp; } }

            public DateTimeOffset DisplayTimestamp { get { return m_LogMessage.DisplayTimestamp; } }

            public LogMessageSeverity Severity { get { return m_LogMessage.Severity; } }

            public string LogSystem { get { return m_LogMessage.LogSystem; } }

            public string CategoryName { get { return m_LogMessage.CategoryName; } }

            public string UserName { get { return m_LogMessage.UserName; } }

            public string Caption { get { return m_LogMessage.Caption; } }

            public string Description { get { return m_LogMessage.Description; } }

            public string Details { get { return m_LogMessage.Details; } }

            public string MethodName { get { return m_LogMessage.MethodName; } }

            public string ClassName { get { return m_LogMessage.ClassName; } }

            public string FileName { get { return m_LogMessage.FileName; } }

            public int LineNumber { get { return m_LogMessage.LineNumber; } }

            public bool HasException { get { return m_LogMessage.HasException; } }

            public IExceptionInfo Exception { get { return m_LogMessage.Exception; } }

            public int ThreadId { get { return m_LogMessage.ThreadId; } }

            public string ThreadName { get { return m_LogMessage.ThreadName; } }

            public int DomainId { get { return m_LogMessage.DomainId; } }

            public string DomainName { get { return m_LogMessage.DomainName; } }

            public bool IsBackground { get { return m_LogMessage.IsBackground; } }

            public bool IsThreadPoolThread { get { return m_LogMessage.IsThreadPoolThread; } }

            public bool HasThreadInfo { get { return m_LogMessage.HasThreadInfo; } }

            public bool HasMethodInfo { get { return m_LogMessage.HasMethodInfo; } }

            public bool HasSourceLocation { get { return m_LogMessage.HasSourceLocation; } }

            /// <summary>
            ///  Provide TimeStamp as DateTime for GLV (SourceGrid doesn't do DateTimeOffset)
            /// </summary>
            /// <remarks>Added for GLV support</remarks>
            public DateTime TimestampDateTime { get { return Timestamp.DateTime; } }

            /// <summary>A combined caption &amp; description</summary>
            /// <remarks>Added for GLV support</remarks>
            public string Message
            {
                get
                {
                    if (m_Message == null) //that's deliberate - null means not calculated, empty string means calculated as empty.
                    {
                        bool haveCaption = (string.IsNullOrEmpty(m_LogMessage.Caption) == false);
                        bool haveDescription = (string.IsNullOrEmpty(m_LogMessage.Description) == false);

                        if (haveCaption && haveDescription)
                        {
                            m_Message = StringReference.GetReference(m_LogMessage.Caption + "\r\n" + m_LogMessage.Description);
                        }
                        else if (haveCaption)
                        {
                            m_Message = m_LogMessage.Caption;
                        }
                        else if (haveDescription)
                        {
                            m_Message = m_LogMessage.Description;
                        }
                        else
                        {
                            //use an empty string - it's empty. then we won't do this property check again.
                            m_Message = string.Empty;
                        }
                    }

                    return m_Message;
                }
            }

            /// <summary>
            /// A display string for the full class and method if available, otherwise an empty string.
            /// </summary>
            /// <remarks>Added for GLV support</remarks>
            public string MethodFullName
            {
                get
                {
                    return ((string.IsNullOrEmpty(ClassName) == false) && (string.IsNullOrEmpty(MethodName) == false)) ?
                        StringReference.GetReference(ClassName + "." + MethodName) : string.Empty;
                }
            }

            /// <summary>
            /// A display string for the full file name and line number if available, otherwise an empty string.
            /// </summary>
            /// <remarks>Added for GLV support</remarks>
            public string SourceCodeLocation
            {
                get
                {
                    return (string.IsNullOrEmpty(FileName) == false) ?
                        StringReference.GetReference(string.Format(FileSystemTools.UICultureFormat, "{0} ({1:N0})", FileName, LineNumber))
                        : string.Empty;
                }
            }

        }

        #endregion
    }
}
