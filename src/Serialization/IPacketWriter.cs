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
namespace Gibraltar.Serialization
{
    /// <summary>
    /// Implemented to support writing packets
    /// </summary>
    /// <remarks>Having everything use an interface allows us to support NMOCK</remarks>
    public interface IPacketWriter
    {
        /// <summary>
        /// Returns the current position within the stream.
        /// </summary>
        long Position { get; }

        /// <summary>
        /// Returns the length of the stream.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Write the data needed to serialize the state of the packet
        /// </summary>
        /// <param name="packet">Object to be serialized, must implement IPacket</param>
        void Write(IPacket packet);
    }
}