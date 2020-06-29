namespace Gibraltar.Monitor.Windows
{
    partial class UILogMessageDetail
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UILogMessageDetail));
            this.lblNoDetails = new System.Windows.Forms.Label();
            this.imagesCommandManager = new System.Windows.Forms.ImageList(this.components);
            this.detailsView = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblNoDetails
            // 
            this.lblNoDetails.BackColor = System.Drawing.Color.Transparent;
            this.lblNoDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoDetails.Location = new System.Drawing.Point(0, 0);
            this.lblNoDetails.Name = "lblNoDetails";
            this.lblNoDetails.Size = new System.Drawing.Size(372, 210);
            this.lblNoDetails.TabIndex = 2;
            this.lblNoDetails.Text = "No Details Available";
            this.lblNoDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imagesCommandManager
            // 
            this.imagesCommandManager.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesCommandManager.ImageStream")));
            this.imagesCommandManager.TransparentColor = System.Drawing.Color.Magenta;
            this.imagesCommandManager.Images.SetKeyName(0, "copy");
            this.imagesCommandManager.Images.SetKeyName(1, "save");
            // 
            // detailsView
            // 
            this.detailsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsView.Location = new System.Drawing.Point(0, 0);
            this.detailsView.Multiline = true;
            this.detailsView.Name = "detailsView";
            this.detailsView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.detailsView.Size = new System.Drawing.Size(372, 210);
            this.detailsView.TabIndex = 3;
            // 
            // UILogMessageDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.detailsView);
            this.Controls.Add(this.lblNoDetails);
            this.Name = "UILogMessageDetail";
            this.Size = new System.Drawing.Size(372, 210);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNoDetails;
        private System.Windows.Forms.ImageList imagesCommandManager;
        private System.Windows.Forms.TextBox detailsView;
    }
}
