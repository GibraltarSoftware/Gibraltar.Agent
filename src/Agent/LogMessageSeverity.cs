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
 *    mechanical, without the expreIstss written consent of Gibraltar Software, Inc,
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
    /// This enumerates the severity levels used by Loupe log messages.
    /// </summary>
    /// <remarks>The values for these levels are chosen to directly map to the TraceEventType enum
    /// for the five levels we support.  These also can be mapped from Log4Net event levels,
    /// with slight name changes for Fatal->Critical and for Debug->Verbose.</remarks>
    [Flags]
    public enum LogMessageSeverity
    {
        /// <summary>
        /// The severity level is uninitialized and thus unknown.
        /// </summary>
        None = 0,  // FxCop demands we have a defined 0.

        /// <summary>
        /// Fatal error or application crash.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Critical.  This also corresponds to Log4Net's Fatal.</remarks>
        Critical = TraceEventType.Critical, // = 1

        /// <summary>
        /// Recoverable error.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Error.  This also corresponds to Log4Net's Error.</remarks>
        Error = TraceEventType.Error, // = 2

        /// <summary>
        /// Noncritical problem.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Warning.  This also corresponds to Log4Net's Warning.</remarks>
        Warning = TraceEventType.Warning, // = 4

        /// <summary>
        /// Informational message.
        /// </summary>
        /// <remarks>This is equal to TraceEventType. Information, This also corresponds to Log4Net's Information.</remarks>
        Information = TraceEventType.Information, // = 8

        /// <summary>
        /// Debugging trace.
        /// </summary>
        /// <remarks>This is equal to TraceEventType.Verbose.  This also corresponds to Log4Net's Debug.</remarks>
        Verbose = TraceEventType.Verbose, // = 16
    }
}
