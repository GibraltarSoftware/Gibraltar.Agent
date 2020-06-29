namespace Gibraltar.Monitor.Windows
{
    partial class AutoSendConsentDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoSendConsentDialog));
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.lblButtonDivider = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.messageFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.linkPrivacyPolicy = new System.Windows.Forms.LinkLabel();
            this.optOptIn = new System.Windows.Forms.RadioButton();
            this.optOptOut = new System.Windows.Forms.RadioButton();
            this.buttonPanel.SuspendLayout();
            this.messageFlowLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPanel
            // 
            this.buttonPanel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPanel.Controls.Add(this.lblButtonDivider);
            this.buttonPanel.Controls.Add(this.btnSave);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 197);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(519, 50);
            this.buttonPanel.TabIndex = 1;
            // 
            // lblButtonDivider
            // 
            this.lblButtonDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblButtonDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblButtonDivider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblButtonDivider.Location = new System.Drawing.Point(0, 0);
            this.lblButtonDivider.Name = "lblButtonDivider";
            this.lblButtonDivider.Size = new System.Drawing.Size(519, 2);
            this.lblButtonDivider.TabIndex = 9;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(308, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(117, 27);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "&Save Changes";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(431, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 27);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // messageFlowLayout
            // 
            this.messageFlowLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.messageFlowLayout.AutoSize = true;
            this.messageFlowLayout.Controls.Add(this.lblCaption);
            this.messageFlowLayout.Controls.Add(this.lblDescription);
            this.messageFlowLayout.Controls.Add(this.linkPrivacyPolicy);
            this.messageFlowLayout.Controls.Add(this.optOptIn);
            this.messageFlowLayout.Controls.Add(this.optOptOut);
            this.messageFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.messageFlowLayout.Location = new System.Drawing.Point(70, 12);
            this.messageFlowLayout.Margin = new System.Windows.Forms.Padding(12);
            this.messageFlowLayout.Name = "messageFlowLayout";
            this.messageFlowLayout.Size = new System.Drawing.Size(437, 171);
            this.messageFlowLayout.TabIndex = 4;
            this.messageFlowLayout.WrapContents = false;
            this.messageFlowLayout.Resize += new System.EventHandler(this.messageFlowLayout_Resize);
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblCaption.Location = new System.Drawing.Point(0, 0);
            this.lblCaption.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(214, 20);
            this.lblCaption.TabIndex = 2;
            this.lblCaption.Text = "Help Improve {ProductName}";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(0, 26);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(437, 52);
            this.lblDescription.TabIndex = 3;
            this.lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // linkPrivacyPolicy
            // 
            this.linkPrivacyPolicy.ActiveLinkColor = System.Drawing.Color.Black;
            this.linkPrivacyPolicy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.linkPrivacyPolicy.AutoSize = true;
            this.linkPrivacyPolicy.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkPrivacyPolicy.Location = new System.Drawing.Point(0, 93);
            this.linkPrivacyPolicy.Margin = new System.Windows.Forms.Padding(0, 9, 0, 0);
            this.linkPrivacyPolicy.Name = "linkPrivacyPolicy";
            this.linkPrivacyPolicy.Size = new System.Drawing.Size(437, 13);
            this.linkPrivacyPolicy.TabIndex = 9;
            this.linkPrivacyPolicy.TabStop = true;
            this.linkPrivacyPolicy.Text = "Read more about the {CeipName} online";
            this.linkPrivacyPolicy.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkPrivacyPolicy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPrivacyPolicy_LinkClicked);
            // 
            // optOptIn
            // 
            this.optOptIn.AutoSize = true;
            this.optOptIn.Location = new System.Drawing.Point(21, 115);
            this.optOptIn.Margin = new System.Windows.Forms.Padding(21, 9, 0, 3);
            this.optOptIn.Name = "optOptIn";
            this.optOptIn.Size = new System.Drawing.Size(215, 17);
            this.optOptIn.TabIndex = 10;
            this.optOptIn.TabStop = true;
            this.optOptIn.Text = "Yes, I want to participate in the program.";
            this.optOptIn.UseVisualStyleBackColor = true;
            this.optOptIn.CheckedChanged += new System.EventHandler(this.optOptIn_CheckedChanged);
            // 
            // optOptOut
            // 
            this.optOptOut.AutoSize = true;
            this.optOptOut.Location = new System.Drawing.Point(21, 138);
            this.optOptOut.Margin = new System.Windows.Forms.Padding(21, 3, 0, 3);
            this.optOptOut.Name = "optOptOut";
            this.optOptOut.Size = new System.Drawing.Size(237, 17);
            this.optOptOut.TabIndex = 11;
            this.optOptOut.TabStop = true;
            this.optOptOut.Text = "No, I don\'t want to participate in the program.";
            this.optOptOut.UseVisualStyleBackColor = true;
            this.optOptOut.CheckedChanged += new System.EventHandler(this.optOptOut_CheckedChanged);
            // 
            // AutoSendConsentDialog
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(519, 247);
            this.Controls.Add(this.messageFlowLayout);
            this.Controls.Add(this.buttonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoSendConsentDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "{CeipName}";
            this.buttonPanel.ResumeLayout(false);
            this.messageFlowLayout.ResumeLayout(false);
            this.messageFlowLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Label lblButtonDivider;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel messageFlowLayout;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.LinkLabel linkPrivacyPolicy;
        private System.Windows.Forms.RadioButton optOptIn;
        private System.Windows.Forms.RadioButton optOptOut;
    }
}