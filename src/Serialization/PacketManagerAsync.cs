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
using System.Diagnostics;
using System.IO;

namespace Gibraltar.Serialization
{
    /// <summary>
    /// Efficiently manage the deserialization of log packets with asynchronous reading of buffers
    /// </summary>
    public class PacketManagerAsync : PacketManagerBase
    {
        private readonly Stream m_Stream;

        /// <summary>
        /// TBD: Need to integrate optimized version of PipeStream.
        /// </summary>
        /// <param name="stream"></param>
        public PacketManagerAsync(Stream stream)
        {
            m_Stream = stream;
        }

        /// <summary>
        /// Get the next packet from the stream
        /// </summary>
        /// <returns>Returns a Packet or null if a Packet is not available</returns>
        public override MemoryStream GetNextPacket()
        {
            return null;
        }
    }
}
