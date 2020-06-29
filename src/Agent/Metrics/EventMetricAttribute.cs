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

using System;

#endregion File Header

namespace Gibraltar.Agent.Metrics
{
    /// <summary>
    /// Define an event metric with value columns from the members of the current object.
    /// </summary>
    /// <remarks><para>An object (class, struct, or interface) can be decorated with this attribute to define an event metric
    /// for it.  Use the EventMetricValue attribute to designate which direct members (properties, fields, or zero-argument
    /// methods) should be stored as value columns each time the event metric is sampled.  The EventMetricInstanceName
    /// attribute can optionally be used on a member (typically not one also chosen as a value column, but it is allowed
    /// to be) to designate that member to automatically provide the instance name when sampling the object for this
    /// defined event metric.</para>
    /// <para>Only one event metric (containing any number of value columns) can be defined on a specific class, struct,
    /// or interface.  However, using interfaces to define event metrics can allow a single object to support multiple
    /// event metric types through those separate interfaces.  Such advanced tricks may require selection of a specific
    /// event metric definition by type (e.g. by typeof a particular interface) in order to sample each possible event
    /// metric as desired for that object.  Selection of a definition by a specific type may also be required when sampling
    /// an inheritor object, to ensure the desired event metric is identified and sampled as appropriate, because multiple
    /// event metrics defined on a complex object can not be assumed to all be appropriate to sample every time.</para></remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class EventMetricAttribute : Attribute
    {
        private readonly string m_Namespace;
        private readonly string m_CategoryName;
        private readonly string m_CounterName;

        private string m_Caption;
        private string m_Description;

        /// <summary>
        /// Define an event metric with value columns to be selected from the direct members of the current object.
        /// </summary>
        /// <param name="metricsSystem">The metrics capture system label the user has selected to distinguish all metrics they define, to avoid colliding with usage by other libraries.</param>
        /// <param name="metricCategoryName">A dot-delimited categorization for the metric under the metrics system.</param>
        /// <param name="counterName">The name of the metric to be defined under the metric category name.</param>
        public EventMetricAttribute(string metricsSystem, string metricCategoryName, string counterName)
        {
            m_Namespace = string.IsNullOrEmpty(metricsSystem) ? metricsSystem : metricsSystem.Trim();
            m_CategoryName = string.IsNullOrEmpty(metricCategoryName) ? metricCategoryName : metricCategoryName.Trim();
            m_CounterName = string.IsNullOrEmpty(counterName) ? counterName : counterName.Trim();
        }

        /// <summary>
        /// The metrics capture system label the user has selected to distinguish all metrics they define, to avoid colliding with usage by other libraries.
        /// </summary>
        public string MetricsSystem
        {
            get { return m_Namespace; }
        }

        /// <summary>
        /// A dot-delimited categorization for the metric under the metrics system.
        /// </summary>
        public string MetricCategoryName
        {
            get { return m_CategoryName; }
        }

        /// <summary>
        /// The name of the metric to be defined under the metric category name.
        /// </summary>
        public string CounterName
        {
            get { return m_CounterName; }
        }

        /// <summary>
        /// A displayable caption for this event metric definition.
        /// </summary>
        public string Caption
        {
            get { return m_Caption; }
            set { m_Caption = string.IsNullOrEmpty(value) ? value : value.Trim(); }
        }

        /// <summary>
        /// An end-user description of this metric definition.
        /// </summary>
        public string Description
        {
            get { return m_Description; }
            set { m_Description = string.IsNullOrEmpty(value) ? value : value.Trim(); }
        }
    }

    // ToDo: Pull this out after testing, because Kendall believes it would cause more difficulty than it helps.
    /// <summary>
    /// Specify a named value column as the default one to graph for the event metric defined on this object type.
    /// </summary>
    /// <remarks>The current object must also have the EventMetric attribute to define an event, and the column name
    /// specified in this attribute must match the column name specified in an EventMetricValue attribute on a member of
    /// the current object defining a value column.  If present and valid, this attribute supersedes the IsDefaultColumn
    /// property of the EventMetricValue attributes (otherwise, that property is used to select a default column to graph).
    /// If no value column is designated as the default, the count of events (of this event metric definition and a
    /// particular metric instance name) will be used as a default way to graph the event metric.</remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class EventMetricDefaultColumnAttribute : Attribute
    {
        private readonly string m_ColumnName;

        /// <summary>
        /// Designate a named value column within this event as the default column to graph for this event metric.
        /// </summary>
        /// <param name="columnName">The name of a value column (e.g. added with the EventMetricValue attribute on a member).</param>
        public EventMetricDefaultColumnAttribute(string columnName)
        {
            m_ColumnName = string.IsNullOrEmpty(columnName) ? columnName : columnName.Trim();
        }

        /// <summary>
        /// The name of a value column within this event to designate as the default column to graph for this event metric.
        /// </summary>
        public string ColumnName
        {
            get { return m_ColumnName; }
        }
    }

    /// <summary>
    /// Indicates which field, property, or method should be used to determine the category name for the event metric.
    /// </summary>
    /// <remarks>The current object must also have the EventMetric attribute defined.  Only one field, property, or method in an object
    /// can have this attribute defined.  Whatever value is returned will be converted to a string to be the category name to the metric.
    /// If no item on an object has this attribute defined, the attribute value from the EventMetric attribute will be used.</remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class EventMetricCategoryNameAttribute : Attribute
    {
    }

    // Note: These two attributes are currently internal to prevent client use because they are not presently supported.
    // But we might consider adding this capability in the future, if we can figure out how to sensibly support it.
    // For now, it seems too problematic, and the hypothetical benefits seem to be achievable with clever usage of the
    // instance name (e.g. using their own dot-delimited hierarchy in the instance name), which we could more easily
    // support analysis across multiple metric instances because they would all share a single definition.

    /// <summary>
    /// Indicates which field, property, or method should be used to determine the counter name for the event metric.
    /// </summary>
    /// <remarks>The current object must also have the EventMetric attribute defined.  Only one field, property, or method in an object
    /// can have this attribute defined.  Whatever value is returned will be converted to a string to be the category name for the metric.
    /// If no item on an object has this attribute defined, the attribute value from the EventMetric attribute will be used.</remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class EventMetricCounterNameAttribute : Attribute
    {
    }

    /// <summary>
    /// Indicates which field, property, or method should be used to determine the instance name for the event metric.
    /// </summary>
    /// <remarks>The current object must also have the EventMetric attribute defined.  Only one field, property, or method in an object
    /// can have this attribute defined.  Whatever value is returned will be converted to a string to uniquely identify the metric, or
    /// a null value will select the default instance.  If no item on an object has this attribute defined, the default event metric will
    /// be used unless an instance name is specified when sampling.</remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class EventMetricInstanceNameAttribute : Attribute
    {
    }

}
