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
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// Base class for extent classes which know how to serialize and deserialize particular types of extents.
    /// </summary>
    public class ExtentBase
    {
        private readonly ExtentPreamble m_Preamble;

        /// <summary>
        /// Initialize the base of a (derived) extent object for a given preamble (typically read from a data source).
        /// </summary>
        /// <param name="preamble">The extent preamble identifying the type and other parameters of the extent.</param>
        protected internal ExtentBase(ExtentPreamble preamble)
        {
            m_Preamble = preamble;
        }

        /// <summary>
        /// Create a new extent (of a particular type and major version) which needs to be filled out with content.
        /// </summary>
        /// <param name="typeCode">The ExtentTypeCode for this (derived) type of extent.</param>
        /// <param name="majorVersion">The major version number of the format of this extent.</param>
        protected internal ExtentBase(ExtentTypeCode typeCode, short majorVersion)
        {
            m_Preamble = new ExtentPreamble(typeCode, majorVersion);
        }

        /// <summary>
        /// The preamble of this extent.
        /// </summary>
        protected ExtentPreamble Preamble { get { return m_Preamble; } }

        /// <summary>
        /// Get the ExtentTypeCode for this extent.
        /// </summary>
        public ExtentTypeCode TypeCode { get { return m_Preamble.TypeCode; } }

        /// <summary>
        /// Get the major version number of the format for this extent.
        /// </summary>
        public short MajorVersion { get { return m_Preamble.MajorVersion; } }

        /// <summary>
        /// Get or set the minor version number of the format for this extent.
        /// </summary>
        public short MinorVersion
        {
            get { return m_Preamble.MinorVersion; }
            internal set { m_Preamble.MinorVersion = value; }
        }

        /// <summary>
        /// Get or set the offset (from the start of the preamble) to the next (peer) extent.
        /// </summary>
        public long Offset
        {
            get { return m_Preamble.Offset; }
            internal set { m_Preamble.Offset = value; }
        }

        /// <summary>
        /// Get or set the checksum value of this extent (from start of preamble until the next peer, with checksum field
        /// zeroed out).  A value of 0 will disable checksum comparison.
        /// </summary>
        public int Checksum
        {
            get { return m_Preamble.Checksum; }
            internal set { m_Preamble.Checksum = value; }
        }
    }
}
