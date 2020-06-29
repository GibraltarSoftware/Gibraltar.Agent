#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
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
