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
using System.Windows.Forms;
using Gibraltar.Monitor.Windows;

#endregion File Header

namespace Gibraltar.Windows.UI.Internal
{
    /// <summary>
    /// The standard wizard result page
    /// </summary>
    public partial class UIWizardResultPage : UserControl
    {
        private readonly AsyncTaskResultEventArgs m_Results;

        /// <summary>
        /// Create a standard wizard result page
        /// </summary>
        /// <param name="results"></param>
        public UIWizardResultPage(AsyncTaskResultEventArgs results)
        {
            m_Results = results;

            InitializeComponent();

            //pick the right image.
            switch (m_Results.Result)
            {
                case AsyncTaskResult.Error:
                    statusPicture.Image = UIResources.error;
                    break;
                case AsyncTaskResult.Warning:
                    statusPicture.Image = UIResources.Warning;
                    break;
                case AsyncTaskResult.Information:
                    statusPicture.Image = UIResources.info;
                    break;
                case AsyncTaskResult.Success:
                    statusPicture.Image = UIResources.info;
                    break;
                default:
                    statusPicture.Visible = false;
                    break;
            }

            lblErrorMessage.Text = m_Results.Message;
        }
    }
}
