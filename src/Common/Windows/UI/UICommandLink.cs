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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// A fairly weak version of the Vista Command Link
    /// </summary>
    public partial class UICommandLink : UserControl
    {
        private bool m_ShowHighlight;

        /// <summary>
        /// create a new Command Link button.
        /// </summary>
        public UICommandLink()
        {
            InitializeComponent();
            m_ShowHighlight = true;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The text label of the button.
        /// </summary>
        [Browsable(true)]
        [Description("The text label of the button")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public string Label
        {
            get
            {
                return btnCommand.Text;
            }
            set
            {
                btnCommand.Text = value;
            }
        }

        /// <summary>
        /// Hides the highlight around the button
        /// </summary>
        [DefaultValue(true)]
        [Browsable(true)]
        [Description("Hides the colored border highlight")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public bool ShowHighlight
        {
            get { return m_ShowHighlight; }
            set
            {
                m_ShowHighlight = value;
                btnCommand.FlatAppearance.BorderSize = m_ShowHighlight ? 1 : 0;
            }
        }

        #endregion

        #region Event Handlers

        private void btnCommand_MouseEnter(object sender, EventArgs e)
        {
            btnCommand.ImageKey = "arrow_highlight";

            if (m_ShowHighlight)
            {
                btnCommand.FlatAppearance.BorderColor = Color.Turquoise;
                btnCommand.FlatAppearance.BorderSize = 1;
            }
        }

        private void btnCommand_MouseLeave(object sender, EventArgs e)
        {
            btnCommand.ImageKey = "arrow";

            if (m_ShowHighlight)
            {
                btnCommand.FlatAppearance.BorderColor = SystemColors.Window;
                btnCommand.FlatAppearance.BorderSize = 1;
            }
        }

        private void btnCommand_Click(object sender, EventArgs e)
        {
            if (m_ShowHighlight)    
                btnCommand.FlatAppearance.BorderSize = 1;

            base.OnClick(EventArgs.Empty);
        }

        #endregion

        private void btnCommand_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_ShowHighlight)
                btnCommand.FlatAppearance.BorderSize = 2;
        }

        private void btnCommand_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_ShowHighlight)
                btnCommand.FlatAppearance.BorderSize = 1;
        }
    }
}
