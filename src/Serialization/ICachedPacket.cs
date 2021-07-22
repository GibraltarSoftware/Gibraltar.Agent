
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

namespace Gibraltar.Serialization
{
    /// <summary>Implemented on invariant packets that can be cached</summary>
    /// <remarks>
    /// This interface extends IPacket to handle packets that are referenced
    /// by multiple packets and should only be serialized once.
    /// </remarks>
    public interface ICachedPacket : IPacket
    {
        /// <summary>
        /// The unique Id of the packet
        /// </summary>
        Guid ID { get; }
    }
}