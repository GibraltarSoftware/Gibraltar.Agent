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
    /// A suggested interval between value samples.
    /// </summary>
    public enum SamplingInterval
    {
        /// <summary>
        /// Use the interval as the data was recorded.
        /// </summary>
        Default = Loupe.Extensibility.Data.MetricSampleInterval.Default,

        /// <summary>
        /// Use the interval as the data was recorded.
        /// </summary>
        Shortest = Loupe.Extensibility.Data.MetricSampleInterval.Shortest,

        /// <summary>
        /// Use a sampling interval set in milliseconds
        /// </summary>
        Millisecond = Loupe.Extensibility.Data.MetricSampleInterval.Millisecond,

        /// <summary>
        /// Use a sampling interval set in seconds.
        /// </summary>
        Second = Loupe.Extensibility.Data.MetricSampleInterval.Second,

        /// <summary>
        /// Use a sampling interval set in minutes.
        /// </summary>
        Minute = Loupe.Extensibility.Data.MetricSampleInterval.Minute,

        /// <summary>
        /// Use a sampling interval set in hours.
        /// </summary>
        Hour = Loupe.Extensibility.Data.MetricSampleInterval.Hour,

        /// <summary>
        /// Use a sampling interval set in days.
        /// </summary>
        Day = Loupe.Extensibility.Data.MetricSampleInterval.Day,

        /// <summary>
        /// Use a sampling interval set in weeks.
        /// </summary>
        Week = Loupe.Extensibility.Data.MetricSampleInterval.Week,

        /// <summary>
        /// Use a sampling interval set in months.
        /// </summary>
        Month = Loupe.Extensibility.Data.MetricSampleInterval.Month,
    }
}
