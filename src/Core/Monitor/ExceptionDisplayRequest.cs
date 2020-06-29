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

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System;
using System.Windows.Forms;

#endregion

namespace Gibraltar.Monitor
{
    /// <summary>
    /// A request to display an exception 
    /// </summary>
    public class ExceptionDisplayRequest
    {
        private DialogResult m_Result; //PROTECTED BY LOCKING THIS

        /// <summary>
        /// Create a new exception display request.
        /// </summary>
        /// <param name="exception">The exception to be displayed</param>
        /// <param name="threadBlocked">Indicates if the original thread the exception was caught on is blocked waiting on this display request.</param>
        /// <param name="canContinue">True if the application can continue after this request, false if this is a fatal error
        /// and the application can not continue after this request.</param>
        public ExceptionDisplayRequest(Exception exception, bool threadBlocked, bool canContinue)
        {
            Timestamp = DateTimeOffset.Now;
            Exception = exception;
            ThreadBlocked = threadBlocked;
            CanContinue = canContinue;
            m_Result = DialogResult.None;
        }

        /// <summary>
        /// The exact timestamp the exception request was made.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// The exception to be displayed.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Indicates if the original thread the exception was caught on is blocked waiting on this display request.
        /// </summary>
        public bool ThreadBlocked { get; private set; }

        /// <summary>
        /// Indicates if the application can continue after this request is resolved or is a fatal error.
        /// </summary>
        public bool CanContinue { get; private set; }

        /// <summary>
        /// The current result.
        /// </summary>
        public DialogResult Result
        {
            get
            {
                lock(this) //we are using this because others are using our lock as a way of being signaled.
                {
                    //System.Threading.Monitor.PulseAll(this); // Not necessary to pulse because we aren't changing any state.

                    return m_Result;
                }
            }
            set
            {
                lock(this) //we are using this because others are using our lock as a way of being signaled.
                {
                    m_Result = value;

                    System.Threading.Monitor.PulseAll(this);
                }
            }
        }

        /// <summary>
        /// Wait for the Result to get set (other than None).  Must be passed to the Alert Dialog before calling this method!
        /// </summary>
        /// <returns></returns>
        public DialogResult WaitForResult()
        {
            DialogResult result;
            lock (this)
            {
                while (Result == DialogResult.None)
                {
                    System.Threading.Monitor.Wait(this);
                }
                result = Result;
            }
            return result;
        }
    }
}
