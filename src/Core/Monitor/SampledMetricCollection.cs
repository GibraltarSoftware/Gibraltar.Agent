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
namespace Gibraltar.Monitor
{
    /// <summary>
    /// A collection of sampled metrics, keyed by their unique ID and name
    /// </summary>
    /// <remarks>A metric has a unique ID to identify a particular instance of the metric (associated with one session) 
    /// and a name that is unique within a session but is designed for comparison of the same metric between sessions.</remarks>
    public class SampledMetricCollection : MetricCollection
    {
        /// <summary>
        /// Create a new sampled metric dictionary for the provided definition.
        /// </summary>
        /// <remarks>This dictionary is created automatically by the Metric Definition during its initialization.</remarks>
        /// <param name="metricDefinition">The definition of the sampled metric to create a metric dictionary for</param>
        internal SampledMetricCollection(SampledMetricDefinition metricDefinition)
            : base(metricDefinition)
        {

        }
    }
}
