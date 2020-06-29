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
using System;
using System.Reflection;
using Gibraltar.Serialization;

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor.Internal
{
    internal class AnalysisPacket : GibraltarCachedPacket, IPacket, IEquatable<AnalysisPacket>
    {
        private string m_Caption;
        private string m_Description;

        public AnalysisPacket()
            : base(false)
        {
            
        }

        #region Public Properties and Methods

        public string Caption { get { return m_Caption; } set { m_Caption = value; } }

        public string Description { get { return m_Description; } set { m_Description = value; } }

        #endregion

        #region IPacket Members

        private const int SerializationVersion = 1;

        /// <summary>
        /// The list of packets that this packet depends on.
        /// </summary>
        /// <returns>An array of IPackets, or null if there are no dependencies.</returns>
        IPacket[] IPacket.GetRequiredPackets()
        {
            //the majority of packets have no dependencies
            return null;
        }

        PacketDefinition IPacket.GetPacketDefinition()
        {
            string typeName = MethodBase.GetCurrentMethod().DeclaringType.Name;
            PacketDefinition definition = new PacketDefinition(typeName, SerializationVersion, false);
            definition.Fields.Add("Caption", FieldType.String);
            definition.Fields.Add("Description", FieldType.String);
            return definition;
        }

        void IPacket.ReadFields(PacketDefinition definition, SerializedPacket packet)
        {
            switch (definition.Version)
            {
                case 1:
                    //pull off the non-object simple stuff first
                    packet.GetField("Caption", out m_Caption);
                    packet.GetField("Description", out m_Caption);
                    break;
            }
        }

        void IPacket.WriteFields(PacketDefinition definition, SerializedPacket packet)
        {
            //put in our basic types
            packet.SetField("Caption", m_Caption);
            packet.SetField("Description", m_Description);
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(object other)
        {
            //use our type-specific override
            return Equals(other as AnalysisPacket);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AnalysisPacket other)
        {
            //Careful - can be null
            if (other == null)
            {
                return false; // since we're a live object we can't be equal.
            }

            return ((Caption == other.Caption) 
                && (Description == other.Description)
                && base.Equals(other));
        }

        /// <summary>
        /// Provides a representative hash code for objects of this type to spread out distribution
        /// in hash tables.
        /// </summary>
        /// <remarks>Objects which consider themselves to be Equal (a.Equals(b) returns true) are
        /// expected to have the same hash code.  Objects which are not Equal may have the same
        /// hash code, but minimizing such overlaps helps with efficient operation of hash tables.
        /// </remarks>
        /// <returns>
        /// an int representing the hash code calculated for the contents of this object
        /// </returns>
        public override int GetHashCode()
        {
            int myHash = base.GetHashCode(); // Fold in hash code for inherited base type

            if (m_Caption != null) myHash ^= m_Caption.GetHashCode(); // Fold in hash code for string caption
            if (m_Description != null) myHash ^= m_Description.GetHashCode(); // Fold in hash code for string description

            return myHash;
        }
    }
}
