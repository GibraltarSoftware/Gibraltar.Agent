
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

#endregion

namespace Gibraltar.Monitor.Internal
{
    /// <summary>
    /// The serializable form of a data extension object.
    /// </summary>
    internal class DataExtensionPacket : GibraltarCachedPacket
    {
        /// <summary>
        /// Create a new data extension packet with the provided unique id of the data object extended by this packet.
        /// </summary>
        /// <param name="objectID"></param>
        public DataExtensionPacket(Guid objectID) 
            : base(objectID, false)
        {
        }

        /// <summary>
        /// Create a data extension packet for rehydration purposes.
        /// </summary>
        public DataExtensionPacket()
            : base(false)
        {            
        }   
    }
}
