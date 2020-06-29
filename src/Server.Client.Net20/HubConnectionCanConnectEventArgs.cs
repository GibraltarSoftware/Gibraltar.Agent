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
using System.ComponentModel;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Event arguments for the CanConnectCompleted event.
    /// </summary>
    public class HubConnectionCanConnectEventArgs: AsyncCompletedEventArgs
    {
        internal HubConnectionCanConnectEventArgs(Exception error, bool cancelled, HubStatus? status, string message, bool isValid)
            :base(error, cancelled, null)
        {
            Status = status;
            Message = message;
            IsValid = isValid;
        }

        /// <summary>
        /// The status of the connection
        /// </summary>
        public HubStatus? Status { get; private set; }

        /// <summary>
        /// A descriptive message of the connection status
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Indicates if the connection was successful
        /// </summary>
        public bool IsValid { get; private set; }
    }

    /// <summary>
    /// Delegate for the CanConnectCompleted event
    /// </summary>
    /// <param name="state"></param>
    /// <param name="e"></param>
    public delegate void HubConnectionCanConnectEventHandler(object state, HubConnectionCanConnectEventArgs e);
}
