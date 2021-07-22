
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
    /// This private helper class is used exclusively to wrapper the exception
    /// sent by the ErrorNotifier.Notify method.
    /// </summary>
    public class ErrorNotificationEventArgs : EventArgs
    {
        private readonly Exception m_Exception;

        /// <summary>
        /// Create an ErrorNotificationEventArgs wrapping the provided exception
        /// </summary>
        public ErrorNotificationEventArgs(Exception exception)
        {
            m_Exception = exception;
        }

        /// <summary>
        /// The Exception we are providing notification on
        /// </summary>
        public Exception Exception { get { return m_Exception; } }
    }
}