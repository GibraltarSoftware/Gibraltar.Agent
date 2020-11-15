
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
    internal class MetricPacketFactory : IPacketFactory
    {
        private readonly Session m_Session;
        private readonly string m_MetricPacketType;
        private readonly string m_SampledMetricPacketType;
        private readonly string m_EventMetricPacketType;
        private readonly string m_CustomSampledMetricPacketType;
        private readonly string m_PerfCounterMetricPacketType;

        public MetricPacketFactory(Session session)
        {
            m_Session = session;

            //resolve the names of all the types we want to be able to get packets for
            //this lets us do a faster switch in CreatePacket
            m_MetricPacketType = typeof(MetricPacket).Name;
            m_SampledMetricPacketType = typeof(SampledMetricPacket).Name;
            m_EventMetricPacketType = typeof(EventMetricPacket).Name;
            m_CustomSampledMetricPacketType = typeof(CustomSampledMetricPacket).Name;
            m_PerfCounterMetricPacketType = typeof(PerfCounterMetricPacket).Name;
        }

        /// <summary>
        /// Register the packet factory with the packet reader for all packet types it supports
        /// </summary>
        /// <param name="packetReader"></param>
        public void Register(IPacketReader packetReader)
        {
            packetReader.RegisterFactory(m_MetricPacketType, this);
            packetReader.RegisterFactory(m_SampledMetricPacketType, this);
            packetReader.RegisterFactory(m_EventMetricPacketType, this);
            packetReader.RegisterFactory(m_CustomSampledMetricPacketType, this);
            packetReader.RegisterFactory(m_PerfCounterMetricPacketType, this);
        }

        public IPacket CreatePacket(PacketDefinition definition, IFieldReader reader)
        {
            IPacket packet;

            //what we create varies by what specific definition they're looking for
            if (definition.TypeName == m_MetricPacketType)
            {
                //metrics can't be created directly - they're an abstract class.
                throw new ArgumentOutOfRangeException(nameof(definition), definition.TypeName, "Metric objects can't be created, only derived classes can.");
            } 

            if (definition.TypeName == m_SampledMetricPacketType)
            {
                //sampled metrics can't be created directly - they're an abstract class.
                throw new ArgumentOutOfRangeException(nameof(definition), definition.TypeName, "Sampled Metric objects can't be created, only derived classes can.");
            }

            if (definition.TypeName == m_EventMetricPacketType)
            {
                packet = new EventMetricPacket(m_Session);
            }
            else if (definition.TypeName == m_CustomSampledMetricPacketType)
            {
                packet = new CustomSampledMetricPacket(m_Session);
            }
            else if (definition.TypeName == m_PerfCounterMetricPacketType)
            {
                packet = new PerfCounterMetricPacket(m_Session);
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
    }
}
