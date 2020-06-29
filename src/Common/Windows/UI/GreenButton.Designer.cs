namespace Gibraltar.Windows.UI
{
    partial class GreenButton
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
            this.btnInternalButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnInternalButton
            // 
            this.btnInternalButton.BackgroundImage = global::Gibraltar.Windows.UI.FancyButtons.green_button;
            this.btnInternalButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnInternalButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnInternalButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInternalButton.ForeColor = System.Drawing.Color.White;
            this.btnInternalButton.Location = new System.Drawing.Point(0, 0);
            this.btnInternalButton.Margin = new System.Windows.Forms.Padding(0);
            this.btnInternalButton.Name = "btnInternalButton";
            this.btnInternalButton.Size = new System.Drawing.Size(99, 36);
            this.btnInternalButton.TabIndex = 0;
            this.btnInternalButton.Text = "button1";
            this.btnInternalButton.UseVisualStyleBackColor = false;
            this.btnInternalButton.MouseLeave += new System.EventHandler(this.GreenButton_MouseLeave);
            this.btnInternalButton.Click += new System.EventHandler(this.btnInternalButton_Click);
            this.btnInternalButton.MouseEnter += new System.EventHandler(this.GreenButton_MouseEnter);
            // 
            // GreenButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnInternalButton);
            this.Name = "GreenButton";
            this.Size = new System.Drawing.Size(99, 36);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInternalButton;
    }
}