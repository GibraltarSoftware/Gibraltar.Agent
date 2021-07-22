
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete]
    public sealed class CollectionExtentPreamble : ExtentPreamble
    {
        private const int PositionOfChildOffset = ExtentPreambleLength;
        private int m_ChildOffset;

        /// <summary>
        /// The fixed byte-length of a collection extent preamble.
        /// </summary>
        public const int CollectionExtentPreambleLength = PositionOfChildOffset + sizeof(int);

        private const int CollectionExtentExtraLength = CollectionExtentPreambleLength - ExtentPreambleLength;

        /// <summary>
        /// Create a new ExtentPreamble from scratch, to be filled out and eventually written to a buffer or stream.
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="majorVersion"></param>
        internal CollectionExtentPreamble(ExtentTypeCode typeCode, short majorVersion)
            : base(typeCode, majorVersion)
        {
        }

        /// <summary>
        /// Read a CollectionExtentPreamble from the beginning of a source byte-array (of sufficient length).
        /// </summary>
        /// <param name="sourceArray"></param>
        public CollectionExtentPreamble(byte[] sourceArray)
            : base(sourceArray) // Read our full preamble and initialize the common base parts.
        {
            Debug.Assert(m_MemoryBuffer.Position == PositionOfChildOffset);

            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_ChildOffset); // Parse our additional field.
        }
 
        /// <summary>
        /// Read an ExtentPreamble from the current position in a provided stream (with sufficient data).
        /// Leaves stream positioned at the end of the basic preamble.
        /// </summary>
        /// <param name="sourceStream"></param>
        public CollectionExtentPreamble(Stream sourceStream)
            : base(sourceStream) // Read our full preamble and initialize the common base parts.
        {
            Debug.Assert(m_MemoryBuffer.Position == PositionOfChildOffset);

            BinarySerializer.DeserializeValue(m_MemoryBuffer, out m_ChildOffset); // Parse our additional field.
        }

        /// <summary>
        /// Get the full byte-length of the preamble for this (possibly derived) object.
        /// </summary>
        /// <returns></returns>
        protected override int GetMemoryBufferLength()
        {
            return CollectionExtentPreambleLength;
        }

        /// <summary>
        /// Get or set the offset (from the start of this extent's preamble) of our first contained child extent.
        /// </summary>
        public int ChildOffset
        {
            get { return m_ChildOffset; }
            internal set
            {
                if (m_ChildOffset == value)
                    return; // Already set to that value.

                m_ChildOffset = value;
                UpdateBuffer(PositionOfChildOffset, BinarySerializer.SerializeValue(m_ChildOffset));
                SetDirty();
            }
        }
    }
}
