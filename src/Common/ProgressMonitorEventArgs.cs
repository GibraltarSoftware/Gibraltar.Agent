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

namespace Gibraltar
{
    /// <summary>
    /// Information about monitor changes
    /// </summary>
    public class ProgressMonitorEventArgs : EventArgs
    {
        private readonly ProgressMonitorStack m_ProgressMonitors;
        private readonly ProgressMonitor m_ProgressMonitor;

        /// <summary>
        /// Create a new monitor changed event arguments object.
        /// </summary>
        /// <param name="progressMonitors">The monitor stack that changed.</param>
        /// <param name="progressMonitor">The monitor object (if any) affected by the change.</param>
        public ProgressMonitorEventArgs(ProgressMonitorStack progressMonitors, ProgressMonitor progressMonitor)
        {
            m_ProgressMonitors = progressMonitors;
            m_ProgressMonitor = progressMonitor;
        }

        /// <summary>
        /// The stack of all monitors currently in use.
        /// </summary>
        public ProgressMonitorStack ProgressMonitors
        {
            get { return m_ProgressMonitors; }
        }

        /// <summary>
        /// The monitor that was changed (may not be the top monitor on the stack)
        /// </summary>
        public ProgressMonitor ProgressMonitor
        {
            get { return m_ProgressMonitor; }
        }
    }

}
