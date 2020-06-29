namespace Gibraltar.Monitor.Windows.Internal
{
    partial class UIPackagerFinish
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
            this.packageProgress = new System.Windows.Forms.ProgressBar();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.lblStaticMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // packageProgress
            // 
            this.packageProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.packageProgress.Location = new System.Drawing.Point(46, 78);
            this.packageProgress.Name = "packageProgress";
            this.packageProgress.Size = new System.Drawing.Size(281, 20);
            this.packageProgress.TabIndex = 0;
            // 
            // lblStatusMessage
            // 
            this.lblStatusMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusMessage.AutoEllipsis = true;
            this.lblStatusMessage.Location = new System.Drawing.Point(46, 58);
            this.lblStatusMessage.Name = "lblStatusMessage";
            this.lblStatusMessage.Size = new System.Drawing.Size(281, 17);
            this.lblStatusMessage.TabIndex = 1;
            this.lblStatusMessage.Text = "Package Progress Status Message";
            // 
            // lblStaticMessage
            // 
            this.lblStaticMessage.AutoSize = true;
            this.lblStaticMessage.Location = new System.Drawing.Point(9, 7);
            this.lblStaticMessage.Name = "lblStaticMessage";
            this.lblStaticMessage.Size = new System.Drawing.Size(61, 13);
            this.lblStaticMessage.TabIndex = 2;
            this.lblStaticMessage.Text = "Packaging ";
            // 
            // UIPackagerFinish
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblStaticMessage);
            this.Controls.Add(this.lblStatusMessage);
            this.Controls.Add(this.packageProgress);
            this.Name = "UIPackagerFinish";
            this.Size = new System.Drawing.Size(381, 127);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar packageProgress;
        private System.Windows.Forms.Label lblStatusMessage;
        private System.Windows.Forms.Label lblStaticMessage;
    }
}
