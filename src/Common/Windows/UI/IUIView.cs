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
 *    Copyright © 2010 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/

using System;
using System.Windows.Forms;

#endregion

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Implemented by controls or forms that implement a View
    /// </summary>
    /// <remarks>Views can be managed with the standard UITreeView control and any form or control that implements IUIContainer.</remarks>
    public interface IUIView
    {
        /// <summary>
        /// raised by a view when it is closed, event if closed explicitly.
        /// </summary>
        event EventHandler ViewClosed;

        /// <summary>
        /// Raised by a view when it wants to close.
        /// </summary>
        event FormClosingEventHandler ViewClosing;

        /// <summary>
        /// A display caption for the view (typically the Text property of a control or form)
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Called to ask the UIView to refresh the data its display is based on.
        /// </summary>
        void RefreshData();

        /// <summary>
        /// Close open views and prepare to be released, depth-first recursively through our child nodes.  
        /// </summary>
        /// <param name="closeReason">The reason that the node is being closed.</param>
        /// <returns>True if the close was successful, false if it was unsuccessful or canceled by the user.</returns>
        bool Close(CloseReason closeReason);
    }
}
