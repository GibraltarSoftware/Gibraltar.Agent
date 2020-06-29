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
using Gibraltar.Serialization;

#endregion File Header


namespace Gibraltar.Serialization
{
    /// <summary>
    /// Defines the interface necessary for a packet factory to be
    /// registered with IPacketReader.
    /// </summary>
    public interface IPacketFactory
    {
        /// <summary>
        /// This is the method that is invoked on an IPacketFactory to create an IPacket
        /// from the data in an IFieldReader given a specified PacketDefinition.
        /// </summary>
        /// <param name="definition">Definition of the fields expected in the next packet</param>
        /// <param name="reader">Data stream to be read</param>
        /// <returns>An IPacket corresponding to the PacketDefinition and the stream data</returns>
        IPacket CreatePacket(PacketDefinition definition, IFieldReader reader);
    }
}