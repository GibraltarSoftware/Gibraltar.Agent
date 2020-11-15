
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
using Gibraltar.Serialization;

#endregion File Header

namespace Gibraltar.Monitor.Internal
{
    class AnalysisPacketFactory : IPacketFactory
    {
        private readonly string m_AnalysisPacketType;
        private readonly string m_CommentPacketType;
        private readonly string m_DataExtensionPacketType;
        private readonly string m_MarkerPacketType;

        public AnalysisPacketFactory()
        {
            //resolve the names of all the types we want to be able to get packets for
            //this lets us do a faster switch in CreatePacket
            m_AnalysisPacketType = typeof(AnalysisPacket).Name;
            m_CommentPacketType = typeof(CommentPacket).Name;
            m_DataExtensionPacketType = typeof(DataExtensionPacket).Name;
            m_MarkerPacketType = typeof(MarkerPacket).Name;
        }

        /// <summary>
        /// This is the method that is invoked on an IPacketFactory to create an IPacket
        /// from the data in an IFieldReader given a specified PacketDefinition.
        /// </summary>
        /// <param name="definition">Definition of the fields expected in the next packet</param>
        /// <param name="reader">Data stream to be read</param>
        /// <returns>An IPacket corresponding to the PacketDefinition and the stream data</returns>
        public IPacket CreatePacket(PacketDefinition definition, IFieldReader reader)
        {
            IPacket packet;

            //what we create varies by what specific definition they're looking for
            if (definition.TypeName == m_AnalysisPacketType)
            {
                packet = new AnalysisPacket();
            }
            else if (definition.TypeName == m_CommentPacketType)
            {
                packet = new CommentPacket();
            }
            else if (definition.TypeName == m_DataExtensionPacketType)
            {
                packet = new DataExtensionPacket();
            }
            else if (definition.TypeName == m_MarkerPacketType)
            {
                packet = new MarkerPacket();
            }
            else
            {
                //crap, we don't know what to do here.
                throw new ArgumentOutOfRangeException(nameof(definition), definition.TypeName, "This packet factory doesn't understand how to create packets for the provided type.");
            }

            //this feels a little crazy, but you have to do your own read call here - we aren't just creating the packet
            //object, we actually have to make the standard call to have it read data... 
            definition.ReadFields(packet, reader);

            return packet;
        }

        /// <summary>
        /// Register the packet factory with the packet reader for all packet types it supports
        /// </summary>
        /// <param name="packetReader"></param>
        public void Register(IPacketReader packetReader)
        {
            packetReader.RegisterFactory(m_AnalysisPacketType, this);
            packetReader.RegisterFactory(m_CommentPacketType, this);
            packetReader.RegisterFactory(m_DataExtensionPacketType, this);
            packetReader.RegisterFactory(m_MarkerPacketType, this);
        }
    }
}
