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
    /// Abstract base class for extents which hold a collection of extents
    /// </summary>
    abstract public class CollectingExtentBase : ExtentBase
    {
        private readonly List<ExtentBase> m_ExtentCollection = new List<ExtentBase>();

        /// <summary>
        /// Initialize the base of a (derived) collecting extent object for a given preamble (typically read from a data source).
        /// </summary>
        /// <param name="preamble">The extent preamble identifying the type and other parameters of the extent.</param>
        protected internal CollectingExtentBase(ExtentPreamble preamble)
            : base(preamble)
        {
        }

        /// <summary>
        /// Create a new collecting extent (of a particular type and major version) which needs to be filled out with content.
        /// </summary>
        /// <param name="typeCode">The ExtentTypeCode for this (derived) type of collection extent.</param>
        /// <param name="majorVersion">The major version number of the format of this collection extent.</param>
        protected internal CollectingExtentBase(ExtentTypeCode typeCode, short majorVersion)
            : base(typeCode, majorVersion)
        {
        }

        /// <summary>
        /// Get or set the offset (from the start of the preamble) to the first contained child extent.
        /// </summary>
        abstract public long ChildOffset { get; internal set; }

        // TODO: Needs methods to add/etc the collection.
    }
}
