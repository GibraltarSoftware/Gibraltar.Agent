namespace Gibraltar.Monitor.Windows
{
    partial class UIExceptionDetail
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
            this.flowExceptions = new System.Windows.Forms.FlowLayoutPanel();
            this.lblNoException = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // flowExceptions
            // 
            this.flowExceptions.AutoScroll = true;
            this.flowExceptions.AutoSize = true;
            this.flowExceptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowExceptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowExceptions.Location = new System.Drawing.Point(0, 0);
            this.flowExceptions.Name = "flowExceptions";
            this.flowExceptions.Padding = new System.Windows.Forms.Padding(0, 0, 24, 0);
            this.flowExceptions.Size = new System.Drawing.Size(690, 230);
            this.flowExceptions.TabIndex = 0;
            this.flowExceptions.WrapContents = false;
            this.flowExceptions.Resize += new System.EventHandler(this.flowExceptions_Resize);
            // 
            // lblNoException
            // 
            this.lblNoException.BackColor = System.Drawing.Color.Transparent;
            this.lblNoException.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoException.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoException.Location = new System.Drawing.Point(0, 0);
            this.lblNoException.Name = "lblNoException";
            this.lblNoException.Size = new System.Drawing.Size(690, 230);
            this.lblNoException.TabIndex = 0;
            this.lblNoException.Text = "No Exception Attached";
            this.lblNoException.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UIExceptionDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.lblNoException);
            this.Controls.Add(this.flowExceptions);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UIExceptionDetail";
            this.Size = new System.Drawing.Size(690, 230);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowExceptions;
        private System.Windows.Forms.Label lblNoException;
    }
}
