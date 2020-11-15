
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

namespace Gibraltar.Agent
{
    /// <summary>
    /// Information about the Package Send Events.
    /// </summary>
    public sealed class PackageSendEventArgs : EventArgs
    {
        internal PackageSendEventArgs(Gibraltar.Data.PackageSendEventArgs args)
        {
            FileSize = args.FileSize;
            Result = (AsyncTaskResult)args.Result;
            Message = args.Message;
            Exception = args.Exception;
        }

        /// <summary>
        /// The final status of the task.
        /// </summary>
        public AsyncTaskResult Result { get; private set; }

        /// <summary>
        /// Optional.  An end-user display message to complement the result.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Optional.  An exception object to allow custom interpretation of an exception.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The number of bytes in the package, if sent successfully.
        /// </summary>
        public int FileSize { get; private set; }
    }

}
