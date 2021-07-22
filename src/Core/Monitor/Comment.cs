
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
using System.Globalization;
using System.Windows.Forms;
using Gibraltar.Monitor.Internal;

#endregion

namespace Gibraltar.Monitor
{
    /// <summary>
    /// A single user or computer generated comment
    /// </summary>
    public class Comment : IDisplayable, IComparable<Comment>, IEquatable<Comment>
    {
        private readonly CommentPacket m_Packet;

        /// <summary>
        /// Create a new empty comment with the current user's identity
        /// </summary>
        public Comment()
        {
            m_Packet = new CommentPacket();

            //create a new GUID and assign the timestamp
            m_Packet.Timestamp = DateTimeOffset.Now; //we convert to UTC during serialization, we want local time.
            m_Packet.Id = Guid.NewGuid();
            m_Packet.UserName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                                              SystemInformation.UserDomainName, SystemInformation.UserName);
            m_Packet.Caption = string.Empty;
            m_Packet.Description = string.Empty;
        }

        /// <summary>
        /// Create a new comment with the specified caption and description and the current user's identity
        /// </summary>
        /// <param name="caption">The short display caption for this comment (entered by the user)</param>
        /// <param name="description">The body of the comment</param>
        public Comment(string caption, string description)
            : this()
        {
            //set our caption & description
            m_Packet.Caption = caption;
            m_Packet.Description = description;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The unique Id of this comment
        /// </summary>
        public Guid Id
        {
            get { return m_Packet.Id; }
        }

        /// <summary>
        /// The short display caption for this comment (entered by the user)
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
        /// The body of the comment
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
        /// The date and time the comment was created
        /// </summary>
        public DateTimeOffset TimeStamp
        {
            get { return m_Packet.Timestamp; }
        }

        /// <summary>
        /// The fully qualified user name (DOMAIN\USER) of the user that created the comment.
        /// </summary>
        public string UserName
        {
            get { return m_Packet.UserName; }
        }

        /// <summary>
        /// Compares the value of this instance to a specified Comment object and indicates whether this instance is earlier than, the same as, or later than the specified Comment.
        /// </summary>
        /// <remarks>Returns zero of the two instances are equivalent data objects, less than zero if this instance is sorted before the provided object,
        /// and more than zero if this instances is sorted after the provided object.</remarks>
        /// <param name="other">The comment to compare with this instance</param>
        /// <returns>A signed number indicating the relative values of this instance and other.</returns>
        public int CompareTo(Comment other)
        {
            // the objects are only the same if the GUIDs are the same.  Check that quickly
            if (Id == other.Id)
            {
                return 0;
            }

            // otherwise, lets do this the long way.  We need to support sorting
            int compareResult;

            // primary sort is by timestamp
            compareResult = TimeStamp.CompareTo(other.TimeStamp);

            if (compareResult == 0)
            {
                // well, timestamps are the same but this isn't the same comment.  Sort by caption alpha.
                // Case-sensitive InvariantCulture compares first ignoring case, then factors case in (lowercase earlier).
                // So differences only in case will sort near each other, which is probably what we want.
                compareResult = string.Compare(Caption, other.Caption, StringComparison.InvariantCulture);

                if (compareResult == 0)
                {
                    // really odd.  Same timestamp, same caption. Compare descriptions.
                    compareResult = string.Compare(Description, other.Description, StringComparison.InvariantCulture);

                    // at this point if they're still the same, we have only one more thing to compare.  If it's the same, they really are the same.
                    if (compareResult == 0)
                    {
                        compareResult = string.Compare(UserName, other.UserName, StringComparison.InvariantCulture);
                    }
                }
            }

            return compareResult;
        }

        /// <summary>
        /// Compares the value of this instance to a specified comment object and indicates whether they represent the same data
        /// </summary>
        /// <param name="other">The comment to compare with this instance</param>
        /// <returns>True if the objects represent the same original data.</returns>
        public bool Equals(Comment other)
        {
            // Careful, it could be null; check it without recursion
            if (object.ReferenceEquals(other, null))
            {
                return false; // Since we're a live object we can't be equal to a null instance.
            }

            // they are the same if their guid's match
            return (m_Packet.Id == other.Id);
        }

        /// <summary>
        /// Compares the value of this instance to a specified object and indicates whether they represent the same data
        /// </summary>
        /// <param name="obj">The object to compare with this instance</param>
        /// <returns>True if the object is a Comment object and represents the same original data as this.</returns>
        public override bool Equals(object obj)
        {
            Comment otherComment = obj as Comment;

            return Equals(otherComment); // Just have type-specific Equals do the check (it even handles null)
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
            int myHash = m_Packet.Id.GetHashCode(); // Packet ID is all that Equals checks!

            return myHash;
        }

        #endregion

        #region Static Public Methods and Operators
        
        /// <summary>
        /// Compares two Comment instances for equality.
        /// </summary>
        /// <param name="left">The Comment to the left of the operator</param>
        /// <param name="right">The Comment to the right of the operator</param>
        /// <returns>True if the two Comments are equal.</returns>
        public static bool operator==(Comment left, Comment right)
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
        /// Compares two Comment instances for inequality.
        /// </summary>
        /// <param name="left">The Comment to the left of the operator</param>
        /// <param name="right">The Comment to the right of the operator</param>
        /// <returns>True if the two Comments are not equal.</returns>
        public static bool operator!=(Comment left, Comment right)
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
        /// Compares if one Comment instance should sort less than another.
        /// </summary>
        /// <param name="left">The Comment to the left of the operator</param>
        /// <param name="right">The Comment to the right of the operator</param>
        /// <returns>True if the Comment to the left should sort less than the Comment to the right.</returns>
        public static bool operator<(Comment left, Comment right)
        {
            return (left.CompareTo(right) < 0);
        }

        /// <summary>
        /// Compares if one Comment instance should sort greater than another.
        /// </summary>
        /// <param name="left">The Comment to the left of the operator</param>
        /// <param name="right">The Comment to the right of the operator</param>
        /// <returns>True if the Comment to the left should sort greater than the Comment to the right.</returns>
        public static bool operator>(Comment left, Comment right)
        {
            return (left.CompareTo(right) > 0);
        }
        
        #endregion
    }

}
