
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
    internal class MetricDefinitionPacketFactory: IPacketFactory
    {
        private readonly Session m_Session;
        private readonly string m_MetricDefinitionPacketType;
        private readonly string m_SampledMetricDefinitionPacketType;
        private readonly string m_EventMetricDefinitionPacketType;
        private readonly string m_EventMetricValueDefinitionPacketType;
        private readonly string m_CustomSampledMetricDefinitionPacketType;
        private readonly string m_PerfCounterMetricDefinitionPacketType;

        public MetricDefinitionPacketFactory(Session session)
        {
            m_Session = session;

            //resolve the names of all the types we want to be able to get packets for
            //this lets us do a faster switch in CreatePacket
            m_MetricDefinitionPacketType = typeof(MetricDefinitionPacket).Name;
            m_SampledMetricDefinitionPacketType = typeof(SampledMetricDefinitionPacket).Name;
            m_EventMetricDefinitionPacketType = typeof(EventMetricDefinitionPacket).Name;
            m_EventMetricValueDefinitionPacketType = typeof(EventMetricValueDefinitionPacket).Name;
            m_CustomSampledMetricDefinitionPacketType = typeof(CustomSampledMetricDefinitionPacket).Name;
            m_PerfCounterMetricDefinitionPacketType = typeof(PerfCounterMetricDefinitionPacket).Name;
        }

        /// <summary>
        /// Register the packet factory with the packet reader for all packet types it supports
        /// </summary>
        /// <param name="packetReader"></param>
        public void Register(IPacketReader packetReader)
        {
            packetReader.RegisterFactory(m_MetricDefinitionPacketType, this);
            packetReader.RegisterFactory(m_SampledMetricDefinitionPacketType, this);
            packetReader.RegisterFactory(m_EventMetricDefinitionPacketType, this);
            packetReader.RegisterFactory(m_EventMetricValueDefinitionPacketType, this);
            packetReader.RegisterFactory(m_CustomSampledMetricDefinitionPacketType, this);
            packetReader.RegisterFactory(m_PerfCounterMetricDefinitionPacketType, this);
        }

        public IPacket CreatePacket(PacketDefinition definition, IFieldReader reader)
        {
            IPacket packet;

            if (definition.TypeName == m_SampledMetricDefinitionPacketType)
            {
                //sampled metrics can't be created directly - they're an abstract class.
                throw new ArgumentOutOfRangeException(nameof(definition), definition.TypeName, "Sampled Metric objects can't be created, only derived classes can.");
            }

            //what we create varies by what specific definition they're looking for
            if (definition.TypeName == m_MetricDefinitionPacketType)
            {
                packet = new MetricDefinitionPacket(m_Session);
            }
            else if (definition.TypeName == m_EventMetricDefinitionPacketType)
            {
                packet = new EventMetricDefinitionPacket(m_Session);
            }
            else if (definition.TypeName == m_EventMetricValueDefinitionPacketType)
            {
                packet = new EventMetricValueDefinitionPacket(m_Session);
            }
            else if (definition.TypeName == m_CustomSampledMetricDefinitionPacketType)
            {
                packet = new CustomSampledMetricDefinitionPacket(m_Session);
            }
            else if (definition.TypeName == m_PerfCounterMetricDefinitionPacketType)
            {
                packet = new PerfCounterMetricDefinitionPacket(m_Session);
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
