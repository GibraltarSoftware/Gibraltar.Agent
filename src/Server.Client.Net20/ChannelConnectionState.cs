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
namespace Gibraltar.Server.Client
{
    /// <summary>
    /// The status of the subscription connection
    /// </summary>
    public enum ChannelConnectionState
    {
        /// <summary>
        /// The subscription is disconnected
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// The subscription is attempting to connect
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// The subscription is connected.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// The subscription is actively transferring data
        /// </summary>
        TransferringData = 3
    }
}
