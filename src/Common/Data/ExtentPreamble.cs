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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// The standard preamble marking the start of an extent.
    /// </summary>
    public class ExtentPreamble
    {
        private const int PositionOfTypeCode = 0;
        private readonly ExtentTypeCode m_TypeCode;

        private const int PositionOfMajorVersion = PositionOfTypeCode + sizeof(long);
        private readonly short m_MajorVersion;

        private const int PositionOfMinorVersion = PositionOfMajorVersion + sizeof(short);
        private short m_MinorVersion;

        private const int PositionOfOffset = PositionOfMinorVersion + sizeof(short);
        private long m_Offset; // Holding a Uint32 value.

        private const int PositionOfChecksum = PositionOfOffset + sizeof(int); // Offset only uses 4 bytes in the file.
        private int m_Checksum;

        private bool m_Dirty;

        /// <summary>
        /// The fixed byte-length of an extent preamble.
        /// </summary>
        public const int ExtentPreambleLength = PositionOfChecksum + sizeof(int);

        /// <summary>
        /// Lock object for modifying the preamble.
        /// </summary>
        protected readonly object m_Lock = new object();

        /// <summary>
        /// Internal memory buffer for this preamble.
        /// </summary>
        protected readonly MemoryStream m_MemoryBuffer;
        private readonly int m_MemoryBufferLength; // Cache of the (derived) buffer length.

        /// <summary>
        /// Fired when the preamble becomes dirty.
        /// </summary>
        public event EventHandler PreambleDirty;

        /// <summary>
        /// Initialize the internal buffer for the full size needed by this (possibly derived) object.
        /// THIS MUST BE CALLED FROM EACH OTHER BASE CONSTRUCTOR.
        /// </summary>
        private ExtentPreamble()
        {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            m_MemoryBufferLength = GetMemoryBufferLength(); // Get the derived-class's full size.  They use our one buffer.
// ReSharper restore DoNotCallOverridableMethodsInConstructor

            m_MemoryBuffer = new MemoryStream(m_MemoryBufferLength);
            UpdateBuffer(0, new byte[m_MemoryBufferLength], 0, m_MemoryBufferLength, true); // Push buffer length to complete.
        }

        /// <summary>
        /// Create a new ExtentPreamble from scratch, to be filled out and eventually written to a buffer or stream.
        /// </summary>
        /// <param name="typeCode">The ExtentTypeCode for the (derived) type of extent being created.</param>
        /// <param name="majorVersion">The major version number of the format of the extent being created.</param>
        internal ExtentPreamble(ExtentTypeCode typeCode, short majorVersion)
            : this()
        {
            m_TypeCode = typeCode;
            m_MajorVersion = majorVersion;
            m_Dirty = true; // Values aren't saved yet.

            UpdateBuffer(PositionOfTypeCode, BinarySerializer.SerializeValue((long)m_TypeCode));
            UpdateBuffer(PositionOfMajorVersion, BinarySerializer.SerializeValue(m_MajorVersion));
        }

        /// <summary>
        /// Read an ExtentPreamble from the beginning of a source byte-array (of sufficient length).
        /// </summary>
        /// <param name="sourceArray">A byte array from which to read the preamble (from start of array).</param>
        public ExtentPreamble(byte[] sourceArray)
            : this()
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));

            if (sourceArray.Length < ExtentPreambleLength)
                throw new ArgumentException(string.Format("The provided sourceArray is of insufficient length for a full {0}.",
                                                          GetType().Name), nameof(sourceArray));

            UpdateBuffer(0, sourceArray, 0, m_MemoryBufferLength, false); // Read in full derived-class's size.

            m_MemoryBuffer.Position = PositionOfTypeCode;
            long rawTypeCode;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out rawTypeCode);
            m_TypeCode = (ExtentTypeCode)rawTypeCode;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_MajorVersion);
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_MinorVersion);
            uint rawOffset;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out rawOffset);
            m_Offset = rawOffset;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_Checksum);

            // Leave m_MemoryBuffer pointed to end of basic preamble so derived classes can parse additional data.
        }

        /// <summary>
        /// Read an ExtentPreamble from the current position in a provided stream (with sufficient data).
        /// Leaves stream positioned at the end of the basic preamble.
        /// </summary>
        /// <param name="sourceStream">A readable Stream positioned at the start of an extent preamble to scan.</param>
        public ExtentPreamble(Stream sourceStream)
            : this()
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));

            byte[] byteBuffer = new byte[m_MemoryBufferLength];
            int readCount = sourceStream.Read(byteBuffer, 0, m_MemoryBufferLength); // Read in full derived-class's size.
            if (readCount < m_MemoryBufferLength)
                throw new EndOfStreamException(string.Format("The provided sourceStream ran out before the full {0} could be read.",
                                                             GetType().Name));

            UpdateBuffer(0, byteBuffer);
            
            m_MemoryBuffer.Position = PositionOfTypeCode;
            long rawTypeCode;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out rawTypeCode);
            m_TypeCode = (ExtentTypeCode)rawTypeCode;
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_MajorVersion);
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_MinorVersion);
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_Offset);
            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_Checksum);

            // Leave m_MemoryBuffer pointed to end of basic preamble so derived classes can parse additional data.
        }

        /// <summary>
        /// Get the byte-length of this preamble, for help in computing and interpreting offset pointers and extent contents.
        /// </summary>
        public int PreambleLength { get { return m_MemoryBufferLength; } }

        /// <summary>
        /// Get the ExtentTypeCode for this extent.
        /// </summary>
        public ExtentTypeCode TypeCode { get { return m_TypeCode; } }

        /// <summary>
        /// Get the major version number of the format for this extent.
        /// </summary>
        public short MajorVersion { get { return m_MajorVersion; } }

        /// <summary>
        /// Get or set the minor version number of the format for this extent.
        /// </summary>
        public short MinorVersion
        {
            get
            {
                lock (m_Lock)
                {
                    return m_MinorVersion;
                }
            }
            internal set
            {
                lock (m_Lock)
                {
                    if (m_MinorVersion == value)
                        return; // Already set to that value.

                    m_MinorVersion = value;
                    UpdateBuffer(PositionOfMinorVersion, BinarySerializer.SerializeValue(m_MinorVersion));
                }
                SetDirty(); // Outside the lock because it may fire an event.
            }
        }

        /// <summary>
        /// Get or set the offset (from the start of the preamble) to the next (peer) extent preamble.
        /// </summary>
        public long Offset
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Offset;
                }
            }
            internal set
            {
                if (value < 0 || value > UInt32.MaxValue)
                    throw new OverflowException(string.Format("Supplied Offset value {0} is outside the range for Uint32 values.", value));

                lock (m_Lock)
                {
                    if (m_Offset == value)
                        return; // Already set to that value.

                    m_Offset = value;
                    uint rawOffset = (uint)value;
                    UpdateBuffer(PositionOfOffset, BinarySerializer.SerializeValue(rawOffset));
                }
                SetDirty(); // Outside the lock because it may fire an event.
            }
        }

        /// <summary>
        /// Get or set the checksum value of this extent (from start of preamble until the next peer, with checksum field
        /// zeroed out).  A value of 0 will disable checksum comparison.
        /// </summary>
        public int Checksum
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Checksum;
                }
            }
            internal set
            {
                lock (m_Lock)
                {
                    if (m_Checksum != value)
                    {
                        m_Checksum = value;
                        UpdateBuffer(PositionOfChecksum, BinarySerializer.SerializeValue(m_Checksum));
                    }
                    ClearDirty(); // Assume if the checksum has been updated that recent changes are now accounted for.
                }
            }
        }

        /// <summary>
        /// Indicates whether the checksum needs to be recomputed including the modified preamble.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Dirty;
                }
            }
        }

        /// <summary>
        /// Export a copy of the full preamble as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            lock (m_Lock)
            {
                return m_MemoryBuffer.ToArray();
            }
        }

        /// <summary>
        /// Get the full byte-length of the preamble for this (possibly derived) object.
        /// MUST BE OVERRIDEN BY INHERITORS IF THEY ADD ADDITIONAL PREAMBLE FIELDS.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetMemoryBufferLength()
        {
            return ExtentPreambleLength;
        }

        /// <summary>
        /// Mark the preamble as dirty (checksum needs to be recomputed).
        /// </summary>
        protected void SetDirty()
        {
            lock (m_Lock)
            {
                if (m_Dirty)
                    return; // Already flagged as Dirty.

                m_Dirty = true;
            }
            OnPreambleDirty();
        }

        /// <summary>
        /// Clear the ExtentPreamble's Dirty flag (eg. after checksum has been recomputed with the modified preamble).
        /// </summary>
        protected void ClearDirty()
        {
            lock (m_Lock)
            {
                m_Dirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnPreambleDirty()
        {
            EventHandler tempEvent = PreambleDirty;
            if (tempEvent != null)
                tempEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="sourceArray"></param>
        protected void UpdateBuffer(int position, byte[] sourceArray)
        {
            UpdateBuffer(position, sourceArray, 0, sourceArray.Length, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="sourceArray"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="keepPosition"></param>
        protected void UpdateBuffer(int position, byte[] sourceArray, int startIndex, int count, bool keepPosition)
        {
            lock (m_Lock)
            {
                long savePosition = m_MemoryBuffer.Position;
                m_MemoryBuffer.Position = position;
                m_MemoryBuffer.Write(sourceArray, startIndex, count);
                if (keepPosition)
                    m_MemoryBuffer.Position = savePosition;
            }
        }
    }
}
