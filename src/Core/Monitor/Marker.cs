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

using System;
using System.Drawing;
using Gibraltar.Monitor.Internal;

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    public class Marker :  IDisplayable, IComparable<Marker>, IEquatable<Marker>
    {
        private readonly Analysis m_Analysis;
        private readonly MarkerPacket m_Packet;

        /// <summary>
        /// Create a new marker for the specified analysis
        /// </summary>
        /// <param name="analysis">The analysis that will own this marker</param>
        public Marker(Analysis analysis)
        {
            m_Analysis = analysis;
            m_Packet = new MarkerPacket();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The analysis this data extension object is associated with
        /// </summary>
        public Analysis Analysis
        {
            get { return m_Analysis; }
        }

        /// <summary>
        /// The unique Id of this marker
        /// </summary>
        public Guid Id
        {
            get { return m_Packet.ID; }
        }

        /// <summary>
        /// A display caption for this marker
        /// </summary>
        public string Caption
        {
            get
            {
                return m_Packet.Caption;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_Packet.Caption = value;
                }
                else
                {
                    m_Packet.Caption = value.Trim();                    
                }
            }
        }

        /// <summary>
        /// A display description for this marker.
        /// </summary>
        public string Description
        {
            get
            {
                return m_Packet.Description;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_Packet.Description = value;
                }
                else
                {
                    m_Packet.Description = value.Trim();
                }
            }
        }

        /// <summary>
        /// The color to use for this marker.
        /// </summary>
        public Color Color
        {
            get
            {
                return m_Packet.Color;
            }
            set
            {
                m_Packet.Color = value;
            }
        }

        public int CompareTo(Marker other)
        {
            //check if the GUID's are equal.  That is our high speed equality check.
            if (Id == other.Id)
            {
                return 0;
            }

            //otherwise do the long hand comparison for sorting purposes

            //sort by caption
            int compareResult = string.Compare(Caption, other.Caption, StringComparison.InvariantCulture);

            if (compareResult == 0)
            {
                //same caption, compare description. If it's the same, someone messed up.
                compareResult = string.Compare(Description, other.Description, StringComparison.InvariantCulture);
            }

            return compareResult;
        }

        public bool Equals(Marker other)
        {
            // Careful, it could be null; check it without recursion
            if (object.ReferenceEquals(other, null))
            {
                return false; // Since we're a live object we can't be equal to a null instance.
            }

            //we compare the GUIDs for equality
            return (Id == other.Id);
        }

        /// <summary>
        /// Determines if the provided object is identical to this object.
        /// </summary>
        /// <param name="obj">The object to compare this object to</param>
        /// <returns>True if the other object is also a Marker and represents the same data.</returns>
        public override bool Equals(object obj)
        {
            Marker otherMarker = obj as Marker;

            return Equals(otherMarker); // Just have type-specific Equals do the check (it even handles null)
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
        /// An int representing the hash code calculated for the contents of this object.
        /// </returns>
        public override int GetHashCode()
        {
            int myHash = Id.GetHashCode(); // The ID is all that Equals checks!

            return myHash;
        }

        #endregion

        #region Static Public Methods and Operators

        /// <summary>
        /// Compares two Marker instances for equality.
        /// </summary>
        /// <param name="left">The Marker to the left of the operator</param>
        /// <param name="right">The Marker to the right of the operator</param>
        /// <returns>True if the two Markers are equal.</returns>
        public static bool operator ==(Marker left, Marker right)
        {
            // We have to check if left is null (right can be checked by Equals itself)
            if (object.ReferenceEquals(left, null))
            {
                // If right is also null, we're equal; otherwise, we're unequal!
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two Marker instances for inequality.
        /// </summary>
        /// <param name="left">The Marker to the left of the operator</param>
        /// <param name="right">The Marker to the right of the operator</param>
        /// <returns>True if the two Markers are not equal.</returns>
        public static bool operator !=(Marker left, Marker right)
        {
            // We have to check if left is null (right can be checked by Equals itself)
            if (object.ReferenceEquals(left, null))
            {
                // If right is also null, we're equal; otherwise, we're unequal!
                return ! object.ReferenceEquals(right, null);
            }
            return ! left.Equals(right);
        }

        /// <summary>
        /// Compares if one Marker instance should sort less than another.
        /// </summary>
        /// <param name="left">The Marker to the left of the operator</param>
        /// <param name="right">The Marker to the right of the operator</param>
        /// <returns>True if the Marker to the left should sort less than the Marker to the right.</returns>
        public static bool operator <(Marker left, Marker right)
        {
            return (left.CompareTo(right) < 0);
        }

        /// <summary>
        /// Compares if one Marker instance should sort greater than another.
        /// </summary>
        /// <param name="left">The Marker to the left of the operator</param>
        /// <param name="right">The Marker to the right of the operator</param>
        /// <returns>True if the Marker to the left should sort greater than the Marker to the right.</returns>
        public static bool operator >(Marker left, Marker right)
        {
            return (left.CompareTo(right) > 0);
        }

        #endregion

    }
}
