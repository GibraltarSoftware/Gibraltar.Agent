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
using System.Diagnostics;
using System.IO;

#endregion File Header

namespace Gibraltar.Data
{
    /// <summary>
    /// An index update that can be queued to disk.
    /// </summary>
    public class IndexUpdate : IComparable<IndexUpdate>, IComparable
    {
        private readonly byte[] m_SessionHeaderRawData;
        private readonly string m_FragmentFileNamePath;
        private readonly int m_FragmentFileSize;
        private readonly bool m_SessionClosed;

        private Guid m_FileId; //so we don't have to keep rehydrating the session header to get it.
        private SessionHeader m_SessionHeader;
        private byte[] m_SerializedFragmentFileNamePath;
        private bool m_SessionStillActive;

        /// <summary>
        /// Create a new index update with the provided session fragment information.
        /// </summary>
        /// <param name="sessionHeader">The session header of a valid session fragment</param>
        /// <param name="fragmentFileNamePath">The full file name &amp; path to the fragment to be added to the index</param>
        /// <param name="fragmentFileSize">The size in bytes of the fragment file</param>
        /// <param name="sessionClosed">True if the session has been marked as successfully closed (therefore exiting normally)</param>
        public IndexUpdate(SessionHeader sessionHeader, string fragmentFileNamePath, int fragmentFileSize, bool sessionClosed)
        {
            if (sessionHeader == null)
                throw new ArgumentNullException(nameof(sessionHeader));

            if (string.IsNullOrEmpty(fragmentFileNamePath))
                throw new ArgumentNullException(nameof(fragmentFileNamePath));

#if DEBUG
            //lets test the session header.
            SessionHeader testHeader = new SessionHeader(sessionHeader.RawData());
            if (testHeader.IsValid == false)
            {
                //we can't be used - we will be rejected
                throw new ArgumentException("The session header could not be serialized successfully.  This indicates an internal code error in Loupe", nameof(sessionHeader));
            }
#endif
            //we want to "fix" the session header, so grab its raw data now.
            m_SessionHeaderRawData = sessionHeader.RawData();
            m_FileId = sessionHeader.FileId; //LOWER case sessionHeader, not our property!
            m_FragmentFileNamePath = fragmentFileNamePath;
            m_FragmentFileSize = fragmentFileSize;
            m_SessionClosed = sessionClosed;
        }
        
        /// <summary>
        /// Create a new index update by reading the provided byte array
        /// </summary>
        /// <param name="data"></param>
        /// <remarks>Will throw an exception if the provided data is not a complete, valid update.</remarks>
        public IndexUpdate(byte[] data)
        {
            //we need the input data buffer to be the right size to interpret.
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            //first pull off our stuff.
            MemoryStream rawData = new MemoryStream(data);

            BinarySerializer.DeserializeValue(rawData, out m_FragmentFileNamePath);

            BinarySerializer.DeserializeValue(rawData, out m_FragmentFileSize);

            BinarySerializer.DeserializeValue(rawData, out m_SessionClosed);

            //now the remainder is the session header, carve that off.
            Byte[] sessionHeaderRawData = new byte[rawData.Length - rawData.Position];
            rawData.Read(sessionHeaderRawData, 0, sessionHeaderRawData.Length);

            m_SessionHeaderRawData = sessionHeaderRawData;

            //and if we got here and what we have isn't valid, go no further.
            if (SessionHeader.IsValid == false) //the session header property loads up the session header raw data.
            {
                throw new GibraltarException("The index update was deserialized but didn't pass validation, indicating it has been corrupted in some way.");
            }

            m_FileId = SessionHeader.FileId;
        }

        /// <summary>
        /// The session header information
        /// </summary>
        public SessionHeader SessionHeader
        {
            get
            {
                if (m_SessionHeader == null)
                {
                    /*
                     This will cause deadlocks in many situations so don't even do it in debug.
                    Debug.Print("Rehydrating session header in index update.  This shouldn't happen during write operations - just during maintenance.");
                    */

                    //we need to rehydrate our byte data to make it.
                    m_SessionHeader = new SessionHeader(m_SessionHeaderRawData);
                }

                return m_SessionHeader;
            }
        }

