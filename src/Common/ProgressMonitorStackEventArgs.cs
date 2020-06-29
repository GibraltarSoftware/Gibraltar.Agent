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
    public class ProgressMonitorStackEventArgs : EventArgs
    {
        private readonly ProgressMonitorStack m_ProgressMonitors;
        private readonly string m_StatusMessage;
        private readonly int m_CompletedSteps;
        private readonly int m_MaximumSteps;

        /// <summary>
        /// Create a new monitor changed event arguments object.
        /// </summary>
        /// <param name="progressMonitors">The monitor stack that changed</param>
        /// <param name="statusMessage">A user display message for the current status</param>
        /// <param name="completedSteps">The current progress step (between zero and the maximum number of steps)</param>
        /// <param name="maximumSteps">The current maximum number of steps</param>
        public ProgressMonitorStackEventArgs(ProgressMonitorStack progressMonitors, string statusMessage, int completedSteps, int maximumSteps)
        {
            m_ProgressMonitors = progressMonitors;
            m_StatusMessage = statusMessage;
            m_CompletedSteps = completedSteps;
            m_MaximumSteps = maximumSteps;
        }

        /// <summary>
        /// The stack of all monitors currently in use.
        /// </summary>
        public ProgressMonitorStack ProgressMonitors
        {
            get { return m_ProgressMonitors; }
        }

        /// <summary>
        /// A user display message for the current status.
        /// </summary>
        public string StatusMessage
        {
            get { return m_StatusMessage; }
        }

        /// <summary>
        /// The number of completed steps (between zero and the maximum number of steps).
        /// </summary>
        public int CompletedSteps
        {
            get { return m_CompletedSteps; }
        }

        /// <summary>
        /// The current maximum number of steps.
        /// </summary>
        public int MaximumSteps
        {
            get { return m_MaximumSteps; }
        }
    }

    /// <summary>
    /// The delegate for progress monitor stack events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ProgressMonitorStackEventHandler(object sender, ProgressMonitorStackEventArgs e); 
}
