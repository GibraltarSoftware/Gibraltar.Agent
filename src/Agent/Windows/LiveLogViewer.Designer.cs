namespace Gibraltar.Agent.Windows
{
    partial class LiveLogViewer
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
            this.internalLogViewer = new Gibraltar.Monitor.Windows.LiveLogViewer();
            this.SuspendLayout();
            // 
            // internalLogViewer
            // 
            this.internalLogViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.internalLogViewer.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.internalLogViewer.Location = new System.Drawing.Point(0, 0);
            this.internalLogViewer.Name = "internalLogViewer";
            this.internalLogViewer.RunButtonTextVisible = false;
            this.internalLogViewer.ShowToolBar = true;
            this.internalLogViewer.Size = new System.Drawing.Size(726, 392);
            this.internalLogViewer.TabIndex = 0;
            // 
            // LiveLogViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.internalLogViewer);
            this.Name = "LiveLogViewer";
            this.Size = new System.Drawing.Size(726, 392);
            this.ResumeLayout(false);

        }

        #endregion

        private Gibraltar.Monitor.Windows.LiveLogViewer internalLogViewer;
    }
}
