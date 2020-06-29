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

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// A standard interface implemented by a user control to support basic wizard sequencing
    /// </summary>
    public interface IUIWizardInputPage : IUIWizardPage
    {
        /// <summary>
        /// Called to request the page store its state back into the request object.
        /// </summary>
        /// <remarks>Return false to cancel moving to the next page.</remarks>
        bool Store();
    }
}