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



#endregion

using System;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// The event arguments for the connection state changed event
    /// </summary>
    public class ChannelConnectionStateChangedEventArgs : EventArgs
    {
        internal ChannelConnectionStateChangedEventArgs(ChannelConnectionState state)
        {
            State = state;
        }

        /// <summary>
        /// The current connection state
        /// </summary>
        public ChannelConnectionState State { get; private set; }
    }


    /// <summary>
    /// Event handler for the connection state changed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ChannelConnectionStateChangedEventHandler(object sender, ChannelConnectionStateChangedEventArgs e);
}
