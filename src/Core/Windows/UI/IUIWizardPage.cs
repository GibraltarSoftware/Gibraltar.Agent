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

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Standard base interface for all wizard pages.
    /// </summary>
    public interface IUIWizardPage
    {
        /// <summary>
        /// Initialize the page to start a new wizard working with the provided request data.
        /// </summary>
        /// <param name="requestData"></param>
        void Initialize(object requestData);

        /// <summary>
        /// The unique name of this page in the wizard, used for cross reference and logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The end-user title for this page in the wizard.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// End user instructions for completing this page of the wizard.
        /// </summary>
        string Instructions { get;  }

        /// <summary>
        /// The wizard controller is entering this page.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Raised whenever the IsValid property changes
        /// </summary>
        event EventHandler IsValidChanged;

        /// <summary>
        /// Indicates if the page is in a valid state to proceed.
        /// </summary>
        bool IsValid { get; }
    }
}
