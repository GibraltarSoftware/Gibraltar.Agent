
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
using System.Diagnostics;

#endregion File Header

namespace Gibraltar.Agent
{
    /// <summary>
    /// The result of processing an asynchronous task
    /// </summary>
    /// <remarks>For any value better than Error the task did complete its primary purpose.</remarks>
    [Flags]
    public enum AsyncTaskResult
    {
        /// <summary>
        /// The severity level is uninitialized and thus unknown.
        /// </summary>
        None = 0,  // FxCop demands we have a defined 0.

        /// <summary>
        /// The task was canceled before it could complete or fail to complete.
        /// </summary>
        Canceled = 1,

        /// <summary>
        /// The task failed.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Error.</remarks>
        Error = TraceEventType.Error, // = 2

        /// <summary>
        /// The task at least partially succeeded but there was a noncritical problem.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Warning.</remarks>
        Warning = TraceEventType.Warning, // = 4

        /// <summary>
        /// The task succeeded but generated an informational message.
        /// </summary>
        /// <remarks>This is equal to TraceEventType. Information</remarks>
        Information = TraceEventType.Information, // = 8

        /// <summary>
        /// The task succeeded completely.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Verbose.</remarks>
        Success = TraceEventType.Verbose, // = 16
    }
}
