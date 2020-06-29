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
using System.Collections.Generic;
using System.Text;
using Gibraltar.Data;
using Gibraltar.Messaging.Net;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Event arguments for session header changes
    /// </summary>
    public class SessionHeaderEventArgs: EventArgs
    {
        /// <summary>
        /// Create a new session header event arguments object
        /// </summary>
        /// <param name="header"></param>
        public SessionHeaderEventArgs(SessionHeader header)
        {
            SessionHeader = header;
        }

        /// <summary>
        /// The session header that was affected
        /// </summary>
        public SessionHeader SessionHeader { get; private set; }
    }

    /// <summary>
    /// Delegate for handling session header events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SessionHeaderEventHandler(object sender, SessionHeaderEventArgs e);
}
