namespace Gibraltar.Monitor.Windows.Internal
{
    partial class UIException
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
            this.lblStackTrace = new System.Windows.Forms.LinkLabel();
            this.txtTypeMessage = new System.Windows.Forms.TextBox();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lblExceptionIndex = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCopyAll = new System.Windows.Forms.Button();
            this.panelLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStackTrace
            // 
            this.lblStackTrace.AutoSize = true;
            this.lblStackTrace.BackColor = System.Drawing.Color.Transparent;
            this.lblStackTrace.Location = new System.Drawing.Point(24, 36);
            this.lblStackTrace.Margin = new System.Windows.Forms.Padding(12, 3, 3, 5);
            this.lblStackTrace.Name = "lblStackTrace";
            this.lblStackTrace.Size = new System.Drawing.Size(75, 13);
            this.lblStackTrace.TabIndex = 2;
            this.lblStackTrace.TabStop = true;
            this.lblStackTrace.Text = "<StackTrace>";
            this.lblStackTrace.UseMnemonic = false;
            this.lblStackTrace.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblStackTrace_LinkClicked);
            this.lblStackTrace.TextChanged += new System.EventHandler(this.TextChangedHandler);
            this.lblStackTrace.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lblStackTrace_MouseDoubleClick);
            this.lblStackTrace.MouseEnter += new System.EventHandler(this.StackTrace_MouseEnter);
            this.lblStackTrace.MouseLeave += new System.EventHandler(this.StackTrace_MouseLeave);
            // 
            // txtTypeMessage
            // 
            this.txtTypeMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTypeMessage.BackColor = System.Drawing.SystemColors.Window;
            this.txtTypeMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTypeMessage.Location = new System.Drawing.Point(15, 3);
            this.txtTypeMessage.Multiline = true;
            this.txtTypeMessage.Name = "txtTypeMessage";
            this.txtTypeMessage.ReadOnly = true;
            this.txtTypeMessage.Size = new System.Drawing.Size(560, 27);
            this.txtTypeMessage.TabIndex = 0;
            this.txtTypeMessage.Text = "<Source> : <TypeName>\r\n<Message>";
            this.txtTypeMessage.TextChanged += new System.EventHandler(this.TextChangedHandler);
            this.txtTypeMessage.MouseEnter += new System.EventHandler(this.txtTypeMessage_MouseEnter);
            this.txtTypeMessage.MouseLeave += new System.EventHandler(this.txtTypeMessage_MouseLeave);
            // 
            // panelLeft
            // 
            this.panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelLeft.BackColor = System.Drawing.Color.SteelBlue;
            this.panelLeft.Controls.Add(this.lblExceptionIndex);
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Margin = new System.Windows.Forms.Padding(0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(12, 85);
            this.panelLeft.TabIndex = 4;
            // 
            // lblExceptionIndex
            // 
            this.lblExceptionIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblExceptionIndex.AutoSize = true;
            this.lblExceptionIndex.BackColor = System.Drawing.Color.Transparent;
            this.lblExceptionIndex.ForeColor = System.Drawing.Color.White;
            this.lblExceptionIndex.Location = new System.Drawing.Point(0, 36);
            this.lblExceptionIndex.Name = "lblExceptionIndex";
            this.lblExceptionIndex.Size = new System.Drawing.Size(13, 13);
            this.lblExceptionIndex.TabIndex = 0;
            this.lblExceptionIndex.Text = "0";
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelBottom.BackColor = System.Drawing.Color.SteelBlue;
            this.panelBottom.Location = new System.Drawing.Point(0, 84);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(0);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(578, 1);
            this.panelBottom.TabIndex = 5;
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.AutoSize = true;
            this.btnCopy.FlatAppearance.BorderSize = 0;
            this.btnCopy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(537, 36);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(41, 23);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Visible = false;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            this.btnCopy.MouseEnter += new System.EventHandler(this.btnCopy_MouseEnter);
            // 
            // btnCopyAll
            // 
            this.btnCopyAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyAll.AutoSize = true;
            this.btnCopyAll.FlatAppearance.BorderSize = 0;
            this.btnCopyAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopyAll.Location = new System.Drawing.Point(476, 36);
            this.btnCopyAll.Name = "btnCopyAll";
            this.btnCopyAll.Size = new System.Drawing.Size(55, 23);
            this.btnCopyAll.TabIndex = 7;
            this.btnCopyAll.Text = "Copy All";
            this.btnCopyAll.UseVisualStyleBackColor = true;
            this.btnCopyAll.Visible = false;
            this.btnCopyAll.Click += new System.EventHandler(this.btnCopyAll_Click);
            this.btnCopyAll.MouseEnter += new System.EventHandler(this.btnCopy_MouseEnter);
            // 
            // UIException
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.btnCopyAll);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.txtTypeMessage);
            this.Controls.Add(this.lblStackTrace);
            this.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.MinimumSize = new System.Drawing.Size(500, 30);
            this.Name = "UIException";
            this.Size = new System.Drawing.Size(578, 86);
            this.BackColorChanged += new System.EventHandler(this.UIException_BackColorChanged);
            this.MouseEnter += new System.EventHandler(this.UIException_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.UIException_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UIException_MouseMove);
            this.Resize += new System.EventHandler(this.UIException_Resize);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTypeMessage;
        private System.Windows.Forms.LinkLabel lblStackTrace;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblExceptionIndex;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCopyAll;
    }
}
