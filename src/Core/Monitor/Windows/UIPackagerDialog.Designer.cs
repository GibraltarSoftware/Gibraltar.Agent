using System.Windows.Forms;

namespace Gibraltar.Monitor.Windows
{
    partial class UIPackagerDialog
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
            ((System.ComponentModel.ISupportInitialize)(this.pageHeaderPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // pageHeaderPicture
            // 
            this.pageHeaderPicture.Image = global::Gibraltar.Monitor.Windows.UIResources.packaging_wizard_corner;
            this.pageHeaderPicture.Location = new System.Drawing.Point(425, 0);
            this.pageHeaderPicture.Size = new System.Drawing.Size(75, 75);
            this.pageHeaderPicture.Dock = DockStyle.Fill;
            this.pageHeaderPicture.Margin = new Padding(0);
            // 
            // UIPackagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(500, 382);
            this.Name = "UIPackagerDialog";
            ((System.ComponentModel.ISupportInitialize)(this.pageHeaderPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
