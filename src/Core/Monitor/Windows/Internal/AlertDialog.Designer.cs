namespace Gibraltar.Monitor.Windows.Internal
{
    partial class AlertDialog
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlertDialog));
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblButtonDivider = new System.Windows.Forms.Label();
            this.lblShowHideDetails = new System.Windows.Forms.Label();
            this.vistaEmulationImages = new System.Windows.Forms.ImageList(this.components);
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblMainInstruction = new System.Windows.Forms.Label();
            this.messageFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.lblErrorSummary = new System.Windows.Forms.Label();
            this.chkIgnoreAllProblems = new System.Windows.Forms.CheckBox();
            this.chkReportProblems = new System.Windows.Forms.CheckBox();
            this.exceptionListViewer = new Gibraltar.Monitor.Windows.Internal.ExceptionListViewer();
            this.buttonPanel.SuspendLayout();
            this.messageFlowLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPanel
            // 
            this.buttonPanel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPanel.Controls.Add(this.lblButtonDivider);
            this.buttonPanel.Controls.Add(this.lblShowHideDetails);
            this.buttonPanel.Controls.Add(this.btnRestart);
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 289);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(462, 50);
            this.buttonPanel.TabIndex = 0;
            // 
            // lblButtonDivider
            // 
            this.lblButtonDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblButtonDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblButtonDivider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblButtonDivider.Location = new System.Drawing.Point(0, 0);
            this.lblButtonDivider.Name = "lblButtonDivider";
            this.lblButtonDivider.Size = new System.Drawing.Size(462, 2);
            this.lblButtonDivider.TabIndex = 9;
            // 
            // lblShowHideDetails
            // 
            this.lblShowHideDetails.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblShowHideDetails.ImageIndex = 3;
            this.lblShowHideDetails.ImageList = this.vistaEmulationImages;
            this.lblShowHideDetails.Location = new System.Drawing.Point(12, 13);
            this.lblShowHideDetails.Name = "lblShowHideDetails";
            this.lblShowHideDetails.Size = new System.Drawing.Size(106, 23);
            this.lblShowHideDetails.TabIndex = 4;
            this.lblShowHideDetails.Text = "        More options";
            this.lblShowHideDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblShowHideDetails.Click += new System.EventHandler(this.lblShowHideDetails_Click);
            this.lblShowHideDetails.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblShowHideDetails_MouseDown);
            this.lblShowHideDetails.MouseEnter += new System.EventHandler(this.lblShowHideDetails_MouseEnter);
            this.lblShowHideDetails.MouseLeave += new System.EventHandler(this.lblShowHideDetails_MouseLeave);
            this.lblShowHideDetails.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblShowHideDetails_MouseUp);
            // 
            // vistaEmulationImages
            // 
            this.vistaEmulationImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("vistaEmulationImages.ImageStream")));
            this.vistaEmulationImages.TransparentColor = System.Drawing.Color.Fuchsia;
            this.vistaEmulationImages.Images.SetKeyName(0, "arrow_up_bw");
            this.vistaEmulationImages.Images.SetKeyName(1, "arrow_up_color");
            this.vistaEmulationImages.Images.SetKeyName(2, "arrow_up_color_pressed");
            this.vistaEmulationImages.Images.SetKeyName(3, "arrow_down_bw");
            this.vistaEmulationImages.Images.SetKeyName(4, "arrow_down_color");
            this.vistaEmulationImages.Images.SetKeyName(5, "arrow_down_color_pressed");
            this.vistaEmulationImages.Images.SetKeyName(6, "green_arrow.bmp");
            // 
            // btnRestart
            // 
            this.btnRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestart.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnRestart.Location = new System.Drawing.Point(251, 9);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(117, 27);
            this.btnRestart.TabIndex = 1;
            this.btnRestart.Text = "&Restart Application";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(374, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(76, 27);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblMainInstruction
            // 
            this.lblMainInstruction.AutoSize = true;
            this.lblMainInstruction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMainInstruction.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lblMainInstruction.Location = new System.Drawing.Point(0, 0);
            this.lblMainInstruction.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblMainInstruction.Name = "lblMainInstruction";
            this.lblMainInstruction.Size = new System.Drawing.Size(376, 40);
            this.lblMainInstruction.TabIndex = 2;
            this.lblMainInstruction.Text = "{Application Name} has encountered an unexpected problem";
            // 
            // messageFlowLayout
            // 
            this.messageFlowLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messageFlowLayout.Controls.Add(this.lblMainInstruction);
            this.messageFlowLayout.Controls.Add(this.lblErrorSummary);
            this.messageFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.messageFlowLayout.Location = new System.Drawing.Point(70, 12);
            this.messageFlowLayout.Name = "messageFlowLayout";
            this.messageFlowLayout.Size = new System.Drawing.Size(379, 74);
            this.messageFlowLayout.TabIndex = 3;
            this.messageFlowLayout.WrapContents = false;
            // 
            // lblErrorSummary
            // 
            this.lblErrorSummary.AutoSize = true;
            this.lblErrorSummary.Location = new System.Drawing.Point(0, 46);
            this.lblErrorSummary.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblErrorSummary.Name = "lblErrorSummary";
            this.lblErrorSummary.Size = new System.Drawing.Size(328, 13);
            this.lblErrorSummary.TabIndex = 3;
            this.lblErrorSummary.Text = "<Short display message from the exception if we can figure one out>";
            // 
            // chkIgnoreAllProblems
            // 
            this.chkIgnoreAllProblems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIgnoreAllProblems.Location = new System.Drawing.Point(74, 266);
            this.chkIgnoreAllProblems.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.chkIgnoreAllProblems.Name = "chkIgnoreAllProblems";
            this.chkIgnoreAllProblems.Size = new System.Drawing.Size(375, 17);
            this.chkIgnoreAllProblems.TabIndex = 4;
            this.chkIgnoreAllProblems.Text = "&Ignore any new problems for the remainder of this session";
            this.chkIgnoreAllProblems.UseVisualStyleBackColor = false;
            this.chkIgnoreAllProblems.CheckedChanged += new System.EventHandler(this.chkIgnoreAllProblems_CheckedChanged);
            // 
            // chkReportProblems
            // 
            this.chkReportProblems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkReportProblems.Location = new System.Drawing.Point(74, 237);
            this.chkReportProblems.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.chkReportProblems.Name = "chkReportProblems";
            this.chkReportProblems.Size = new System.Drawing.Size(375, 17);
            this.chkReportProblems.TabIndex = 6;
            this.chkReportProblems.Text = "&Automatically report these problems at the end of the session";
            this.chkReportProblems.UseVisualStyleBackColor = false;
            this.chkReportProblems.CheckedChanged += new System.EventHandler(this.chkReportProblems_CheckedChanged);
            // 
            // exceptionListViewer
            // 
            this.exceptionListViewer.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.exceptionListViewer.Location = new System.Drawing.Point(12, 92);
            this.exceptionListViewer.Name = "exceptionListViewer";
            this.exceptionListViewer.Size = new System.Drawing.Size(437, 121);
            this.exceptionListViewer.TabIndex = 5;
            // 
            // AlertDialog
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(462, 339);
            this.Controls.Add(this.chkReportProblems);
            this.Controls.Add(this.exceptionListViewer);
            this.Controls.Add(this.messageFlowLayout);
            this.Controls.Add(this.chkIgnoreAllProblems);
            this.Controls.Add(this.buttonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AlertDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AlertDialog";
            this.TopMost = true;
            this.buttonPanel.ResumeLayout(false);
            this.messageFlowLayout.ResumeLayout(false);
            this.messageFlowLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ImageList vistaEmulationImages;
        private System.Windows.Forms.Label lblShowHideDetails;
        private System.Windows.Forms.Label lblMainInstruction;
        private System.Windows.Forms.FlowLayoutPanel messageFlowLayout;
        private System.Windows.Forms.Label lblErrorSummary;
        private System.Windows.Forms.CheckBox chkIgnoreAllProblems;
        private ExceptionListViewer exceptionListViewer;
        private System.Windows.Forms.Label lblButtonDivider;
        private System.Windows.Forms.CheckBox chkReportProblems;
    }
}