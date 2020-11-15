
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
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Core
{
    [TestFixture]
    public class EventMetricDefinitionTests
    {
        /// <summary>
        /// Register metric definitions the long hand way to prove the underlying API works.
        /// </summary>
        /// <remarks>This test is coupled with other tests in this batch, so don't go changing the strings willy nilly</remarks>
        [Test]
        public void RegisterEventMetrics()
        {
            //Define an event metric manually (the hard way)
            EventMetricDefinition newEventMetric =
                new EventMetricDefinition("EventMetricTests", "Gibraltar.Monitor.Test", "Manual");

            //we now have a minimal definition, but we probably want to add a few attributes to make it useful
            //NOTE:  This is designed to exactly match UserDataObject for convenience in analyzing results.
            EventMetricValueDefinitionCollection valueDefinition = (EventMetricValueDefinitionCollection)newEventMetric.Values;
            valueDefinition.Add("short_average", typeof(short), "Short Average", "Data of type Short").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("short_sum", typeof(short), "Short Sum", "Data of type Short").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("short_runningaverage", typeof(short), "Short Running Average", "Data of type Short").DefaultTrend = EventMetricValueTrend.RunningAverage;
            valueDefinition.Add("short_runningsum", typeof(short), "Short Running Sum", "Data of type Short").DefaultTrend = EventMetricValueTrend.RunningSum;
            valueDefinition.Add("ushort_average", typeof(ushort), "UShort Average", "Data of type UShort").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("ushort_sum", typeof(ushort), "UShort Sum", "Data of type UShort").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("int_average", typeof(int), "Int Average", "Data of type Int").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("int_sum", typeof(int), "Int Sum", "Data of type Int").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("uint_average", typeof(uint), "UInt Average", "Data of type UInt").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("uint_sum", typeof(uint), "UInt Sum", "Data of type UInt").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("long_average", typeof(long), "Long Average", "Data of type Long").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("long_sum", typeof(long), "Long Sum", "Data of type Long").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("ulong_average", typeof(ulong), "ULong Average", "Data of type ULong").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("ulong_sum", typeof(ulong), "ULong Sum", "Data of type ULong").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("decimal_average", typeof(decimal), "Decimal Average", "Data of type Decimal").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("decimal_sum", typeof(decimal), "Decimal Sum", "Data of type Decimal").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("double_average", typeof(double), "Double Average", "Data of type Double").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("double_sum", typeof(double), "Double Sum", "Data of type Double").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("float_average", typeof(float), "Float Average", "Data of type Float").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("float_sum", typeof(float), "Float Sum", "Data of type Float").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("timespan_average", typeof(TimeSpan), "TimeSpan Average", "Data of type TimeSpan").DefaultTrend = EventMetricValueTrend.Average;
            valueDefinition.Add("timespan_sum", typeof(TimeSpan), "TimeSpan Sum", "Data of type TimeSpan").DefaultTrend = EventMetricValueTrend.Sum;
            valueDefinition.Add("timespan_runningaverage", typeof(TimeSpan), "TimeSpan Running Average", "Data of type TimeSpan represented as a running average.").DefaultTrend = EventMetricValueTrend.RunningAverage;
            valueDefinition.Add("timespan_runningsum", typeof(TimeSpan), "TimeSpan Running Sum", "Data of type TimeSpan represented as a running sum.").DefaultTrend = EventMetricValueTrend.RunningSum;
            valueDefinition.Add("string", typeof(string), "String", "Data of type String");
            valueDefinition.Add("system.enum", typeof(UserDataEnumeration), "System.Enum", "Data of type System.Enum");

            newEventMetric.DefaultValue = newEventMetric.Values["int_average"];
            newEventMetric = newEventMetric.Register(); // Register it with the collection.

            //Create this instance of that definition
            newEventMetric.Metrics.Add("This Experiment");
        }

        /*
        /// <summary>
        /// Register metric definitions and metrics using reflection instead of the long-hand way.
        /// </summary>
        /// <remarks>This test is coupled with other tests in this batch, so don't go changing the strings willy nilly</remarks>
        [Test]
        [Obsolete("Internal use of event metrics must define programmatically rather than by reflection.", true)]
        public void RegisterEventMetricsReflection()
        {
            //Define an event metric using reflection.  See Mock data object.
            EventMetricDefinition.AddOrGet(typeof(UserDataObject));

            //Create a metric using the base object instance
            UserDataObject myExistingDataObject = new UserDataObject("RegisterEventMetricsReflection");
            EventMetric existingObjectMetric = EventMetric.AddOrGet(myExistingDataObject);

            //now add an inherited object, this one shouldn't add at all (because it doesn't have the attributes)
            UserNonMetricDataObject myNonMetricObject = new UserNonMetricDataObject("RegisterEventMetricsReflection", 1);
            EventMetric myNonMetricObjectInterfaceMetric = EventMetric.AddOrGet(myNonMetricObject);   //this one only uses an interface for implementation

            //now add the third inherited object that does implement the attributes, but different ones than the base object
            UserSecondaryMetricDataObject myInheritorDataObject = new UserSecondaryMetricDataObject("RegisterEventMetricsReflection", 2);
            EventMetric inheritorObjectMetric = EventMetric.AddOrGet(myInheritorDataObject);

            //Now add the second object, but on an implemented interface. This we have to be more creative to do since the object supports multiple interfaces.
            EventMetricDefinition eventMetricTestInterfaceDefinition = EventMetricDefinition.AddOrGet(typeof(IEventMetricTestInterface));
            EventMetric inheritorObjectInterfaceMetric = eventMetricTestInterfaceDefinition.Metrics.AddOrGet(myInheritorDataObject);
            //this metric object and our previous one, even though they refer to the same object, should be distinct
            Assert.AreNotSame(inheritorObjectMetric, inheritorObjectInterfaceMetric);

            //Now look up each of the definitions by their objects
            EventMetricDefinition userDataObjectDefinition = EventMetricDefinition.AddOrGet(typeof(UserDataObject)); //really just should get our object
            EventMetricDefinition inheritorObjectDefinition = EventMetricDefinition.AddOrGet(myInheritorDataObject.GetType()); //really just should get our object

            //now count up how many instances we have of each item - we should have one of each except the interface which should have two.
            Assert.AreEqual(1, userDataObjectDefinition.Metrics.Count);
            Assert.AreEqual(1, inheritorObjectDefinition.Metrics.Count);
            Assert.AreEqual(2, eventMetricTestInterfaceDefinition.Metrics.Count);

            //make sure we have all of our metrics we expect when we do a contains
            Assert.IsTrue(userDataObjectDefinition.Metrics.Contains(existingObjectMetric));
            Assert.IsTrue(eventMetricTestInterfaceDefinition.Metrics.Contains(myNonMetricObjectInterfaceMetric));
            Assert.IsTrue(eventMetricTestInterfaceDefinition.Metrics.Contains(inheritorObjectInterfaceMetric));
            Assert.IsTrue(inheritorObjectDefinition.Metrics.Contains(inheritorObjectMetric));
        }
        */
    }
}
