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



#endregion File Header

using System;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Event arguments for tracking the state of a web request
    /// </summary>
    public class WebRequestEventArgs : EventArgs
    {
        internal WebRequestEventArgs(WebRequestState state)
        {
            State = state;
        }

        /// <summary>
        /// The state of the web request when the event was raised.
        /// </summary>
        public WebRequestState State { get; private set; }
    }

    /// <summary>
    /// The state of a web request
    /// </summary>
    public enum WebRequestState
    {
        /// <summary>
        /// Not yet processed
        /// </summary>
        New = 0,

        /// <summary>
        /// Completed successfully.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Canceled before it could be completed
        /// </summary>
        Canceled = 2,

        /// <summary>
        /// Attempted but generated an error.
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// Delegate definition for a web request event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void WebRequestEventHandler(object sender, EventArgs e);
}
