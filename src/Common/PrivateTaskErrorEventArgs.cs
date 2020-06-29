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
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Event arguments for the TaskException event 
    /// </summary>
    public class PrivateTaskErrorEventArgs: EventArgs
    {
        /// <summary>
        /// Create a new event argument object for the provided task information
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ex"></param>
        public PrivateTaskErrorEventArgs(object state, Exception ex)
        {
            State = state;
            Exception = ex;
        }

        /// <summary>
        /// The state object provided for the task (if any)
        /// </summary>
        public object State { get; private set; }

        /// <summary>
        /// The exception that was generated.
        /// </summary>
        public Exception Exception { get; private set; }
    }

    /// <summary>
    /// Event handler for using the Private Task Error event args.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="e"></param>
    public delegate void PrivateTaskErrorEventHandler(object state, PrivateTaskErrorEventArgs e);
}
