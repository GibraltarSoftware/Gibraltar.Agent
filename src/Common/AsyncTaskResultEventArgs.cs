
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
    /// The results from processing an asynchronous task.
    /// </summary>
    public class AsyncTaskResultEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new result arguments object from the provided information
        /// </summary>
        /// <param name="result">The final status of the task</param>
        /// <param name="message">Optional. A display message to complement the result.</param>
        public AsyncTaskResultEventArgs(AsyncTaskResult result, string message)
        {
            Result = result;
            Message = message;
        }

        /// <summary>
        /// Create a new result arguments object from the provided information
        /// </summary>
        /// <param name="result">The final status of the task</param>
        /// <param name="message">Optional. A display message to complement the result.</param>
        /// <param name="exception">Optional. An exception object to allow the caller to do its own interpretation of an exception.</param>
        public AsyncTaskResultEventArgs(AsyncTaskResult result, string message, Exception exception)
        {
            Result = result;
            Message = message;
            Exception = exception;
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
    }

    /// <summary>
    /// A standard event handler for asynchronous task results
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void AsyncTaskResultEventHandler(object sender, AsyncTaskResultEventArgs args);
}
