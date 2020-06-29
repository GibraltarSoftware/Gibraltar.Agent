namespace Gibraltar.Monitor.Windows.Internal
{
    partial class UIPackagerTransport
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
            this.components = new System.ComponentModel.Container();
            this.optSendToEmail = new System.Windows.Forms.RadioButton();
            this.optSaveToFile = new System.Windows.Forms.RadioButton();
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtFileNamePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblDestinationEmailAddress = new System.Windows.Forms.Label();
            this.txtEmailAddress = new System.Windows.Forms.TextBox();
            this.optSaveToRemovableMedia = new System.Windows.Forms.RadioButton();
            this.lblDrive = new System.Windows.Forms.Label();
            this.cboVolumeSelect = new System.Windows.Forms.ComboBox();
            this.mainLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSendToEmail = new System.Windows.Forms.Label();
            this.emailOptionsPanel = new System.Windows.Forms.Panel();
            this.fromEmailOptionsPanel = new System.Windows.Forms.Panel();
            this.lblFromEmailAddressInstructions = new System.Windows.Forms.Label();
            this.txtFromEmailAddress = new System.Windows.Forms.TextBox();
            this.lblFromEmailAddress = new System.Windows.Forms.Label();
            this.lblSaveToFile = new System.Windows.Forms.Label();
            this.fileOptionsPanel = new System.Windows.Forms.Panel();
            this.lblSaveToRemovableMedia = new System.Windows.Forms.Label();
            this.removableMediaOptionsPanel = new System.Windows.Forms.Panel();
            this.lblRemovableMediaNotValid = new System.Windows.Forms.Label();
            this.lblNoRemovableMedia = new System.Windows.Forms.Label();
            this.btnRefreshRemovableMedia = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mainLayoutPanel.SuspendLayout();
            this.emailOptionsPanel.SuspendLayout();
            this.fromEmailOptionsPanel.SuspendLayout();
            this.fileOptionsPanel.SuspendLayout();
            this.removableMediaOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // optSendToEmail
            // 
            this.optSendToEmail.AutoSize = true;
            this.optSendToEmail.BackColor = System.Drawing.Color.Transparent;
            this.optSendToEmail.Location = new System.Drawing.Point(3, 16);
            this.optSendToEmail.Name = "optSendToEmail";
            this.optSendToEmail.Size = new System.Drawing.Size(95, 17);
            this.optSendToEmail.TabIndex = 0;
            this.optSendToEmail.TabStop = true;
            this.optSendToEmail.Text = "Send via Email";
            this.optSendToEmail.UseVisualStyleBackColor = false;
            this.optSendToEmail.Click += new System.EventHandler(this.Option_Click);
            // 
            // optSaveToFile
            // 
            this.optSaveToFile.AutoSize = true;
            this.optSaveToFile.BackColor = System.Drawing.Color.Transparent;
            this.optSaveToFile.Location = new System.Drawing.Point(3, 134);
            this.optSaveToFile.Name = "optSaveToFile";
            this.optSaveToFile.Size = new System.Drawing.Size(81, 17);
            this.optSaveToFile.TabIndex = 1;
            this.optSaveToFile.TabStop = true;
            this.optSaveToFile.Text = "Save to File";
            this.optSaveToFile.UseVisualStyleBackColor = false;
            this.optSaveToFile.Click += new System.EventHandler(this.Option_Click);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.BackColor = System.Drawing.Color.Transparent;
            this.lblFileName.Location = new System.Drawing.Point(28, 4);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(57, 13);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "File Name:";
            // 
            // txtFileNamePath
            // 
            this.txtFileNamePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileNamePath.Location = new System.Drawing.Point(91, 0);
            this.txtFileNamePath.Name = "txtFileNamePath";
            this.txtFileNamePath.Size = new System.Drawing.Size(267, 20);
            this.txtFileNamePath.TabIndex = 3;
            this.toolTip.SetToolTip(this.txtFileNamePath, "The full file name and path to write the session information to");
            this.txtFileNamePath.TextChanged += new System.EventHandler(this.txtFileNamePath_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(364, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(28, 20);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "...";
            this.toolTip.SetToolTip(this.btnBrowse, "Browse your computer to select where to store the package");
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblDestinationEmailAddress
            // 
            this.lblDestinationEmailAddress.AutoSize = true;
            this.lblDestinationEmailAddress.BackColor = System.Drawing.Color.Transparent;
            this.lblDestinationEmailAddress.Location = new System.Drawing.Point(28, 3);
            this.lblDestinationEmailAddress.Name = "lblDestinationEmailAddress";
            this.lblDestinationEmailAddress.Size = new System.Drawing.Size(51, 13);
            this.lblDestinationEmailAddress.TabIndex = 5;
            this.lblDestinationEmailAddress.Text = "Send To:";
            // 
            // txtEmailAddress
            // 
            this.txtEmailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmailAddress.Location = new System.Drawing.Point(91, 0);
            this.txtEmailAddress.Name = "txtEmailAddress";
            this.txtEmailAddress.Size = new System.Drawing.Size(267, 20);
            this.txtEmailAddress.TabIndex = 6;
            this.toolTip.SetToolTip(this.txtEmailAddress, "Internet email address to send the session information to");
            this.txtEmailAddress.TextChanged += new System.EventHandler(this.txtEmailAddress_TextChanged);
            // 
            // optSaveToRemovableMedia
            // 
            this.optSaveToRemovableMedia.AutoSize = true;
            this.optSaveToRemovableMedia.BackColor = System.Drawing.Color.Transparent;
            this.optSaveToRemovableMedia.Location = new System.Drawing.Point(3, 213);
            this.optSaveToRemovableMedia.Name = "optSaveToRemovableMedia";
            this.optSaveToRemovableMedia.Size = new System.Drawing.Size(151, 17);
            this.optSaveToRemovableMedia.TabIndex = 7;
            this.optSaveToRemovableMedia.TabStop = true;
            this.optSaveToRemovableMedia.Text = "Save to Removable Media";
            this.optSaveToRemovableMedia.UseVisualStyleBackColor = false;
            this.optSaveToRemovableMedia.Click += new System.EventHandler(this.Option_Click);
            // 
            // lblDrive
            // 
            this.lblDrive.AutoSize = true;
            this.lblDrive.BackColor = System.Drawing.Color.Transparent;
            this.lblDrive.Location = new System.Drawing.Point(28, 6);
            this.lblDrive.Name = "lblDrive";
            this.lblDrive.Size = new System.Drawing.Size(39, 13);
            this.lblDrive.TabIndex = 8;
            this.lblDrive.Text = "Media:";
            // 
            // cboVolumeSelect
            // 
            this.cboVolumeSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboVolumeSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVolumeSelect.FormattingEnabled = true;
            this.cboVolumeSelect.Location = new System.Drawing.Point(91, 3);
            this.cboVolumeSelect.Name = "cboVolumeSelect";
            this.cboVolumeSelect.Size = new System.Drawing.Size(267, 21);
            this.cboVolumeSelect.TabIndex = 9;
            this.toolTip.SetToolTip(this.cboVolumeSelect, "Select the removable media to save the package to.");
            this.cboVolumeSelect.SelectedValueChanged += new System.EventHandler(this.cboVolumeSelect_SelectedValueChanged);
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.Controls.Add(this.lblSendToEmail);
            this.mainLayoutPanel.Controls.Add(this.optSendToEmail);
            this.mainLayoutPanel.Controls.Add(this.emailOptionsPanel);
            this.mainLayoutPanel.Controls.Add(this.lblSaveToFile);
            this.mainLayoutPanel.Controls.Add(this.optSaveToFile);
            this.mainLayoutPanel.Controls.Add(this.fileOptionsPanel);
            this.mainLayoutPanel.Controls.Add(this.lblSaveToRemovableMedia);
            this.mainLayoutPanel.Controls.Add(this.optSaveToRemovableMedia);
            this.mainLayoutPanel.Controls.Add(this.removableMediaOptionsPanel);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.Size = new System.Drawing.Size(451, 262);
            this.mainLayoutPanel.TabIndex = 10;
            this.mainLayoutPanel.Resize += new System.EventHandler(this.mainLayoutPanel_Resize);
            // 
            // lblSendToEmail
            // 
            this.lblSendToEmail.AutoSize = true;
            this.lblSendToEmail.Location = new System.Drawing.Point(3, 0);
            this.lblSendToEmail.Name = "lblSendToEmail";
            this.lblSendToEmail.Size = new System.Drawing.Size(77, 13);
            this.lblSendToEmail.TabIndex = 12;
            this.lblSendToEmail.Text = "Send via Email";
            // 
            // emailOptionsPanel
            // 
            this.emailOptionsPanel.Controls.Add(this.fromEmailOptionsPanel);
            this.emailOptionsPanel.Controls.Add(this.txtEmailAddress);
            this.emailOptionsPanel.Controls.Add(this.lblDestinationEmailAddress);
            this.emailOptionsPanel.Location = new System.Drawing.Point(3, 39);
            this.emailOptionsPanel.Name = "emailOptionsPanel";
            this.emailOptionsPanel.Size = new System.Drawing.Size(398, 76);
            this.emailOptionsPanel.TabIndex = 11;
            // 
            // fromEmailOptionsPanel
            // 
            this.fromEmailOptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fromEmailOptionsPanel.Controls.Add(this.lblFromEmailAddressInstructions);
            this.fromEmailOptionsPanel.Controls.Add(this.txtFromEmailAddress);
            this.fromEmailOptionsPanel.Controls.Add(this.lblFromEmailAddress);
            this.fromEmailOptionsPanel.Location = new System.Drawing.Point(0, 23);
            this.fromEmailOptionsPanel.Name = "fromEmailOptionsPanel";
            this.fromEmailOptionsPanel.Size = new System.Drawing.Size(398, 53);
            this.fromEmailOptionsPanel.TabIndex = 10;
            // 
            // lblFromEmailAddressInstructions
            // 
            this.lblFromEmailAddressInstructions.AutoSize = true;
            this.lblFromEmailAddressInstructions.BackColor = System.Drawing.Color.Transparent;
            this.lblFromEmailAddressInstructions.Location = new System.Drawing.Point(15, 7);
            this.lblFromEmailAddressInstructions.Name = "lblFromEmailAddressInstructions";
            this.lblFromEmailAddressInstructions.Size = new System.Drawing.Size(308, 13);
            this.lblFromEmailAddressInstructions.TabIndex = 9;
            this.lblFromEmailAddressInstructions.Text = "Address to direct responses to: (Optional.  Blank for anonymous)";
            // 
            // txtFromEmailAddress
            // 
            this.txtFromEmailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFromEmailAddress.Location = new System.Drawing.Point(91, 26);
            this.txtFromEmailAddress.Name = "txtFromEmailAddress";
            this.txtFromEmailAddress.Size = new System.Drawing.Size(267, 20);
            this.txtFromEmailAddress.TabIndex = 8;
            this.toolTip.SetToolTip(this.txtFromEmailAddress, "Optional.  Internet email address to use as the reply-to address.");
            // 
            // lblFromEmailAddress
            // 
            this.lblFromEmailAddress.AutoSize = true;
            this.lblFromEmailAddress.BackColor = System.Drawing.Color.Transparent;
            this.lblFromEmailAddress.Location = new System.Drawing.Point(28, 29);
            this.lblFromEmailAddress.Name = "lblFromEmailAddress";
            this.lblFromEmailAddress.Size = new System.Drawing.Size(53, 13);
            this.lblFromEmailAddress.TabIndex = 7;
            this.lblFromEmailAddress.Text = "Reply To:";
            // 
            // lblSaveToFile
            // 
            this.lblSaveToFile.AutoSize = true;
            this.lblSaveToFile.Location = new System.Drawing.Point(3, 118);
            this.lblSaveToFile.Name = "lblSaveToFile";
            this.lblSaveToFile.Size = new System.Drawing.Size(63, 13);
            this.lblSaveToFile.TabIndex = 13;
            this.lblSaveToFile.Text = "Save to File";
            // 
            // fileOptionsPanel
            // 
            this.fileOptionsPanel.Controls.Add(this.txtFileNamePath);
            this.fileOptionsPanel.Controls.Add(this.lblFileName);
            this.fileOptionsPanel.Controls.Add(this.btnBrowse);
            this.fileOptionsPanel.Location = new System.Drawing.Point(3, 157);
            this.fileOptionsPanel.Name = "fileOptionsPanel";
            this.fileOptionsPanel.Size = new System.Drawing.Size(395, 37);
            this.fileOptionsPanel.TabIndex = 11;
            // 
            // lblSaveToRemovableMedia
            // 
            this.lblSaveToRemovableMedia.AutoSize = true;
            this.lblSaveToRemovableMedia.Location = new System.Drawing.Point(3, 197);
            this.lblSaveToRemovableMedia.Name = "lblSaveToRemovableMedia";
            this.lblSaveToRemovableMedia.Size = new System.Drawing.Size(137, 13);
            this.lblSaveToRemovableMedia.TabIndex = 14;
            this.lblSaveToRemovableMedia.Text = "Save To Removable Media";
            // 
            // removableMediaOptionsPanel
            // 
            this.removableMediaOptionsPanel.Controls.Add(this.lblRemovableMediaNotValid);
            this.removableMediaOptionsPanel.Controls.Add(this.lblNoRemovableMedia);
            this.removableMediaOptionsPanel.Controls.Add(this.btnRefreshRemovableMedia);
            this.removableMediaOptionsPanel.Controls.Add(this.cboVolumeSelect);
            this.removableMediaOptionsPanel.Controls.Add(this.lblDrive);
            this.removableMediaOptionsPanel.Location = new System.Drawing.Point(407, 3);
            this.removableMediaOptionsPanel.Name = "removableMediaOptionsPanel";
            this.removableMediaOptionsPanel.Size = new System.Drawing.Size(395, 60);
            this.removableMediaOptionsPanel.TabIndex = 11;
            // 
            // lblRemovableMediaNotValid
            // 
            this.lblRemovableMediaNotValid.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblRemovableMediaNotValid.Location = new System.Drawing.Point(88, 30);
            this.lblRemovableMediaNotValid.Name = "lblRemovableMediaNotValid";
            this.lblRemovableMediaNotValid.Size = new System.Drawing.Size(286, 29);
            this.lblRemovableMediaNotValid.TabIndex = 11;
            this.lblRemovableMediaNotValid.Text = "Unable to write to the selected media.  Make sure there is media in the drive and" +
    " it isn\'t write protected.";
            this.lblRemovableMediaNotValid.Visible = false;
            // 
            // lblNoRemovableMedia
            // 
            this.lblNoRemovableMedia.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblNoRemovableMedia.Location = new System.Drawing.Point(31, 59);
            this.lblNoRemovableMedia.Name = "lblNoRemovableMedia";
            this.lblNoRemovableMedia.Size = new System.Drawing.Size(361, 29);
            this.lblNoRemovableMedia.TabIndex = 10;
            this.lblNoRemovableMedia.Text = "No removable media were detected.  Insert a USB memory strick or other removable " +
    "media to use this option.";
            // 
            // btnRefreshRemovableMedia
            // 
            this.btnRefreshRemovableMedia.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshRemovableMedia.Image = global::Gibraltar.Monitor.Windows.UIResources.RepeatHS;
            this.btnRefreshRemovableMedia.Location = new System.Drawing.Point(364, 3);
            this.btnRefreshRemovableMedia.Name = "btnRefreshRemovableMedia";
            this.btnRefreshRemovableMedia.Size = new System.Drawing.Size(28, 21);
            this.btnRefreshRemovableMedia.TabIndex = 11;
            this.toolTip.SetToolTip(this.btnRefreshRemovableMedia, "Refresh the list of removable media");
            this.btnRefreshRemovableMedia.UseVisualStyleBackColor = true;
            this.btnRefreshRemovableMedia.Click += new System.EventHandler(this.btnRefreshRemovableMedia_Click);
            // 
            // UIPackagerTransport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayoutPanel);
            this.Name = "UIPackagerTransport";
            this.Size = new System.Drawing.Size(451, 262);
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.emailOptionsPanel.ResumeLayout(false);
            this.emailOptionsPanel.PerformLayout();
            this.fromEmailOptionsPanel.ResumeLayout(false);
            this.fromEmailOptionsPanel.PerformLayout();
            this.fileOptionsPanel.ResumeLayout(false);
            this.fileOptionsPanel.PerformLayout();
            this.removableMediaOptionsPanel.ResumeLayout(false);
            this.removableMediaOptionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton optSendToEmail;
        private System.Windows.Forms.RadioButton optSaveToFile;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtFileNamePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblDestinationEmailAddress;
        private System.Windows.Forms.TextBox txtEmailAddress;
        private System.Windows.Forms.RadioButton optSaveToRemovableMedia;
        private System.Windows.Forms.Label lblDrive;
        private System.Windows.Forms.ComboBox cboVolumeSelect;
        private System.Windows.Forms.FlowLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.Panel emailOptionsPanel;
        private System.Windows.Forms.Panel fileOptionsPanel;
        private System.Windows.Forms.Panel removableMediaOptionsPanel;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label lblFromEmailAddressInstructions;
        private System.Windows.Forms.TextBox txtFromEmailAddress;
        private System.Windows.Forms.Label lblFromEmailAddress;
        private System.Windows.Forms.Panel fromEmailOptionsPanel;
        private System.Windows.Forms.Label lblNoRemovableMedia;
        private System.Windows.Forms.Button btnRefreshRemovableMedia;
        private System.Windows.Forms.Label lblRemovableMediaNotValid;
        private System.Windows.Forms.Label lblSendToEmail;
        private System.Windows.Forms.Label lblSaveToFile;
        private System.Windows.Forms.Label lblSaveToRemovableMedia;
    }
}
