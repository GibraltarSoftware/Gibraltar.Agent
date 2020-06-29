namespace Gibraltar.Monitor.Windows.Internal
{

    partial class UIPackagerCriteria
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.optJustThisSession = new System.Windows.Forms.RadioButton();
            this.optAllSessions = new System.Windows.Forms.RadioButton();
            this.optNewSessions = new System.Windows.Forms.RadioButton();
            this.mainLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblJustThisSession = new System.Windows.Forms.Label();
            this.lblAllInformation = new System.Windows.Forms.Label();
            this.runningAppNotePanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblRunningAppNote = new System.Windows.Forms.Label();
            this.mainLayoutPanel.SuspendLayout();
            this.runningAppNotePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // optJustThisSession
            // 
            this.optJustThisSession.AutoSize = true;
            this.optJustThisSession.BackColor = System.Drawing.Color.Transparent;
            this.optJustThisSession.Location = new System.Drawing.Point(3, 58);
            this.optJustThisSession.Name = "optJustThisSession";
            this.optJustThisSession.Size = new System.Drawing.Size(101, 17);
            this.optJustThisSession.TabIndex = 0;
            this.optJustThisSession.TabStop = true;
            this.optJustThisSession.Text = "Just this session";
            this.optJustThisSession.UseVisualStyleBackColor = false;
            this.optJustThisSession.Validating += new System.ComponentModel.CancelEventHandler(this.optJustThisSession_Validating);
            // 
            // optAllSessions
            // 
            this.optAllSessions.AutoSize = true;
            this.optAllSessions.BackColor = System.Drawing.Color.Transparent;
            this.optAllSessions.Location = new System.Drawing.Point(3, 113);
            this.optAllSessions.Name = "optAllSessions";
            this.optAllSessions.Size = new System.Drawing.Size(178, 17);
            this.optAllSessions.TabIndex = 1;
            this.optAllSessions.TabStop = true;
            this.optAllSessions.Text = "All information for this application";
            this.optAllSessions.UseVisualStyleBackColor = false;
            this.optAllSessions.Validating += new System.ComponentModel.CancelEventHandler(this.optAllSessions_Validating);
            // 
            // optNewSessions
            // 
            this.optNewSessions.AutoSize = true;
            this.optNewSessions.BackColor = System.Drawing.Color.Transparent;
            this.optNewSessions.Location = new System.Drawing.Point(3, 3);
            this.optNewSessions.Name = "optNewSessions";
            this.optNewSessions.Size = new System.Drawing.Size(201, 17);
            this.optNewSessions.TabIndex = 2;
            this.optNewSessions.TabStop = true;
            this.optNewSessions.Text = "Unsent information for this application";
            this.optNewSessions.UseVisualStyleBackColor = false;
            this.optNewSessions.Validating += new System.ComponentModel.CancelEventHandler(this.optNewSessions_Validating);
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.Controls.Add(this.optNewSessions);
            this.mainLayoutPanel.Controls.Add(this.label1);
            this.mainLayoutPanel.Controls.Add(this.optJustThisSession);
            this.mainLayoutPanel.Controls.Add(this.lblJustThisSession);
            this.mainLayoutPanel.Controls.Add(this.optAllSessions);
            this.mainLayoutPanel.Controls.Add(this.lblAllInformation);
            this.mainLayoutPanel.Controls.Add(this.runningAppNotePanel);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.Size = new System.Drawing.Size(416, 253);
            this.mainLayoutPanel.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(410, 26);
            this.label1.TabIndex = 7;
            this.label1.Text = "Session information stored on the local computer that has not been sent before wi" +
                "ll be sent along with this active application session.";
            // 
            // lblJustThisSession
            // 
            this.lblJustThisSession.AutoSize = true;
            this.lblJustThisSession.Location = new System.Drawing.Point(3, 78);
            this.lblJustThisSession.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.lblJustThisSession.Name = "lblJustThisSession";
            this.lblJustThisSession.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblJustThisSession.Size = new System.Drawing.Size(409, 26);
            this.lblJustThisSession.TabIndex = 5;
            this.lblJustThisSession.Text = "Only information about this active application session will be sent.  This will i" +
                "nclude everything that has happened up to the point where you click Finish in th" +
                "is wizard.";
            // 
            // lblAllInformation
            // 
            this.lblAllInformation.AutoSize = true;
            this.lblAllInformation.Location = new System.Drawing.Point(3, 133);
            this.lblAllInformation.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.lblAllInformation.Name = "lblAllInformation";
            this.lblAllInformation.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblAllInformation.Size = new System.Drawing.Size(409, 26);
            this.lblAllInformation.TabIndex = 6;
            this.lblAllInformation.Text = "Any session information stored on the local computer for this application, includ" +
                "ing this active session, will be sent even if it has been sent before.";
            // 
            // runningAppNotePanel
            // 
            this.runningAppNotePanel.Controls.Add(this.pictureBox1);
            this.runningAppNotePanel.Controls.Add(this.lblRunningAppNote);
            this.runningAppNotePanel.Location = new System.Drawing.Point(3, 174);
            this.runningAppNotePanel.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.runningAppNotePanel.Name = "runningAppNotePanel";
            this.runningAppNotePanel.Size = new System.Drawing.Size(395, 56);
            this.runningAppNotePanel.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Gibraltar.Monitor.Windows.UIResources.info;
            this.pictureBox1.Location = new System.Drawing.Point(5, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 49);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // lblRunningAppNote
            // 
            this.lblRunningAppNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRunningAppNote.Location = new System.Drawing.Point(61, 3);
            this.lblRunningAppNote.Name = "lblRunningAppNote";
            this.lblRunningAppNote.Size = new System.Drawing.Size(331, 49);
            this.lblRunningAppNote.TabIndex = 3;
            this.lblRunningAppNote.Text = "Information about any active (running) application will not be included in this p" +
                "ackage.  Exit any copy of the application that you want to include.  ";
            this.lblRunningAppNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UIPackagerCriteria
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayoutPanel);
            this.Name = "UIPackagerCriteria";
            this.Size = new System.Drawing.Size(416, 253);
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.runningAppNotePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton optJustThisSession;
        private System.Windows.Forms.RadioButton optAllSessions;
        private System.Windows.Forms.RadioButton optNewSessions;
        private System.Windows.Forms.FlowLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.Panel runningAppNotePanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblRunningAppNote;
        private System.Windows.Forms.Label lblJustThisSession;
        private System.Windows.Forms.Label lblAllInformation;
        private System.Windows.Forms.Label label1;
    }
}
