
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// Base implementation of an extent factory to parse extents and instantiate extent objects to encapsulate them.
    /// (Must be inherited with knowledge of typecodes and extent Type to parse more than generic raw extents.)
    /// </summary>
    public class ExtentFactoryBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ExtentFactoryBase()
        {
        }

        /// <summary>
        /// Get the best derived extent object available to represent the extent starting at the current Position
        /// in the provided sourceStream.
        /// </summary>
        /// <param name="sourceStream">A readable Stream positioned at the start of an extent preamble to scan.</param>
        /// <returns>An extent object encapsulating the extent read from the stream, or null if none can be read.</returns>
        /// <exception cref="InvalidOperationException">Stream is not positioned at a valid extent preamble.</exception>
        public ExtentBase GetExtent(Stream sourceStream)
        {
            if (sourceStream == null || sourceStream.CanRead == false)
                return null;

            long positionOfExtent = sourceStream.Position;
            long availableDataLength = sourceStream.Length - positionOfExtent;
            if (availableDataLength < ExtentPreamble.ExtentPreambleLength)
                return null; // Or should we throw an exception?

            ExtentPreamble preamble = new ExtentPreamble(sourceStream);

            long typeCodeValidation = (long)preamble.TypeCode;
            typeCodeValidation >>= 40; // Shift down to the top three bytes;
            typeCodeValidation |= 0x2020; // Map the second and third bytes to lower-case ASCII;
            if (typeCodeValidation != 0x79677c) // Looking for 'ygl' prefix; add other possibilities when extended.
            {
                if (sourceStream.CanSeek)
                    sourceStream.Position = positionOfExtent;

                throw new InvalidOperationException("Source stream is not positioned at a valid extent preamble.");
            }
            ExtentBase extent = OnGetExtent(preamble, sourceStream);
            return extent;
        }

        /// <summary>
        /// Base implementation to get an extent.  Only knows how to create a generic extent with opaque data.
        /// </summary>
        /// <param name="preamble">The extent preamble identifying the type and other parameters of the extent.</param>
        /// <param name="sourceStream">A readable Stream (typically from which the preamble was read),
        /// now positioned at the end of the preamble.</param>
        /// <returns>An extent object encapsulating the extent read from the stream, or null if none can be read.</returns>
        protected virtual ExtentBase OnGetExtent(ExtentPreamble preamble, Stream sourceStream)
        {
            long positionOfData = sourceStream.Position;
            long availableDataLength = sourceStream.Length - positionOfData;
            
            // BUG: Not complete

            return null; // TODO: Create a generic Extent to read over this extent to the next peer.
        }
    }
}
