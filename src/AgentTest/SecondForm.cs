
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gibraltar.Agent.Data;

namespace Gibraltar.Agent.Test
{
    public partial class SecondForm : Form
    {
        public SecondForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //if (e != null)
            //    throw new Exception("Testing Manager deadlock case");

            base.OnLoad(e);
        }

        private void liveLogViewer_MessageFilter(object sender, LogMessageFilterEventArgs e)
        {
            ILogMessage message = e.Message;

            // Filter messages, only show Warnings and up.
            if (message.Severity > LogMessageSeverity.Warning)
                e.Cancel = true;
        }
    }
}
