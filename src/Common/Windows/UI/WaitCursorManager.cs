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
using System.Windows.Forms;

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Attempts to set the current form's wait cursor if not already set.
    /// </summary>
    /// <remarks>Designed for use in a Using statement so it will restore the previous state on return.</remarks>
    public class WaitCursorManager : IDisposable
    {
        private Form m_CurrentForm;

        private readonly bool m_PreviousWaitCursor;

        /// <summary>
        /// Set the current form's wait cursor if not already set.
        /// </summary>
        /// <param name="currentControl"></param>
        public WaitCursorManager(Control currentControl)
        {
            //what's our current form?
            m_CurrentForm = currentControl.FindForm();

            if (m_CurrentForm != null)
            {
                m_PreviousWaitCursor = m_CurrentForm.UseWaitCursor;
                m_CurrentForm.UseWaitCursor = true;
                Cursor.Current = Cursors.WaitCursor; // try to beat the cursor set by the event.
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (m_CurrentForm != null)
            {
                try
                {
                    m_CurrentForm.UseWaitCursor = m_PreviousWaitCursor;
                    m_CurrentForm = null;
                    if (m_PreviousWaitCursor == false)
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
                catch
                {
                }                
            }
        }
    }
}
