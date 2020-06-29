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
    /// Base class for file extents as non-nestable outer containers.
    /// </summary>
    public class FileExtentBase : CollectingExtentBase
    {
        /// <summary>
        /// Initialize the base of a (derived) file extent object for a given preamble (typically read from a data source).
        /// </summary>
        /// <param name="preamble">The extent preamble identifying the type and other parameters of the extent.</param>
        protected internal FileExtentBase(ExtentPreamble preamble)
            : base(preamble)
        {
        }

        /// <summary>
        /// Create a new file extent (of a particular type and major version) which needs to be filled out with content.
        /// </summary>
        /// <param name="typeCode">The ExtentTypeCode for this (derived) type of file extent.</param>
        /// <param name="majorVersion">The major version number of the format of this file extent.</param>
        protected internal FileExtentBase(ExtentTypeCode typeCode, short majorVersion)
            : base(typeCode, majorVersion)
        {
        }

        /// <summary>
        /// Get or set the offset (from the start of the preamble) to the first contained child extent.
        /// </summary>
        public override long ChildOffset
        {
            get { return Preamble.Offset; }
            internal set { Preamble.Offset = value; }
        }
    }
}
