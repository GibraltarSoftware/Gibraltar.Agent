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
using Gibraltar.Agent;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test
{
    [TestFixture]
    public class UnitConversionTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void TimeConversions()
        {
            MetricUnit msec = new MetricUnit(MetricUnit.MetricTimeUnit.msec);
            MetricUnit sec = new MetricUnit(MetricUnit.MetricTimeUnit.sec);
            MetricUnit min = new MetricUnit(MetricUnit.MetricTimeUnit.min);
            MetricUnit hour = new MetricUnit(MetricUnit.MetricTimeUnit.h);
            MetricUnit day = new MetricUnit(MetricUnit.MetricTimeUnit.day);
            MetricUnit week = new MetricUnit(MetricUnit.MetricTimeUnit.week);

            // Week combinations
            Assert.IsTrue(week.ConversionFactor(week) == 1, "week to week conversion");
            Assert.IsTrue(week.ConversionFactor(day) == 7, "week to day conversion");
            Assert.IsTrue(week.ConversionFactor(hour) == 168, "week to hour conversion");
            Assert.IsTrue(week.ConversionFactor(min) == 10080, "week to min conversion");
            Assert.IsTrue(week.ConversionFactor(sec) == 604800, "week to sec conversion");
            Assert.IsTrue(week.ConversionFactor(msec) == 604800000, "week to msec conversion");

            // day combinations
            Assert.IsTrue(day.ConversionFactor(week) == (double)(1.0 / 7.0), "day to week conversion");
            Assert.IsTrue(day.ConversionFactor(day) == 1, "day to day conversion");
            Assert.IsTrue(day.ConversionFactor(hour) == 24, "day to hour conversion");
            Assert.IsTrue(day.ConversionFactor(min) == 1440, "day to min conversion");
            Assert.IsTrue(day.ConversionFactor(sec) == 86400, "day to sec conversion");
            Assert.IsTrue(day.ConversionFactor(msec) == 86400000, "day to msec conversion");

            // hour combinations
            Assert.IsTrue(hour.ConversionFactor(week) == (double)(1.0 / 24.0 / 7.0), "hour to week conversion");
            Assert.IsTrue(hour.ConversionFactor(day) == (double)(1.0 / 24.0), "hour to day conversion");
            Assert.IsTrue(hour.ConversionFactor(hour) == 1, "hour to hour conversion");
            Assert.IsTrue(hour.ConversionFactor(min) == 60, "hour to min conversion");
            Assert.IsTrue(hour.ConversionFactor(sec) == 3600, "hour to sec conversion");
            Assert.IsTrue(hour.ConversionFactor(msec) == 3600000, "hour to msec conversion");

            // min combinations
            Assert.IsTrue(min.ConversionFactor(week) == (double)(1.0 / 60.0 / 24.0 / 7.0), "min to week conversion");
            Assert.IsTrue(min.ConversionFactor(day) == (double)(1.0 / 60.0 / 24.0), "min to day conversion");
            Assert.IsTrue(min.ConversionFactor(hour) == (double)(1.0 / 60.0), "min to hour conversion");
            Assert.IsTrue(min.ConversionFactor(min) == 1, "min to min conversion");
            Assert.IsTrue(min.ConversionFactor(sec) == 60, "min to sec conversion");
            Assert.IsTrue(min.ConversionFactor(msec) == 60000, "min to msec conversion");

            // sec combinations
            double f = sec.ConversionFactor(week);
            Assert.IsTrue(sec.ConversionFactor(week) == (double)(1.0 / 604800), "sec to week conversion");
            Assert.IsTrue(sec.ConversionFactor(day) == (double)(1.0 / 60.0 / 60.0 / 24.0), "sec to day conversion");
            Assert.IsTrue(sec.ConversionFactor(hour) == (double)(1.0 / 60.0 / 60.0), "sec to hour conversion");
            Assert.IsTrue(sec.ConversionFactor(min) == (double)(1.0 / 60.0), "sec to min conversion");
            Assert.IsTrue(sec.ConversionFactor(sec) == 1, "sec to sec conversion");
            Assert.IsTrue(sec.ConversionFactor(msec) == 1000, "sec to msec conversion");

            // milisec combinations
            Assert.IsTrue(msec.ConversionFactor(week) == (double)(1.0 / 604800000), "msec to week conversion");
            Assert.IsTrue(msec.ConversionFactor(day) == (double)(1.0 / 86400000), "msec to day conversion");
            Assert.IsTrue(msec.ConversionFactor(hour) == (double)(1.0 / 3600000), "msec to hour conversion");
            Assert.IsTrue(msec.ConversionFactor(min) == (double)(1.0 / 1000.0 / 60.0), "msec to min conversion");
            Assert.IsTrue(msec.ConversionFactor(sec) == (double)(1.0 / 1000.0), "msec to sec conversion");
            Assert.IsTrue(msec.ConversionFactor(msec) == 1, "msec to msec conversion");
        }

        [Test]
        public void DataConversions()
        {
            MetricUnit bit = new MetricUnit(MetricUnit.MetricDataUnit.bit);
            MetricUnit bytes = new MetricUnit(MetricUnit.MetricDataUnit.bytes);
            MetricUnit kbit = new MetricUnit(MetricUnit.MetricDataUnit.kbit);
            MetricUnit kB = new MetricUnit(MetricUnit.MetricDataUnit.kB);
            MetricUnit Mbit = new MetricUnit(MetricUnit.MetricDataUnit.Mbit);
            MetricUnit MB = new MetricUnit(MetricUnit.MetricDataUnit.MB);
            MetricUnit Gbit = new MetricUnit(MetricUnit.MetricDataUnit.Gbit);
            MetricUnit GB = new MetricUnit(MetricUnit.MetricDataUnit.GB);

            // Bit combinations
            Assert.IsTrue(bit.ConversionFactor(bit) == 1, "bit to bit conversion");
            Assert.IsTrue(bit.ConversionFactor(bytes) == 1.0 / 8.0, "bit to bytes conversion");
            Assert.IsTrue(bit.ConversionFactor(kbit) == 1.0 / 1000.0, "bit to kbit conversion");
            Assert.IsTrue(bit.ConversionFactor(kB) == 1.0 / 8.0 / 1024.0 , "bit to kB conversion");
            Assert.IsTrue(bit.ConversionFactor(Mbit) == 1.0 / 1000.0 / 1000.0, "bit to Mbit conversion");
            Assert.IsTrue(bit.ConversionFactor(MB) == 1.0 / 8.0 / 1024.0 / 1024.0, "bit to MB conversion");
            Assert.IsTrue(bit.ConversionFactor(Gbit) == 1.0 / 1000000000.0, "bit to Gbit conversion");
            Assert.IsTrue(bit.ConversionFactor(GB) == 1.0 / 8589934592.0, "bit to GB conversion");

            // TODO: Other Data conversions
        }

        [Test]
        public void DataRateConversions()
        {
            MetricUnit bit_msec = new MetricUnit(MetricUnit.MetricDataUnit.bit, MetricUnit.MetricTimeUnit.msec);
            MetricUnit bytes_sec = new MetricUnit(MetricUnit.MetricDataUnit.bytes, MetricUnit.MetricTimeUnit.sec);
            MetricUnit kbit_sec = new MetricUnit(MetricUnit.MetricDataUnit.kbit, MetricUnit.MetricTimeUnit.sec);
            MetricUnit kB_sec = new MetricUnit(MetricUnit.MetricDataUnit.kB, MetricUnit.MetricTimeUnit.sec);
            MetricUnit Mbit_sec = new MetricUnit(MetricUnit.MetricDataUnit.Mbit, MetricUnit.MetricTimeUnit.sec);
            MetricUnit MB_sec = new MetricUnit(MetricUnit.MetricDataUnit.MB, MetricUnit.MetricTimeUnit.sec);
            MetricUnit Gbit_sec = new MetricUnit(MetricUnit.MetricDataUnit.Gbit, MetricUnit.MetricTimeUnit.sec);
            MetricUnit GB_sec = new MetricUnit(MetricUnit.MetricDataUnit.GB, MetricUnit.MetricTimeUnit.sec);
            MetricUnit kbit_min = new MetricUnit(MetricUnit.MetricDataUnit.kbit, MetricUnit.MetricTimeUnit.min);
            MetricUnit kB_min = new MetricUnit(MetricUnit.MetricDataUnit.kB, MetricUnit.MetricTimeUnit.min);
            MetricUnit Mbit_hour = new MetricUnit(MetricUnit.MetricDataUnit.Mbit, MetricUnit.MetricTimeUnit.h);
            MetricUnit MB_hour = new MetricUnit(MetricUnit.MetricDataUnit.MB, MetricUnit.MetricTimeUnit.h);
            MetricUnit Gbit_day = new MetricUnit(MetricUnit.MetricDataUnit.Gbit, MetricUnit.MetricTimeUnit.day);
            MetricUnit GB_day = new MetricUnit(MetricUnit.MetricDataUnit.GB, MetricUnit.MetricTimeUnit.day);

            // Test some common conversions
            Assert.IsTrue(bit_msec.ConversionFactor(bit_msec) == 1, "bit_msec to bit_msec conversion");
            Assert.IsTrue(bit_msec.ConversionFactor(bytes_sec) == 1000.0 / 8.0, "bit_msec to bytes_sec conversion");
            Assert.IsTrue(bit_msec.ConversionFactor(kbit_min) == 1000.0 * 60.0 / 1000.0, "bit_msec to kbit_min conversion");
            Assert.IsTrue(bit_msec.ConversionFactor(kB_min) == 1000.0 * 60.0 / 1024.0 / 8.0, "bit_msec to kB_min conversion");
            Assert.IsTrue(bit_msec.ConversionFactor(Mbit_hour) == 1000.0 * 60.0 * 60.0/ 1000000.0, "bit_msec to Mbit_hour conversion");
            Assert.IsTrue(Math.Round(bit_msec.ConversionFactor(MB_hour), 12) == Math.Round(1000.0 * 60.0 * 60.0 / 1024.0 / 1024.0 / 8.0, 12), "bit_msec to MB_hour conversion");
            Assert.IsTrue(Math.Round(bit_msec.ConversionFactor(Gbit_day), 12) == Math.Round(1000.0 * 60.0 * 60.0 * 24.0 / 1000000000.0, 12), "bit_msec to Gbit_day conversion");
            Assert.IsTrue(Math.Round(bit_msec.ConversionFactor(GB_day), 12) == Math.Round(1000.0 * 60.0 * 60.0 * 24.0 / 1024.0 / 1024.0 / 1024.0 / 8.0, 12), "bit_msec to GB_day conversion");

            // Data/sec conversions
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(bit_msec), 12) == Math.Round(1024.0 * 1024.0 * 8.0 / 1000.0, 12), "MB_sec to bit_msec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(bytes_sec), 12) == Math.Round(1024.0 * 1024.0, 12), "MB_sec to bytes_sec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(kbit_sec), 12) == Math.Round(1024.0 * 1024.0 * 8.0 / 1000.0, 12), "MB_sec to kbit_sec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(kB_sec), 12) == Math.Round(1024.0, 12), "MB_sec to kB_sec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(Mbit_sec), 12) == Math.Round(1024.0 * 1024.0 * 8.0 / 1000.0 / 1000.0, 12), "MB_sec to Mbit_sec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(Gbit_sec), 12) == Math.Round(1024.0 * 1024.0 * 8.0 / 1000.0 / 1000.0 / 1000.0, 12), "MB_sec to Gbit_sec conversion");
            Assert.IsTrue(Math.Round(MB_sec.ConversionFactor(GB_sec), 12) == Math.Round(1.0 / 1024.0, 12), "MB_sec to GB_sec conversion");

        }
    }
}