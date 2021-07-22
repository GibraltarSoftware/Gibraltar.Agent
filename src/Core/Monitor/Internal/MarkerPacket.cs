
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
using System.Drawing;
using System.Reflection;
using Gibraltar.Serialization;

#endregion File Header

namespace Gibraltar.Monitor.Internal
{
    internal class MarkerPacket : GibraltarPacket, IPacket, IEquatable<MarkerPacket>
    {
        private Guid m_ID;
        private string m_Caption;
        private string m_Description;
        private Color m_Color;

        /// <summary>
        /// Create a new marker packet
        /// </summary>
        public MarkerPacket()
        {
            ID = Guid.NewGuid();
            Caption = string.Empty;
            Description = string.Empty;
        }

        #region Public Properties and Methods


        /// <summary>
        /// The unique Id of this marker
        /// </summary>
        public Guid ID { get { return m_ID; } set { m_ID = value; } }


        /// <summary>
        /// A display caption for this marker
        /// </summary>
        public string Caption { get { return m_Caption; } set { m_Caption = value; } }


        /// <summary>
        /// A display description for this marker.
        /// </summary>
        public string Description { get { return m_Description; } set { m_Description = value; } }


        /// <summary>
        /// The color to use for this marker.
        /// </summary>
        public Color Color { get { return m_Color; } set { m_Color = value; } }

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
            return Equals(other as MarkerPacket);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(MarkerPacket other)
        {
            //Careful - can be null
            if (other == null)
            {
                return false; // since we're a live object we can't be equal.
            }

            return ((ID == other.ID)
                 && (Caption == other.Caption)
                 && (Description == other.Description)
                 && (Color == other.Color)
                 && (base.Equals(other)));
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

            myHash ^= m_ID.GetHashCode(); // Fold in hash code for GUID
            if (m_Caption != null) myHash ^= m_Caption.GetHashCode(); // Fold in hash code for string Caption
            if (m_Description != null) myHash ^= m_Description.GetHashCode(); // Fold in hash code for string Description
            myHash ^= m_Color.GetHashCode(); // Fold in hash code for Color member

            return myHash;
        }

        #endregion

        #region IFastSerializable Members

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
            definition.Fields.Add("ID", m_ID.GetType());
            definition.Fields.Add("Caption", m_Caption.GetType());
            definition.Fields.Add("Description", m_Description.GetType());
            definition.Fields.Add("Color", FieldType.Int32);
            return definition;
        }

        void IPacket.WriteFields(PacketDefinition definition, SerializedPacket packet)
        {
            packet.SetField("ID", m_ID);
            packet.SetField("Caption", m_Caption);
            packet.SetField("Description", m_Description);
            packet.SetField("Color", Color.ToArgb());
        }

        void IPacket.ReadFields(PacketDefinition definition, SerializedPacket packet)
        {
            switch (definition.Version)
            {
                case 1:
                    packet.GetField("ID", out m_ID);
                    packet.GetField("Caption", out m_Caption);
                    packet.GetField("Description", out m_Description);

                    int colorArgb;
                    packet.GetField("Color", out colorArgb);
                    Color = Color.FromArgb(colorArgb);
                    break;
                    // don't need default case because ReadVersion is already validating that.
            }
        }

        #endregion

    }
}
