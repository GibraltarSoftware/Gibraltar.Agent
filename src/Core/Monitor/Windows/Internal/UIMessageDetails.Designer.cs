namespace Gibraltar.Monitor.Windows.Internal
{
    partial class UIMessageDetails
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            DisposeLooseTabs(disposing);

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UIMessageDetails));
            this.messageTabControl = new System.Windows.Forms.TabControl();
            this.detailPanel = new System.Windows.Forms.TabPage();
            this.logMessage = new Gibraltar.Monitor.Windows.UILogMessage();
            this.exceptionPanel = new System.Windows.Forms.TabPage();
            this.exceptionDetail = new Gibraltar.Monitor.Windows.UIExceptionDetail();
            this.detailXmlPanel = new System.Windows.Forms.TabPage();
            this.detailsXmlViewer = new Gibraltar.Monitor.Windows.UILogMessageDetail();
            this.sourceCodePanel = new System.Windows.Forms.TabPage();
            this.sourceCodeViewer = new Gibraltar.Monitor.Windows.UISourceViewer();
            this.smallIconImages = new System.Windows.Forms.ImageList(this.components);
            this.messageTabControl.SuspendLayout();
            this.detailPanel.SuspendLayout();
            this.exceptionPanel.SuspendLayout();
            this.detailXmlPanel.SuspendLayout();
            this.sourceCodePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // messageTabControl
            // 
            this.messageTabControl.Controls.Add(this.detailPanel);
            this.messageTabControl.Controls.Add(this.exceptionPanel);
            this.messageTabControl.Controls.Add(this.detailXmlPanel);
            this.messageTabControl.Controls.Add(this.sourceCodePanel);
            this.messageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageTabControl.ImageList = this.smallIconImages;
            this.messageTabControl.Location = new System.Drawing.Point(0, 0);
            this.messageTabControl.Name = "messageTabControl";
            this.messageTabControl.SelectedIndex = 0;
            this.messageTabControl.Size = new System.Drawing.Size(742, 305);
            this.messageTabControl.TabIndex = 0;
            this.messageTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.messageTabControl_Selected);
            // 
            // detailPanel
            // 
            this.detailPanel.Controls.Add(this.logMessage);
            this.detailPanel.ImageKey = "details";
            this.detailPanel.Location = new System.Drawing.Point(4, 23);
            this.detailPanel.Name = "detailPanel";
            this.detailPanel.Size = new System.Drawing.Size(734, 278);
            this.detailPanel.TabIndex = 0;
            this.detailPanel.Text = "Log Message";
            this.detailPanel.UseVisualStyleBackColor = true;
            // 
            // logMessage
            // 
            this.logMessage.BackColor = System.Drawing.SystemColors.Window;
            this.logMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logMessage.Location = new System.Drawing.Point(0, 0);
            this.logMessage.Margin = new System.Windows.Forms.Padding(0);
            this.logMessage.Name = "logMessage";
            this.logMessage.ShowDescriptionPreformatted = false;
            this.logMessage.ShowLocationInfo = true;
            this.logMessage.ShowThreadInfo = true;
            this.logMessage.Size = new System.Drawing.Size(734, 278);
            this.logMessage.TabIndex = 0;
            // 
            // exceptionPanel
            // 
            this.exceptionPanel.Controls.Add(this.exceptionDetail);
            this.exceptionPanel.ImageKey = "flag_blue";
            this.exceptionPanel.Location = new System.Drawing.Point(4, 23);
            this.exceptionPanel.Name = "exceptionPanel";
            this.exceptionPanel.Size = new System.Drawing.Size(734, 278);
            this.exceptionPanel.TabIndex = 1;
            this.exceptionPanel.Text = "Exceptions";
            this.exceptionPanel.UseVisualStyleBackColor = true;
            // 
            // exceptionDetail
            // 
            this.exceptionDetail.BackColor = System.Drawing.SystemColors.Window;
            this.exceptionDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exceptionDetail.Location = new System.Drawing.Point(0, 0);
            this.exceptionDetail.Margin = new System.Windows.Forms.Padding(0);
            this.exceptionDetail.Name = "exceptionDetail";
            this.exceptionDetail.Size = new System.Drawing.Size(734, 278);
            this.exceptionDetail.TabIndex = 0;
            this.exceptionDetail.TitleChanged += new System.EventHandler(this.exceptionDetail_TitleChanged);
            // 
            // detailXmlPanel
            // 
            this.detailXmlPanel.Controls.Add(this.detailsXmlViewer);
            this.detailXmlPanel.Location = new System.Drawing.Point(4, 23);
            this.detailXmlPanel.Name = "detailXmlPanel";
            this.detailXmlPanel.Size = new System.Drawing.Size(734, 278);
            this.detailXmlPanel.TabIndex = 2;
            this.detailXmlPanel.Text = "Details";
            this.detailXmlPanel.UseVisualStyleBackColor = true;
            // 
            // detailsXmlViewer
            // 
            this.detailsXmlViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsXmlViewer.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.detailsXmlViewer.Location = new System.Drawing.Point(0, 0);
            this.detailsXmlViewer.Name = "detailsXmlViewer";
            this.detailsXmlViewer.Size = new System.Drawing.Size(734, 278);
            this.detailsXmlViewer.TabIndex = 0;
            this.detailsXmlViewer.TitleChanged += new System.EventHandler(this.detailsXmlViewer_TitleChanged);
            // 
            // sourceCodePanel
            // 
            this.sourceCodePanel.Controls.Add(this.sourceCodeViewer);
            this.sourceCodePanel.ImageKey = "source";
            this.sourceCodePanel.Location = new System.Drawing.Point(4, 23);
            this.sourceCodePanel.Name = "sourceCodePanel";
            this.sourceCodePanel.Size = new System.Drawing.Size(734, 278);
            this.sourceCodePanel.TabIndex = 3;
            this.sourceCodePanel.Text = "Source Code";
            this.sourceCodePanel.UseVisualStyleBackColor = true;
            // 
            // sourceCodeViewer
            // 
            this.sourceCodeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeViewer.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.sourceCodeViewer.Location = new System.Drawing.Point(0, 0);
            this.sourceCodeViewer.Name = "sourceCodeViewer";
            this.sourceCodeViewer.Size = new System.Drawing.Size(734, 278);
            this.sourceCodeViewer.TabIndex = 0;
            this.sourceCodeViewer.TitleChanged += new System.EventHandler(this.sourceCodeViewer_TitleChanged);
            // 
            // smallIconImages
            // 
            this.smallIconImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallIconImages.ImageStream")));
            this.smallIconImages.TransparentColor = System.Drawing.Color.Magenta;
            this.smallIconImages.Images.SetKeyName(0, "critical");
            this.smallIconImages.Images.SetKeyName(1, "error");
            this.smallIconImages.Images.SetKeyName(2, "warning");
            this.smallIconImages.Images.SetKeyName(3, "information");
            this.smallIconImages.Images.SetKeyName(4, "flag_blue");
            this.smallIconImages.Images.SetKeyName(5, "flag_green");
            this.smallIconImages.Images.SetKeyName(6, "flag_red");
            this.smallIconImages.Images.SetKeyName(7, "comment");
            this.smallIconImages.Images.SetKeyName(8, "details");
            this.smallIconImages.Images.SetKeyName(9, "source");
            // 
            // UIMessageDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.messageTabControl);
            this.Name = "UIMessageDetails";
            this.Size = new System.Drawing.Size(742, 305);
            this.messageTabControl.ResumeLayout(false);
            this.detailPanel.ResumeLayout(false);
            this.exceptionPanel.ResumeLayout(false);
            this.detailXmlPanel.ResumeLayout(false);
            this.sourceCodePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl messageTabControl;
        private System.Windows.Forms.TabPage detailPanel;
        private System.Windows.Forms.TabPage exceptionPanel;
        private System.Windows.Forms.TabPage detailXmlPanel;
        private System.Windows.Forms.TabPage sourceCodePanel;
        private UIExceptionDetail exceptionDetail;
        private UILogMessage logMessage;
        private UILogMessageDetail detailsXmlViewer;
        private UISourceViewer sourceCodeViewer;
        private System.Windows.Forms.ImageList smallIconImages;
    }
}
