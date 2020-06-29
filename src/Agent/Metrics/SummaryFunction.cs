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
namespace Gibraltar.Agent.Metrics
{
    /// <summary>
    /// Indicates the default way to interpret multiple values for display purposes
    /// </summary>
    public enum SummaryFunction
    {
        /// <summary>
        /// Average all of the values within each sample range to determine the displayed value.
        /// </summary>
        Average = Loupe.Extensibility.Data.EventMetricValueTrend.Average,

        /// <summary>
        /// Add all of the values within each sample range to determine the displayed value.
        /// </summary>
        Sum = Loupe.Extensibility.Data.EventMetricValueTrend.Sum,

        /// <summary>
        /// An average of all values up through the end of the sample range.
        /// </summary>
        RunningAverage = Loupe.Extensibility.Data.EventMetricValueTrend.RunningAverage,

        /// <summary>
        /// The sum of all values up through the end of the sample range.
        /// </summary>
        RunningSum = Loupe.Extensibility.Data.EventMetricValueTrend.RunningSum,

        /// <summary>
        /// The number of values within each sample range.
        /// </summary>
        Count = Loupe.Extensibility.Data.EventMetricValueTrend.Count
    }
}
