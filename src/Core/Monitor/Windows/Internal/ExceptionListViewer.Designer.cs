namespace Gibraltar.Monitor.Windows.Internal
{
    partial class ExceptionListViewer
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
            this.exceptionListTable = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // exceptionListTable
            // 
            this.exceptionListTable.ColumnCount = 2;
            this.exceptionListTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.exceptionListTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.exceptionListTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exceptionListTable.Location = new System.Drawing.Point(0, 0);
            this.exceptionListTable.Name = "exceptionListTable";
            this.exceptionListTable.RowCount = 2;
            this.exceptionListTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.exceptionListTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.exceptionListTable.Size = new System.Drawing.Size(335, 150);
            this.exceptionListTable.TabIndex = 0;
            // 
            // ExceptionListViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.exceptionListTable);
            this.Name = "ExceptionListViewer";
            this.Size = new System.Drawing.Size(335, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel exceptionListTable;


    }
}
