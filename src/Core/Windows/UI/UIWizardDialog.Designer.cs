namespace Gibraltar.Windows.UI
{
    public partial class UIWizardDialog
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
            this.headerDisplayPanel = new System.Windows.Forms.Panel();
            this.pageHeaderPicture = new System.Windows.Forms.PictureBox();
            this.lblStepDescription = new System.Windows.Forms.Label();
            this.lblStepCaption = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.wizardDisplayPanel = new System.Windows.Forms.Panel();
            this.lblDivider = new System.Windows.Forms.Label();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.lblDivider2 = new System.Windows.Forms.Label();
            this.tmrAutoClose = new System.Windows.Forms.Timer(this.components);
            this.headTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.headerDisplayPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pageHeaderPicture)).BeginInit();
            this.wizardDisplayPanel.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.headTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerDisplayPanel
            // 
            this.headerDisplayPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.headerDisplayPanel.Controls.Add(this.lblStepDescription);
            this.headerDisplayPanel.Controls.Add(this.lblStepCaption);
            this.headerDisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerDisplayPanel.Location = new System.Drawing.Point(0, 0);
            this.headerDisplayPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headerDisplayPanel.Name = "headerDisplayPanel";
            this.headerDisplayPanel.Padding = new System.Windows.Forms.Padding(10);
            this.headerDisplayPanel.Size = new System.Drawing.Size(430, 75);
            this.headerDisplayPanel.TabIndex = 5;
            // 
            // pageHeaderPicture
            // 
            this.pageHeaderPicture.Location = new System.Drawing.Point(433, 10);
            this.pageHeaderPicture.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.pageHeaderPicture.Name = "pageHeaderPicture";
            this.pageHeaderPicture.Size = new System.Drawing.Size(56, 56);
            this.pageHeaderPicture.TabIndex = 2;
            this.pageHeaderPicture.TabStop = false;
            // 
            // lblStepDescription
            // 
            this.lblStepDescription.Location = new System.Drawing.Point(30, 30);
            this.lblStepDescription.Name = "lblStepDescription";
            this.lblStepDescription.Size = new System.Drawing.Size(393, 30);
            this.lblStepDescription.TabIndex = 1;
            this.lblStepDescription.Text = "[Long Step Description Goes Here]";
            // 
            // lblStepCaption
            // 
            this.lblStepCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStepCaption.Location = new System.Drawing.Point(13, 13);
            this.lblStepCaption.Name = "lblStepCaption";
            this.lblStepCaption.Size = new System.Drawing.Size(410, 17);
            this.lblStepCaption.TabIndex = 0;
            this.lblStepCaption.Text = "[Step Caption Goes Here]";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(418, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 22);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(256, 14);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(70, 22);
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Text = "< &Back";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(332, 14);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(70, 22);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "&Next >";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // wizardDisplayPanel
            // 
            this.wizardDisplayPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.wizardDisplayPanel.Controls.Add(this.lblDivider);
            this.wizardDisplayPanel.Location = new System.Drawing.Point(0, 75);
            this.wizardDisplayPanel.Margin = new System.Windows.Forms.Padding(0);
            this.wizardDisplayPanel.Name = "wizardDisplayPanel";
            this.wizardDisplayPanel.Padding = new System.Windows.Forms.Padding(30, 10, 30, 10);
            this.wizardDisplayPanel.Size = new System.Drawing.Size(500, 257);
            this.wizardDisplayPanel.TabIndex = 3;
            // 
            // lblDivider
            // 
            this.lblDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDivider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblDivider.Location = new System.Drawing.Point(0, 0);
            this.lblDivider.Name = "lblDivider";
            this.lblDivider.Size = new System.Drawing.Size(500, 2);
            this.lblDivider.TabIndex = 0;
            // 
            // footerPanel
            // 
            this.footerPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.footerPanel.Controls.Add(this.lblDivider2);
            this.footerPanel.Controls.Add(this.btnCancel);
            this.footerPanel.Controls.Add(this.btnNext);
            this.footerPanel.Controls.Add(this.btnPrevious);
            this.footerPanel.Location = new System.Drawing.Point(0, 332);
            this.footerPanel.Margin = new System.Windows.Forms.Padding(0);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(500, 50);
            this.footerPanel.TabIndex = 6;
            // 
            // lblDivider2
            // 
            this.lblDivider2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDivider2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblDivider2.Location = new System.Drawing.Point(0, 0);
            this.lblDivider2.Name = "lblDivider2";
            this.lblDivider2.Size = new System.Drawing.Size(500, 2);
            this.lblDivider2.TabIndex = 5;
            // 
            // tmrAutoClose
            // 
            this.tmrAutoClose.Interval = 1200;
            this.tmrAutoClose.Tick += new System.EventHandler(this.tmrAutoClose_Tick);
            // 
            // headTableLayoutPanel
            // 
            this.headTableLayoutPanel.BackColor = System.Drawing.Color.White;
            this.headTableLayoutPanel.ColumnCount = 2;
            this.headTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 86.37274F));
            this.headTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.62725F));
            this.headTableLayoutPanel.Controls.Add(this.headerDisplayPanel, 0, 0);
            this.headTableLayoutPanel.Controls.Add(this.pageHeaderPicture, 1, 0);
            this.headTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.headTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headTableLayoutPanel.Name = "headTableLayoutPanel";
            this.headTableLayoutPanel.RowCount = 1;
            this.headTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.headTableLayoutPanel.Size = new System.Drawing.Size(499, 75);
            this.headTableLayoutPanel.TabIndex = 1;
            // 
            // UIWizardDialog
            // 
            this.AcceptButton = this.btnNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(500, 382);
            this.Controls.Add(this.headTableLayoutPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.wizardDisplayPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UIWizardDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Standard Wizard Dialog";
            this.TopMost = true;
            this.headerDisplayPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pageHeaderPicture)).EndInit();
            this.wizardDisplayPanel.ResumeLayout(false);
            this.footerPanel.ResumeLayout(false);
            this.headTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Panel wizardDisplayPanel;
        private System.Windows.Forms.Panel headerDisplayPanel;
        private System.Windows.Forms.Label lblStepDescription;
        private System.Windows.Forms.Label lblStepCaption;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel footerPanel;
        private System.Windows.Forms.Label lblDivider;
        private System.Windows.Forms.Label lblDivider2;
        private System.Windows.Forms.Timer tmrAutoClose;

        /// <summary>
        /// Using visual inheritance you can set a wizard specific picture.
        /// </summary>
        protected System.Windows.Forms.PictureBox pageHeaderPicture;
        private System.Windows.Forms.TableLayoutPanel headTableLayoutPanel;
    }
}