        /// <summary>
        /// Indicates if the session has been closed or not
        /// </summary>
        /// <remarks>Updates are still possible on a closed session; this should be used to determine if the
        /// session is considered closed normally or still running.</remarks>
        public bool SessionClosed { get { return m_SessionClosed; } }

        /// <summary>
        /// Gets or sets a flag indicating that the session is still active (holds a lock) despite SessionClosed.
        /// (Not determined automatically; it's up to the owner to manage this flag.)
        /// </summary>
        public bool SessionStillActive { get { return m_SessionStillActive; } set { m_SessionStillActive = value; } }

        /// <summary>
        /// The full file name and path to the session fragment file
        /// </summary>
        public string FragmentFileNamePath
        {
            get { return m_FragmentFileNamePath; }
        }

        /// <summary>
        /// The size in bytes of the session fragment file.
        /// </summary>
        public int FragmentFileSize { get { return m_FragmentFileSize; } }

        /// <summary>
        /// The unique id of the file the session header is associated with
        /// </summary>
        public Guid FileId { get { return m_FileId; } }

        /// <summary>
        /// Indicates if the index update is complete and valid or not.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (m_SessionHeaderRawData == null)
                    return false;

                if (string.IsNullOrEmpty(m_FragmentFileNamePath))
                    return false;

                return SessionHeader.IsValid;
            }
        }

        /// <summary>
        /// Serialize the entire index update to a byte array.
        /// </summary>
        /// <returns></returns>
        public Byte[] RawData()
        {
            MemoryStream rawData = new MemoryStream(2048);

            //we put our stuff on first because there's no current way to find out how much the session header used.
            if (m_SerializedFragmentFileNamePath == null)
            {
                m_SerializedFragmentFileNamePath = BinarySerializer.SerializeValue(m_FragmentFileNamePath);
            }

            rawData.Write(m_SerializedFragmentFileNamePath, 0, m_SerializedFragmentFileNamePath.Length);

            byte[] curValue = BinarySerializer.SerializeValue(m_FragmentFileSize);
            rawData.Write(curValue, 0, curValue.Length);

            curValue = BinarySerializer.SerializeValue(m_SessionClosed);
            rawData.Write(curValue, 0, curValue.Length);

            //short cut:  If we were started from a byte array just use it directly.
            if (m_SessionHeaderRawData != null)
            {
                rawData.Write(m_SessionHeaderRawData, 0, m_SessionHeaderRawData.Length);
            }
            else
            {
                Debug.Assert(m_SessionHeader != null);

                curValue = m_SessionHeader.RawData();
                rawData.Write(curValue, 0, curValue.Length);
            }

            return rawData.ToArray();
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This object is less than the <paramref name="other"/> parameter.
        ///                     Zero 
        ///                     This object is equal to <paramref name="other"/>. 
        ///                     Greater than zero 
        ///                     This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public int CompareTo(IndexUpdate other)
        {
            //sort first by session ID then by fragment.
            int compareResult = SessionHeader.Id.CompareTo(other.SessionHeader.Id);

            if (compareResult == 0)
            {
                //same so far, so go deeper.  How about fragment sequence?
                compareResult = SessionHeader.FileSequence.CompareTo(other.SessionHeader.FileSequence);
            }

            if (compareResult == 0)
            {
                //same so far - the one with the last end date time is later.
                compareResult = SessionHeader.EndDateTime.CompareTo(other.SessionHeader.EndDateTime);
            }

            return compareResult;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This instance is less than <paramref name="obj"/>. 
        ///                     Zero 
        ///                     This instance is equal to <paramref name="obj"/>. 
        ///                     Greater than zero 
        ///                     This instance is greater than <paramref name="obj"/>. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. 
        ///                 </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. 
        ///                 </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as IndexUpdate);
        }
    }
}