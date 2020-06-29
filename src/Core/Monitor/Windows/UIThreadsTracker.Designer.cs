namespace Gibraltar.Monitor.Windows
{
    partial class UIThreadsTracker
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
            this.flowThreads = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flowThreads
            // 
            this.flowThreads.AutoSize = true;
            this.flowThreads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowThreads.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowThreads.Location = new System.Drawing.Point(0, 0);
            this.flowThreads.Name = "flowThreads";
            this.flowThreads.Size = new System.Drawing.Size(305, 400);
            this.flowThreads.TabIndex = 1;
            this.flowThreads.WrapContents = false;
            // 
            // UIThreadsTracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.flowThreads);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UIThreadsTracker";
            this.Size = new System.Drawing.Size(305, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowThreads;
    }
}
