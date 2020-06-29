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
    /// A LogicNode operating on a dataset instance (or on a key to a logical dataset) of type D.
    /// </summary>
    /// <typeparam name="D">The type of dataset instances (or of a key to a logical dataset).</typeparam>
    public interface ILogicNode<D>
    {
        /// <summary>
        /// Evaluate this ILogicNode&lt;D&gt; on a data set represented by a key of type D.
        /// </summary>
        /// <remarks>The dataset parameter is merely passed to the configured GetDataDelegate, and so can also be a key
        /// from which the configured delegate knows how to find the desired data value for the indicated dataset instance
        /// represented by that key.</remarks>
        /// <param name="dataset">A dataset instance (or key to a logical dataset) to test for a match.</param>
        /// <returns>True if the dataset instance matches on this LogicNode, false if it does not match.</returns>
        bool EvaluateNode(D dataset);

        /// <summary>
        /// Return a shallow stand-alone copy of this ILogicNode&lt;D&gt; with the same local behavior.
        /// </summary>
        /// <returns>A new node with the same local behavior but with no connections to other nodes.</returns>
        ILogicNode<D> Clone();

        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is true, or null to conclude with true.
        /// </summary>
        ILogicNode<D> NextOnTrue { get; set; }

        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is false, or null to conclude with false.
        /// </summary>
        ILogicNode<D> NextOnFalse { get; set; }
    }
}
