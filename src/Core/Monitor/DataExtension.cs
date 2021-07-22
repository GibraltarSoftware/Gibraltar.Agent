
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
using Gibraltar.Monitor.Internal;

#endregion

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Extends another object to provide comments and markers.
    /// </summary>
    public class DataExtension : IEquatable<DataExtension>
    {
        private readonly Analysis m_Analysis;
        private readonly Guid m_ID;
        private readonly DataMarkerCollection m_Markers;
        private readonly CommentCollection m_Comments;

        /// <summary>
        /// Create a new data extension object for the specified analysis and data object owner.
        /// </summary>
        /// <param name="analysis">The analysis that will own this data extension</param>
        /// <param name="owner">The data object extended by this object.</param>
        internal DataExtension(Analysis analysis, IDataObject owner)
        {
            m_Analysis = analysis;
            m_ID = owner.Id;
            m_Markers = new DataMarkerCollection(m_Analysis);
            m_Comments = new CommentCollection();
        }

        internal DataExtension(Analysis analysis, DataExtensionPacket packet, CommentCollection comments)
        {
            m_Analysis = analysis;
            m_Comments = comments;
            m_ID = packet.ID;

            //we have to be sure we have the markers and comments collections, because 
            if (m_Comments == null)
                m_Comments = new CommentCollection();

            if (m_Markers == null)
                m_Markers = new DataMarkerCollection(m_Analysis);
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
        /// The unique Id of this data extension object.  This is also the unique Id of the object this data extension extends.
        /// </summary>
        public Guid Id
        {
            get { return m_ID; }
        }

        /// <summary>
        /// The collection of markers associated with this data extension object.
        /// </summary>
        public DataMarkerCollection Markers
        {
            get
            {
                return m_Markers;
            }
        }

        /// <summary>
        /// The collection of comments associated with this data extension object.
        /// </summary>
        public CommentCollection Comments
        {
            get
            {
                return m_Comments;
            }
        }

        /// <summary>
        /// Compares this data extension object with the provided object and indicates if they represent the same data.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DataExtension other)
        {
            //Careful - can be null
            if (other == null)
            {
                return false; // since we're a live object we can't be equal.
            }

            return (m_ID == other.Id);
        }

        #endregion

        #region Internal Properties and Methods

        /// <summary>
        /// The raw data extension packet for this object.
        /// </summary>
        internal DataExtensionPacket Packet
        {
            get
            {
                //create a data exchange packet and pass it back
                return new DataExtensionPacket(m_ID);
            }
        }

        #endregion
    }
}
