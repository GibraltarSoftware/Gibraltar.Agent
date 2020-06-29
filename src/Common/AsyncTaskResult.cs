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
using System.Diagnostics;

#endregion File Header

namespace Gibraltar
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
        Unknown = 0,  // FxCop demands we have a defined 0.

        /// <summary>
        /// The task was canceled before it could complete or fail to complete.
        /// </summary>
        Canceled = 1,

        /// <summary>
        /// Recoverable error.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Error.</remarks>
        Error = TraceEventType.Error, // = 2

        /// <summary>
        /// Noncritical problem.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Warning.</remarks>
        Warning = TraceEventType.Warning, // = 4

        /// <summary>
        /// Informational message.
        /// </summary>
        /// <remarks>This is equal to TraceEventType. Information</remarks>
        Information = TraceEventType.Information, // = 8

        /// <summary>
        /// Debugging trace.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Verbose.</remarks>
        Success = TraceEventType.Verbose, // = 16
    }
}
