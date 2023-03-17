namespace Gibraltar.Monitor.Windows
{
    partial class LiveLogViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) // TODO: This method needs a redundant-dispose preventer?
            {
                components.Dispose();
            }

            ShutdownViewer();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LiveLogViewer));
            this.ToolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.threadsViewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.messageDetailsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.messageDetails = new Gibraltar.Monitor.Windows.Internal.UIMessageDetails();
            this.uiThreadTrackViewer1 = new Gibraltar.Monitor.Windows.Internal.UIThreadTrackViewer();
            this.statusBarToolStrip = new System.Windows.Forms.ToolStrip();
            this.RunToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.PauseToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MoveFirstToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.MoveLastToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.CopyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.downloadToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ShowDetailsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ClearToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.DetailsToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ViewerStatusToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.CriticalToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ErrorToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.WarningToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.InformationToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.VerboseToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.SearchToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ResetSearchToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ToolStripContainer1.ContentPanel.SuspendLayout();
            this.ToolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.ToolStripContainer1.SuspendLayout();
            this.threadsViewSplitContainer.Panel1.SuspendLayout();
            this.threadsViewSplitContainer.Panel2.SuspendLayout();
            this.threadsViewSplitContainer.SuspendLayout();
            this.messageDetailsSplitContainer.Panel2.SuspendLayout();
            this.messageDetailsSplitContainer.SuspendLayout();
            this.statusBarToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStripContainer1
            // 
            // 
            // ToolStripContainer1.ContentPanel
            // 
            this.ToolStripContainer1.ContentPanel.Controls.Add(this.threadsViewSplitContainer);
            this.ToolStripContainer1.ContentPanel.Size = new System.Drawing.Size(964, 600);
            this.ToolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.ToolStripContainer1.Name = "ToolStripContainer1";
            this.ToolStripContainer1.Size = new System.Drawing.Size(964, 625);
            this.ToolStripContainer1.TabIndex = 3;
            this.ToolStripContainer1.Text = "ToolStripContainer1";
            // 
            // ToolStripContainer1.TopToolStripPanel
            // 
            this.ToolStripContainer1.TopToolStripPanel.Controls.Add(this.statusBarToolStrip);
            // 
            // threadsViewSplitContainer
            // 
            this.threadsViewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadsViewSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.threadsViewSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.threadsViewSplitContainer.Name = "threadsViewSplitContainer";
            // 
            // threadsViewSplitContainer.Panel1
            // 
            this.threadsViewSplitContainer.Panel1.Controls.Add(this.messageDetailsSplitContainer);
            // 
            // threadsViewSplitContainer.Panel2
            // 
            this.threadsViewSplitContainer.Panel2.Controls.Add(this.uiThreadTrackViewer1);
            this.threadsViewSplitContainer.Size = new System.Drawing.Size(964, 600);
            this.threadsViewSplitContainer.SplitterDistance = 620;
            this.threadsViewSplitContainer.TabIndex = 0;
            this.threadsViewSplitContainer.Resize += new System.EventHandler(this.threadsViewSplitContainer_Resize);
            // 
            // messageDetailsSplitContainer
            // 
            this.messageDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageDetailsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.messageDetailsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.messageDetailsSplitContainer.Name = "messageDetailsSplitContainer";
            this.messageDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.messageDetailsSplitContainer.Panel1MinSize = 85;
            // 
            // messageDetailsSplitContainer.Panel2
            // 
            this.messageDetailsSplitContainer.Panel2.Controls.Add(this.messageDetails);
            this.messageDetailsSplitContainer.Panel2MinSize = 134;
            this.messageDetailsSplitContainer.Size = new System.Drawing.Size(620, 600);
            this.messageDetailsSplitContainer.SplitterDistance = 413;
            this.messageDetailsSplitContainer.TabIndex = 0;
            this.messageDetailsSplitContainer.Resize += new System.EventHandler(this.messageDetailsSplitContainer_Resize);
            // 
            // messageDetails
            // 
            this.messageDetails.BackColor = System.Drawing.SystemColors.Window;
            this.messageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageDetails.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.messageDetails.Location = new System.Drawing.Point(0, 0);
            this.messageDetails.Name = "messageDetails";
            this.messageDetails.ShowDescriptionPreformatted = false;
            this.messageDetails.Size = new System.Drawing.Size(620, 183);
            this.messageDetails.TabIndex = 0;
            // 
            // uiThreadTrackViewer1
            // 
            this.uiThreadTrackViewer1.BackColor = System.Drawing.SystemColors.Window;
            this.uiThreadTrackViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiThreadTrackViewer1.Location = new System.Drawing.Point(0, 0);
            this.uiThreadTrackViewer1.Name = "uiThreadTrackViewer1";
            this.uiThreadTrackViewer1.Size = new System.Drawing.Size(340, 600);
            this.uiThreadTrackViewer1.TabIndex = 0;
            // 
            // statusBarToolStrip
            // 
            this.statusBarToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusBarToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.statusBarToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RunToolStripButton,
            this.PauseToolStripButton,
            this.ToolStripSeparator3,
            this.MoveFirstToolStripButton,
            this.MoveLastToolStripButton,
            this.ToolStripSeparator4,
            this.SaveToolStripButton,
            this.CopyToolStripButton,
            this.ToolStripSeparator1,
            this.downloadToolStripButton,
            this.ShowDetailsToolStripButton,
            this.ClearToolStripButton,
            this.DetailsToolStripSeparator,
            this.toolStripLabel1,
            this.ViewerStatusToolStripLabel,
            this.CriticalToolStripButton,
            this.ErrorToolStripButton,
            this.WarningToolStripButton,
            this.InformationToolStripButton,
            this.VerboseToolStripButton,
            this.SearchToolStripTextBox,
            this.SearchToolStripButton,
            this.ResetSearchToolStripButton});
            this.statusBarToolStrip.Location = new System.Drawing.Point(0, 0);
            this.statusBarToolStrip.Name = "statusBarToolStrip";
            this.statusBarToolStrip.Size = new System.Drawing.Size(964, 25);
            this.statusBarToolStrip.Stretch = true;
            this.statusBarToolStrip.TabIndex = 1;
            this.statusBarToolStrip.Text = "Status Bar Tools";
            // 
            // RunToolStripButton
            // 
            this.RunToolStripButton.Checked = true;
            this.RunToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RunToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Play;
            this.RunToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RunToolStripButton.Name = "RunToolStripButton";
            this.RunToolStripButton.Size = new System.Drawing.Size(138, 22);
            this.RunToolStripButton.Text = "Click to Auto Refresh";
            this.RunToolStripButton.ToolTipText = "Automatically update and scroll the display to show that latest results";
            this.RunToolStripButton.Click += new System.EventHandler(this.RunToolStripButton_Click);
            // 
            // PauseToolStripButton
            // 
            this.PauseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PauseToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Pause;
            this.PauseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PauseToolStripButton.Name = "PauseToolStripButton";
            this.PauseToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.PauseToolStripButton.Text = "Pause";
            this.PauseToolStripButton.ToolTipText = "Click to freeze the display and analyze results without any updates.";
            this.PauseToolStripButton.Click += new System.EventHandler(this.PauseToolStripButton_Click);
            // 
            // ToolStripSeparator3
            // 
            this.ToolStripSeparator3.Name = "ToolStripSeparator3";
            this.ToolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // MoveFirstToolStripButton
            // 
            this.MoveFirstToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MoveFirstToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.FastReverse;
            this.MoveFirstToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MoveFirstToolStripButton.Name = "MoveFirstToolStripButton";
            this.MoveFirstToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.MoveFirstToolStripButton.Text = "Move First";
            this.MoveFirstToolStripButton.ToolTipText = "Move to the first (oldest) result";
            this.MoveFirstToolStripButton.Click += new System.EventHandler(this.MoveFirstToolStripButton_Click);
            // 
            // MoveLastToolStripButton
            // 
            this.MoveLastToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MoveLastToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.FastForward;
            this.MoveLastToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MoveLastToolStripButton.Name = "MoveLastToolStripButton";
            this.MoveLastToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.MoveLastToolStripButton.Text = "Move Last";
            this.MoveLastToolStripButton.ToolTipText = "Move to the last (most current) record";
            this.MoveLastToolStripButton.Click += new System.EventHandler(this.MoveLastToolStripButton_Click);
            // 
            // ToolStripSeparator4
            // 
            this.ToolStripSeparator4.Name = "ToolStripSeparator4";
            this.ToolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // SaveToolStripButton
            // 
            this.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Save;
            this.SaveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveToolStripButton.Name = "SaveToolStripButton";
            this.SaveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SaveToolStripButton.Text = "Save";
            this.SaveToolStripButton.ToolTipText = "Package and Send session information to a file or via email";
            this.SaveToolStripButton.Click += new System.EventHandler(this.SaveToolStripButton_Click);
            // 
            // CopyToolStripButton
            // 
            this.CopyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CopyToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Copy;
            this.CopyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CopyToolStripButton.Name = "CopyToolStripButton";
            this.CopyToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.CopyToolStripButton.Text = "Copy";
            this.CopyToolStripButton.ToolTipText = "Copy the entire grid to the clipboard";
            this.CopyToolStripButton.Click += new System.EventHandler(this.CopyToolStripButton_Click);
            // 
            // ToolStripSeparator1
            // 
            this.ToolStripSeparator1.Name = "ToolStripSeparator1";
            this.ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // downloadToolStripButton
            // 
            this.downloadToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.downloadToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.downloadToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Download;
            this.downloadToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.downloadToolStripButton.Name = "downloadToolStripButton";
            this.downloadToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.downloadToolStripButton.Text = "Go to OnLoupe.com for more information about Loupe";
            this.downloadToolStripButton.Click += new System.EventHandler(this.downloadToolStripButton_Click);
            // 
            // ShowDetailsToolStripButton
            // 
            this.ShowDetailsToolStripButton.Checked = true;
            this.ShowDetailsToolStripButton.CheckOnClick = true;
            this.ShowDetailsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowDetailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowDetailsToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Code;
            this.ShowDetailsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowDetailsToolStripButton.Name = "ShowDetailsToolStripButton";
            this.ShowDetailsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ShowDetailsToolStripButton.Text = "Show Details";
            this.ShowDetailsToolStripButton.ToolTipText = "Include details about the thread and call information like class, method, and sou" +
    "rce code location.";
            this.ShowDetailsToolStripButton.Click += new System.EventHandler(this.ShowDetailsToolStripButton_Click);
            // 
            // ClearToolStripButton
            // 
            this.ClearToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ClearToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.DeleteMessages;
            this.ClearToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ClearToolStripButton.Name = "ClearToolStripButton";
            this.ClearToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ClearToolStripButton.Text = "Clear";
            this.ClearToolStripButton.ToolTipText = "Clear all messages from the grid";
            this.ClearToolStripButton.Click += new System.EventHandler(this.ClearToolStripButton_Click);
            // 
            // DetailsToolStripSeparator
            // 
            this.DetailsToolStripSeparator.Name = "DetailsToolStripSeparator";
            this.DetailsToolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel1.Text = "Show:";
            // 
            // ViewerStatusToolStripLabel
            // 
            this.ViewerStatusToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ViewerStatusToolStripLabel.Name = "ViewerStatusToolStripLabel";
            this.ViewerStatusToolStripLabel.Size = new System.Drawing.Size(93, 22);
            this.ViewerStatusToolStripLabel.Text = "<Viewer Status>";
            // 
            // CriticalToolStripButton
            // 
            this.CriticalToolStripButton.CheckOnClick = true;
            this.CriticalToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("CriticalToolStripButton.Image")));
            this.CriticalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CriticalToolStripButton.Name = "CriticalToolStripButton";
            this.CriticalToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.CriticalToolStripButton.Text = "0";
            this.CriticalToolStripButton.ToolTipText = "Click to show only Critical severity (narrowest filter)";
            this.CriticalToolStripButton.Click += new System.EventHandler(this.CriticalToolStripButton_Click);
            // 
            // ErrorToolStripButton
            // 
            this.ErrorToolStripButton.CheckOnClick = true;
            this.ErrorToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ErrorToolStripButton.Image")));
            this.ErrorToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ErrorToolStripButton.Name = "ErrorToolStripButton";
            this.ErrorToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.ErrorToolStripButton.Text = "0";
            this.ErrorToolStripButton.ToolTipText = "Click to show only severity of Error and above";
            this.ErrorToolStripButton.Click += new System.EventHandler(this.ErrorToolStripButton_Click);
            // 
            // WarningToolStripButton
            // 
            this.WarningToolStripButton.CheckOnClick = true;
            this.WarningToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("WarningToolStripButton.Image")));
            this.WarningToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WarningToolStripButton.Name = "WarningToolStripButton";
            this.WarningToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.WarningToolStripButton.Text = "0";
            this.WarningToolStripButton.ToolTipText = "Click to show only severity of Warning and above";
            this.WarningToolStripButton.Click += new System.EventHandler(this.WarningToolStripButton_Click);
            // 
            // InformationToolStripButton
            // 
            this.InformationToolStripButton.CheckOnClick = true;
            this.InformationToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("InformationToolStripButton.Image")));
            this.InformationToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InformationToolStripButton.Name = "InformationToolStripButton";
            this.InformationToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.InformationToolStripButton.Text = "0";
            this.InformationToolStripButton.ToolTipText = "Click to show only severity of Information and above";
            this.InformationToolStripButton.Click += new System.EventHandler(this.InformationToolStripButton_Click);
            // 
            // VerboseToolStripButton
            // 
            this.VerboseToolStripButton.CheckOnClick = true;
            this.VerboseToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("VerboseToolStripButton.Image")));
            this.VerboseToolStripButton.Name = "VerboseToolStripButton";
            this.VerboseToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.VerboseToolStripButton.Text = "0";
            this.VerboseToolStripButton.ToolTipText = "Click to hide messages with Verbose severity";
            this.VerboseToolStripButton.Click += new System.EventHandler(this.VerboseToolStripButton_Click);
            // 
            // SearchToolStripTextBox
            // 
            this.SearchToolStripTextBox.AcceptsReturn = true;
            this.SearchToolStripTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.SearchToolStripTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.SearchToolStripTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SearchToolStripTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchToolStripTextBox.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.SearchToolStripTextBox.Margin = new System.Windows.Forms.Padding(6, 0, 1, 0);
            this.SearchToolStripTextBox.Name = "SearchToolStripTextBox";
            this.SearchToolStripTextBox.Size = new System.Drawing.Size(150, 25);
            this.SearchToolStripTextBox.Text = "Search Messages";
            this.SearchToolStripTextBox.Enter += new System.EventHandler(this.SearchToolStripTextBox_Enter);
            this.SearchToolStripTextBox.Leave += new System.EventHandler(this.SearchToolStripTextBox_Leave);
            this.SearchToolStripTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchToolStripTextBox_KeyDown);
            this.SearchToolStripTextBox.TextChanged += new System.EventHandler(this.SearchToolStripTextBox_TextChanged);
            // 
            // SearchToolStripButton
            // 
            this.SearchToolStripButton.Checked = true;
            this.SearchToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.Search;
            this.SearchToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchToolStripButton.Name = "SearchToolStripButton";
            this.SearchToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchToolStripButton.Text = "Search";
            this.SearchToolStripButton.Visible = false;
            this.SearchToolStripButton.Click += new System.EventHandler(this.SearchToolStripButton_Click);
            // 
            // ResetSearchToolStripButton
            // 
            this.ResetSearchToolStripButton.Checked = true;
            this.ResetSearchToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ResetSearchToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ResetSearchToolStripButton.Image = global::Gibraltar.Monitor.Windows.UIResources.CancelSearch;
            this.ResetSearchToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ResetSearchToolStripButton.Name = "ResetSearchToolStripButton";
            this.ResetSearchToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ResetSearchToolStripButton.Text = "Reset";
            this.ResetSearchToolStripButton.Click += new System.EventHandler(this.ClearSearchToolStripButton_Click);
            // 
            // LiveLogViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ToolStripContainer1);
            this.Name = "LiveLogViewer";
            this.Size = new System.Drawing.Size(964, 625);
            this.ParentChanged += new System.EventHandler(this.CMViewer_ParentChanged);
            this.ToolStripContainer1.ContentPanel.ResumeLayout(false);
            this.ToolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.ToolStripContainer1.TopToolStripPanel.PerformLayout();
            this.ToolStripContainer1.ResumeLayout(false);
            this.ToolStripContainer1.PerformLayout();
            this.threadsViewSplitContainer.Panel1.ResumeLayout(false);
            this.threadsViewSplitContainer.Panel2.ResumeLayout(false);
            this.threadsViewSplitContainer.ResumeLayout(false);
            this.messageDetailsSplitContainer.Panel2.ResumeLayout(false);
            this.messageDetailsSplitContainer.ResumeLayout(false);
            this.statusBarToolStrip.ResumeLayout(false);
            this.statusBarToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ToolStripButton MoveFirstToolStripButton;
        internal System.Windows.Forms.ToolStripButton MoveLastToolStripButton;
        internal System.Windows.Forms.ToolStripButton ClearToolStripButton;
        internal System.Windows.Forms.ToolStripContainer ToolStripContainer1;
        internal System.Windows.Forms.ToolStrip statusBarToolStrip;
        internal System.Windows.Forms.ToolStripButton RunToolStripButton;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator4;
        internal System.Windows.Forms.ToolStripButton SaveToolStripButton;
        internal System.Windows.Forms.ToolStripButton CopyToolStripButton;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel ViewerStatusToolStripLabel;
        private System.Windows.Forms.ToolStripButton ShowDetailsToolStripButton;
        private System.Windows.Forms.ToolStripButton PauseToolStripButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton CriticalToolStripButton;
        private System.Windows.Forms.ToolStripButton ErrorToolStripButton;
        private System.Windows.Forms.ToolStripButton WarningToolStripButton;
        private System.Windows.Forms.ToolStripButton InformationToolStripButton;
        private System.Windows.Forms.ToolStripButton VerboseToolStripButton;
        private System.Windows.Forms.ToolStripTextBox SearchToolStripTextBox;
        private System.Windows.Forms.ToolStripButton SearchToolStripButton;
        private System.Windows.Forms.ToolStripButton ResetSearchToolStripButton;
        private System.Windows.Forms.ToolStripSeparator DetailsToolStripSeparator;
        private System.Windows.Forms.ToolStripButton downloadToolStripButton;
        private System.Windows.Forms.SplitContainer threadsViewSplitContainer;
        private System.Windows.Forms.SplitContainer messageDetailsSplitContainer;
        private Gibraltar.Monitor.Windows.Internal.UIMessageDetails messageDetails;
        private Gibraltar.Monitor.Windows.Internal.UIThreadTrackViewer uiThreadTrackViewer1;
    }
}
