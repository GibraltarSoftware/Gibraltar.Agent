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
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Draws our classic green gradient button with white text.
    /// </summary>
    [DefaultEvent("Click")]
    public partial class GreenButton : UserControl
    {
        /// <summary>
        /// Create a new instance of the green button
        /// </summary>
        public GreenButton()
        {
            InitializeComponent();
        }

        #region Public Properties and Methods

        /// <summary>
        /// Set the text to show on the button
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return btnInternalButton.Text;
            }
            set
            {
                btnInternalButton.Text = value;
            }
        }

        #endregion

        #region Protected Properies and Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ForeColorChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            btnInternalButton.ForeColor = ForeColor;
        }

        #endregion

        #region Event handlers

        private void btnInternalButton_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void GreenButton_MouseEnter(object sender, EventArgs e)
        {
            btnInternalButton.BackgroundImage = Gibraltar.Windows.UI.FancyButtons.green_button_highlight;
        }

        private void GreenButton_MouseLeave(object sender, EventArgs e)
        {
            btnInternalButton.BackgroundImage = Gibraltar.Windows.UI.FancyButtons.green_button;
        }

        #endregion
    }
}