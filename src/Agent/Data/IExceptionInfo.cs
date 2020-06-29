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
namespace Gibraltar.Agent.Data
{
    /// <summary>
    /// An interface which provides recorded information about an Exception.
    /// </summary>
    public interface IExceptionInfo
    {
        /// <summary>
        /// The full name of the type of the Exception.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// The Message string of the Exception.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// A formatted string describing the source of an Exception.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// A string dump of the Exception stack trace information.
        /// </summary>
        string StackTrace { get; }

        /// <summary>
        /// The information about this exception's inner exception (or null if none).
        /// </summary>
        IExceptionInfo InnerException { get; }
    }
}