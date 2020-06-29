namespace Gibraltar.Windows.UI
{
    partial class UICommandLink
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UICommandLink));
            this.btnCommand = new System.Windows.Forms.Button();
            this.vistaEmulationImages = new System.Windows.Forms.ImageList();
            this.SuspendLayout();
            // 
            // btnCommand
            // 
            this.btnCommand.BackColor = System.Drawing.Color.Transparent;
            this.btnCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCommand.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCommand.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnCommand.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCommand.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCommand.ImageKey = "arrow";
            this.btnCommand.ImageList = this.vistaEmulationImages;
            this.btnCommand.Location = new System.Drawing.Point(0, 0);
            this.btnCommand.Margin = new System.Windows.Forms.Padding(0);
            this.btnCommand.Name = "btnCommand";
            this.btnCommand.Padding = new System.Windows.Forms.Padding(2, 6, 4, 6);
            this.btnCommand.Size = new System.Drawing.Size(199, 42);
            this.btnCommand.TabIndex = 26;
            this.btnCommand.Text = "btnCommandText";
            this.btnCommand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCommand.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCommand.UseVisualStyleBackColor = false;
            this.btnCommand.Click += new System.EventHandler(this.btnCommand_Click);
            this.btnCommand.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnCommand_MouseDown);
            this.btnCommand.MouseEnter += new System.EventHandler(this.btnCommand_MouseEnter);
            this.btnCommand.MouseLeave += new System.EventHandler(this.btnCommand_MouseLeave);
            this.btnCommand.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnCommand_MouseUp);
            // 
            // vistaEmulationImages
            // 
            this.vistaEmulationImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("vistaEmulationImages.ImageStream")));
            this.vistaEmulationImages.TransparentColor = System.Drawing.Color.Transparent;
            this.vistaEmulationImages.Images.SetKeyName(0, "arrow");
            this.vistaEmulationImages.Images.SetKeyName(1, "arrow_highlight");
            // 
            // UICommandLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.btnCommand);
            this.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.Name = "UICommandLink";
            this.Size = new System.Drawing.Size(199, 42);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCommand;
        private System.Windows.Forms.ImageList vistaEmulationImages;
    }
}
