
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// Base class for collection extent classes as nestable containers.
    /// </summary>
    public class CollectionExtentBase : CollectingExtentBase
    {
        private long m_ChildOffset;

        /// <summary>
        /// Initialize the base of a (derived) collection extent object for a given preamble (typically read from a data source).
        /// </summary>
        /// <param name="preamble">The extent preamble identifying the type and other parameters of the extent.</param>
        protected internal CollectionExtentBase(ExtentPreamble preamble)
            : base(preamble)
        {
        }

        /// <summary>
        /// Create a new collection extent (of a particular type and major version) which needs to be filled out with content.
        /// </summary>
        /// <param name="typeCode">The ExtentTypeCode for this (derived) type of collection extent.</param>
        /// <param name="majorVersion">The major version number of the format of this collection extent.</param>
        protected internal CollectionExtentBase(ExtentTypeCode typeCode, short majorVersion)
            : base(typeCode, majorVersion)
        {
        }

        /// <summary>
        /// Get or set the offset (from the start of the preamble) to the first contained child extent.
        /// </summary>
        public override long ChildOffset
        {
            get { return m_ChildOffset; }
            internal set { m_ChildOffset = value; }
        }
    }
}
