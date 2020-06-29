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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gibraltar.Monitor.Windows
{
    /// <summary>
    /// A control to display the latest message logged by each active thread. (Place-holder, not functional.)
    /// </summary>
    public partial class UIThreadsTracker : UserControl
    {
        /// <summary>
        /// Create a blank UIThreadsTracker control.
        /// </summary>
        public UIThreadsTracker()
        {
            InitializeComponent();
        }
    }
}
