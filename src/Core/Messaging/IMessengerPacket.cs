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
using Gibraltar.Serialization;

namespace Gibraltar.Messaging
{
    /// <summary>
    /// This interface is required to be a publishable packet
    /// </summary>
    public interface IMessengerPacket : IPacket
    {
        /// <summary>
        /// The unique sequence number of this packet in the session
        /// </summary>
        long Sequence { get; set; }

        /// <summary>
        /// The timestamp when this packet was created
        /// </summary>
        DateTimeOffset Timestamp { get; set; }
    }
}