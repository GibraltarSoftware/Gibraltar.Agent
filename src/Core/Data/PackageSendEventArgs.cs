
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


namespace Gibraltar.Data
{
    /// <summary>
    /// Information about the Package Send Events.
    /// </summary>
    public class PackageSendEventArgs : AsyncTaskResultEventArgs
    {
        /// <summary>
        /// Create a new result arguments object from the provided information
        /// </summary>
        /// <param name="fileSize">The number of bytes in the package, if sent successfully.</param>
        /// <param name="result">The final status of the task</param>
        /// <param name="message">Optional. A display message to complement the result.</param>
        /// <param name="exception">Optional. An exception object to allow the caller to do its own interpretation of an exception.</param>
        public PackageSendEventArgs(int fileSize, AsyncTaskResult result, string message, Exception exception)
            :base(result, message, exception)
        {
            FileSize = fileSize;
        }

        /// <summary>
        /// The number of bytes in the package, if sent successfully.
        /// </summary>
        public int FileSize { get; private set; }
    }

}
