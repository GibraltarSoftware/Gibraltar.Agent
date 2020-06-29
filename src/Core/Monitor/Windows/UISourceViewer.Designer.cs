namespace Gibraltar.Monitor.Windows
{
#pragma warning disable 1591
    partial class UISourceViewer
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
            this.sourcePreview = new System.Windows.Forms.WebBrowser();
            this.lblNoSource = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sourcePreview
            // 
            this.sourcePreview.AllowWebBrowserDrop = false;
            this.sourcePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcePreview.IsWebBrowserContextMenuEnabled = false;
            this.sourcePreview.Location = new System.Drawing.Point(0, 0);
            this.sourcePreview.MinimumSize = new System.Drawing.Size(20, 20);
            this.sourcePreview.Name = "sourcePreview";
            this.sourcePreview.ScriptErrorsSuppressed = true;
            this.sourcePreview.Size = new System.Drawing.Size(560, 342);
            this.sourcePreview.TabIndex = 0;
            this.sourcePreview.WebBrowserShortcutsEnabled = false;
            this.sourcePreview.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.sourcePreview_DocumentCompleted);
            // 
            // lblNoSource
            // 
            this.lblNoSource.BackColor = System.Drawing.Color.Transparent;
            this.lblNoSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoSource.Location = new System.Drawing.Point(0, 0);
            this.lblNoSource.Name = "lblNoSource";
            this.lblNoSource.Size = new System.Drawing.Size(560, 342);
            this.lblNoSource.TabIndex = 1;
            this.lblNoSource.Text = "No Source Code Available";
            this.lblNoSource.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UISourceViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblNoSource);
            this.Controls.Add(this.sourcePreview);
            this.Name = "UISourceViewer";
            this.Size = new System.Drawing.Size(560, 342);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser sourcePreview;
        private System.Windows.Forms.Label lblNoSource;
    }
}
