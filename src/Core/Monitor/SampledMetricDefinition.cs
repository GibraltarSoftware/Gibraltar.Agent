
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

using Gibraltar.Monitor.Internal;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor
{
    /// <summary>
    /// The base class for defining sampled metrics
    /// </summary>
    /// <remarks>
    /// A sampled metric always has a value for any timestamp between its start and end timestamps.
    /// It presumes any interim value by looking at the best fit sampling of the real world value
    /// and assuming it covers the timestamp in question.  It is therefore said to be contiguous for 
    /// the range of start and end.  Event metrics are only defined at the instant they are timestamped, 
    /// and imply nothing for other timestamps.  
    /// For event based metrics, use the EventMetricDefinition base class.</remarks>
    public abstract class SampledMetricDefinition : MetricDefinition//, ISampledMetricDefinition
    {
        /// <summary>
        /// Create a new sampled metric object from the provided raw data packet.
        /// </summary>
        /// <remarks>The metric definition <b>will</b> be automatically added to the provided collection.</remarks>
        /// <param name="definitions">The definitions dictionary this definition is a part of.</param>
        /// <param name="packet">The packet to create a definition from.</param>
        internal SampledMetricDefinition(MetricDefinitionCollection definitions, SampledMetricDefinitionPacket packet)
            : base(definitions, packet)
        {
            // After the base constructor, auto-add ourself to the definition collection
            SetReadOnly(); // Make sure we're read-only before we're added to the collection.
            definitions.Add(this); // ToDo: Determine whether to keep or discard this behavior for sampled metrics.
        }

        #region Public Properties and Methods

        /// <summary>
        /// The display caption for the calculated values captured under this metric
        /// </summary>
        public string UnitCaption
        {
            get
            {
                return ((SampledMetricDefinitionPacket)base.Packet).UnitCaption;
            }
            set
            {
                //the inner property is trimming, no need to do so here.
                ((SampledMetricDefinitionPacket)base.Packet).UnitCaption = value;
            }
        }

        /*
        /// <inheritdoc />
        public int CompareTo(ISampledMetricDefinition other)
        {
            //we let our base object do the compare, we're really just casting things
            return base.CompareTo(other);
        }

        /// <inheritdoc />
        public bool Equals(ISampledMetricDefinition other)
        {
            //We're really just a type cast, refer to our base object
            return base.Equals(other);
        }*/

        #endregion

    }
}
