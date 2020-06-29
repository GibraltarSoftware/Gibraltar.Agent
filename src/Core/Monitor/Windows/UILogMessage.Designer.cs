namespace Gibraltar.Monitor.Windows
{
    partial class UILogMessage
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowDescriptionPreformatted = new System.Windows.Forms.CheckBox();
            this.descriptionPanel = new System.Windows.Forms.Panel();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.callInfoPanel = new System.Windows.Forms.Panel();
            this.txtCategoryName = new System.Windows.Forms.TextBox();
            this.lblCategoryName = new System.Windows.Forms.Label();
            this.txtMethodName = new System.Windows.Forms.TextBox();
            this.txtClassName = new System.Windows.Forms.TextBox();
            this.lblMethodName = new System.Windows.Forms.Label();
            this.lblClassName = new System.Windows.Forms.Label();
            this.threadPanel = new System.Windows.Forms.Panel();
            this.txtThreadType = new System.Windows.Forms.TextBox();
            this.txtThreadId = new System.Windows.Forms.TextBox();
            this.txtThreadName = new System.Windows.Forms.TextBox();
            this.lblThreadType = new System.Windows.Forms.Label();
            this.lblThreadName = new System.Windows.Forms.Label();
            this.lblThreadId = new System.Windows.Forms.Label();
            this.locationPanel = new System.Windows.Forms.Panel();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.lblLocation = new System.Windows.Forms.Label();
            this.mainLayoutPanel.SuspendLayout();
            this.descriptionPanel.SuspendLayout();
            this.callInfoPanel.SuspendLayout();
            this.threadPanel.SuspendLayout();
            this.locationPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.mainLayoutPanel.Controls.Add(this.chkShowDescriptionPreformatted, 0, 3);
            this.mainLayoutPanel.Controls.Add(this.descriptionPanel, 0, 2);
            this.mainLayoutPanel.Controls.Add(this.callInfoPanel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.threadPanel, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.locationPanel, 0, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 4;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(742, 305);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // chkShowDescriptionPreformatted
            // 
            this.chkShowDescriptionPreformatted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowDescriptionPreformatted.AutoSize = true;
            this.mainLayoutPanel.SetColumnSpan(this.chkShowDescriptionPreformatted, 2);
            this.chkShowDescriptionPreformatted.Location = new System.Drawing.Point(3, 285);
            this.chkShowDescriptionPreformatted.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.chkShowDescriptionPreformatted.Name = "chkShowDescriptionPreformatted";
            this.chkShowDescriptionPreformatted.Size = new System.Drawing.Size(287, 17);
            this.chkShowDescriptionPreformatted.TabIndex = 5;
            this.chkShowDescriptionPreformatted.Text = "Show Description as Preformatted, fixed width text";
            this.chkShowDescriptionPreformatted.UseVisualStyleBackColor = true;
            this.chkShowDescriptionPreformatted.CheckedChanged += new System.EventHandler(this.chkShowDescriptionPreformatted_CheckedChanged);
            // 
            // descriptionPanel
            // 
            this.mainLayoutPanel.SetColumnSpan(this.descriptionPanel, 2);
            this.descriptionPanel.Controls.Add(this.txtDescription);
            this.descriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionPanel.Location = new System.Drawing.Point(0, 68);
            this.descriptionPanel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.descriptionPanel.Name = "descriptionPanel";
            this.descriptionPanel.Size = new System.Drawing.Size(742, 214);
            this.descriptionPanel.TabIndex = 3;
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.SystemColors.Window;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(0, 0);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(0);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDescription.Size = new System.Drawing.Size(742, 214);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Text = "<Description>";
            // 
            // callInfoPanel
            // 
            this.callInfoPanel.Controls.Add(this.txtCategoryName);
            this.callInfoPanel.Controls.Add(this.lblCategoryName);
            this.callInfoPanel.Controls.Add(this.txtMethodName);
            this.callInfoPanel.Controls.Add(this.txtClassName);
            this.callInfoPanel.Controls.Add(this.lblMethodName);
            this.callInfoPanel.Controls.Add(this.lblClassName);
            this.callInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.callInfoPanel.Location = new System.Drawing.Point(0, 0);
            this.callInfoPanel.Margin = new System.Windows.Forms.Padding(0);
            this.callInfoPanel.Name = "callInfoPanel";
            this.callInfoPanel.Size = new System.Drawing.Size(519, 50);
            this.callInfoPanel.TabIndex = 1;
            // 
            // txtCategoryName
            // 
            this.txtCategoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCategoryName.BackColor = System.Drawing.SystemColors.Window;
            this.txtCategoryName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCategoryName.Location = new System.Drawing.Point(92, 3);
            this.txtCategoryName.Name = "txtCategoryName";
            this.txtCategoryName.ReadOnly = true;
            this.txtCategoryName.Size = new System.Drawing.Size(424, 15);
            this.txtCategoryName.TabIndex = 6;
            this.txtCategoryName.Text = "<Category Name>";
            // 
            // lblCategoryName
            // 
            this.lblCategoryName.AutoSize = true;
            this.lblCategoryName.Location = new System.Drawing.Point(3, 3);
            this.lblCategoryName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.lblCategoryName.Name = "lblCategoryName";
            this.lblCategoryName.Size = new System.Drawing.Size(56, 13);
            this.lblCategoryName.TabIndex = 5;
            this.lblCategoryName.Text = "Category:";
            // 
            // txtMethodName
            // 
            this.txtMethodName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMethodName.BackColor = System.Drawing.SystemColors.Window;
            this.txtMethodName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMethodName.Location = new System.Drawing.Point(92, 35);
            this.txtMethodName.Name = "txtMethodName";
            this.txtMethodName.ReadOnly = true;
            this.txtMethodName.Size = new System.Drawing.Size(424, 15);
            this.txtMethodName.TabIndex = 4;
            this.txtMethodName.Text = "<Method Name>";
            // 
            // txtClassName
            // 
            this.txtClassName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtClassName.BackColor = System.Drawing.SystemColors.Window;
            this.txtClassName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtClassName.Location = new System.Drawing.Point(92, 19);
            this.txtClassName.Name = "txtClassName";
            this.txtClassName.ReadOnly = true;
            this.txtClassName.Size = new System.Drawing.Size(424, 15);
            this.txtClassName.TabIndex = 3;
            this.txtClassName.Text = "<Class Name>";
            // 
            // lblMethodName
            // 
            this.lblMethodName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMethodName.AutoSize = true;
            this.lblMethodName.Location = new System.Drawing.Point(3, 35);
            this.lblMethodName.Name = "lblMethodName";
            this.lblMethodName.Size = new System.Drawing.Size(83, 13);
            this.lblMethodName.TabIndex = 1;
            this.lblMethodName.Text = "Method Name:";
            // 
            // lblClassName
            // 
            this.lblClassName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblClassName.AutoSize = true;
            this.lblClassName.Location = new System.Drawing.Point(3, 19);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(68, 13);
            this.lblClassName.TabIndex = 0;
            this.lblClassName.Text = "Class Name:";
            // 
            // threadPanel
            // 
            this.threadPanel.Controls.Add(this.txtThreadType);
            this.threadPanel.Controls.Add(this.txtThreadId);
            this.threadPanel.Controls.Add(this.txtThreadName);
            this.threadPanel.Controls.Add(this.lblThreadType);
            this.threadPanel.Controls.Add(this.lblThreadName);
            this.threadPanel.Controls.Add(this.lblThreadId);
            this.threadPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadPanel.Location = new System.Drawing.Point(519, 0);
            this.threadPanel.Margin = new System.Windows.Forms.Padding(0);
            this.threadPanel.Name = "threadPanel";
            this.threadPanel.Size = new System.Drawing.Size(223, 50);
            this.threadPanel.TabIndex = 2;
            // 
            // txtThreadType
            // 
            this.txtThreadType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThreadType.BackColor = System.Drawing.SystemColors.Window;
            this.txtThreadType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtThreadType.Location = new System.Drawing.Point(78, 35);
            this.txtThreadType.Name = "txtThreadType";
            this.txtThreadType.ReadOnly = true;
            this.txtThreadType.Size = new System.Drawing.Size(142, 15);
            this.txtThreadType.TabIndex = 6;
            this.txtThreadType.Text = "<Thread Type>";
            // 
            // txtThreadId
            // 
            this.txtThreadId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThreadId.BackColor = System.Drawing.SystemColors.Window;
            this.txtThreadId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtThreadId.Location = new System.Drawing.Point(78, 3);
            this.txtThreadId.Name = "txtThreadId";
            this.txtThreadId.ReadOnly = true;
            this.txtThreadId.Size = new System.Drawing.Size(142, 15);
            this.txtThreadId.TabIndex = 5;
            this.txtThreadId.Text = "<Thread Id>";
            // 
            // txtThreadName
            // 
            this.txtThreadName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThreadName.BackColor = System.Drawing.SystemColors.Window;
            this.txtThreadName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtThreadName.Location = new System.Drawing.Point(78, 19);
            this.txtThreadName.Name = "txtThreadName";
            this.txtThreadName.ReadOnly = true;
            this.txtThreadName.Size = new System.Drawing.Size(142, 15);
            this.txtThreadName.TabIndex = 4;
            this.txtThreadName.Text = "<Thread Name>";
            // 
            // lblThreadType
            // 
            this.lblThreadType.AutoSize = true;
            this.lblThreadType.Location = new System.Drawing.Point(0, 35);
            this.lblThreadType.Name = "lblThreadType";
            this.lblThreadType.Size = new System.Drawing.Size(71, 13);
            this.lblThreadType.TabIndex = 3;
            this.lblThreadType.Text = "Thread Type:";
            // 
            // lblThreadName
            // 
            this.lblThreadName.AutoSize = true;
            this.lblThreadName.Location = new System.Drawing.Point(0, 19);
            this.lblThreadName.Name = "lblThreadName";
            this.lblThreadName.Size = new System.Drawing.Size(77, 13);
            this.lblThreadName.TabIndex = 2;
            this.lblThreadName.Text = "Thread Name:";
            // 
            // lblThreadId
            // 
            this.lblThreadId.AutoSize = true;
            this.lblThreadId.Location = new System.Drawing.Point(0, 3);
            this.lblThreadId.Name = "lblThreadId";
            this.lblThreadId.Size = new System.Drawing.Size(58, 13);
            this.lblThreadId.TabIndex = 1;
            this.lblThreadId.Text = "Thread Id:";
            // 
            // locationPanel
            // 
            this.mainLayoutPanel.SetColumnSpan(this.locationPanel, 2);
            this.locationPanel.Controls.Add(this.txtLocation);
            this.locationPanel.Controls.Add(this.lblLocation);
            this.locationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationPanel.Location = new System.Drawing.Point(3, 50);
            this.locationPanel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.locationPanel.Name = "locationPanel";
            this.locationPanel.Size = new System.Drawing.Size(736, 15);
            this.locationPanel.TabIndex = 4;
            // 
            // txtLocation
            // 
            this.txtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLocation.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLocation.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtLocation.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.txtLocation.Location = new System.Drawing.Point(89, 0);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.ReadOnly = true;
            this.txtLocation.Size = new System.Drawing.Size(641, 15);
            this.txtLocation.TabIndex = 5;
            this.txtLocation.Text = "<Location>";
            this.txtLocation.Click += new System.EventHandler(this.txtLocation_Click);
            // 
            // lblLocation
            // 
            this.lblLocation.AutoSize = true;
            this.lblLocation.Location = new System.Drawing.Point(0, 0);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(54, 13);
            this.lblLocation.TabIndex = 2;
            this.lblLocation.Text = "Location:";
            // 
            // UILogMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.mainLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UILogMessage";
            this.Size = new System.Drawing.Size(742, 305);
            this.Resize += new System.EventHandler(this.UILogMessage_Resize);
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.descriptionPanel.ResumeLayout(false);
            this.descriptionPanel.PerformLayout();
            this.callInfoPanel.ResumeLayout(false);
            this.callInfoPanel.PerformLayout();
            this.threadPanel.ResumeLayout(false);
            this.threadPanel.PerformLayout();
            this.locationPanel.ResumeLayout(false);
            this.locationPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.Panel callInfoPanel;
        private System.Windows.Forms.TextBox txtClassName;
        private System.Windows.Forms.Label lblLocation;
        private System.Windows.Forms.Label lblMethodName;
        private System.Windows.Forms.Label lblClassName;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.TextBox txtMethodName;
        private System.Windows.Forms.Panel threadPanel;
        private System.Windows.Forms.Label lblThreadName;
        private System.Windows.Forms.Label lblThreadId;
        private System.Windows.Forms.TextBox txtThreadType;
        private System.Windows.Forms.TextBox txtThreadId;
        private System.Windows.Forms.TextBox txtThreadName;
        private System.Windows.Forms.Label lblThreadType;
        private System.Windows.Forms.Panel descriptionPanel;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Panel locationPanel;
        private System.Windows.Forms.TextBox txtCategoryName;
        private System.Windows.Forms.Label lblCategoryName;
        private System.Windows.Forms.CheckBox chkShowDescriptionPreformatted;
    }
}
