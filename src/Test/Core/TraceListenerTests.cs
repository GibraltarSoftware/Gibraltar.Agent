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
using System.Collections.Generic;
using System.Diagnostics;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Net;
using Loupe.Extensibility.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Core
{
    [TestFixture]
    public class TraceListenerTests
    {
        private static void EnsureTraceListenerRegistered()
        {
            //Initialize our process to use our trace listener explicitly.
            if (IsListenerRegistered(typeof(LogListener)) == false)
            {
                //there isn't one registered yet, go ahead and register it
                Trace.Listeners.Add(new LogListener());
            }
        }

        private static void EnsureTraceListenerUnregistered()
        {
            List<TraceListener> victims = new List<TraceListener>();
            foreach (TraceListener traceListener in Trace.Listeners)
            {
                if (traceListener is LogListener)
                {
                    //this is one of ours, we need to remove it
                    victims.Add(traceListener); // so we can remove it after completing the iteration.
                }
            }

            //now unregister every victim
            foreach (TraceListener victim in victims)
            {
                Trace.Listeners.Remove(victim);
            }
        }

        private static bool IsListenerRegistered(Type candidate)
        {
            bool foundTraceListener = false;
            foreach (TraceListener traceListener in Trace.Listeners)
            {
                if (traceListener.GetType() == candidate)
                {
                    //yeah, we found one.
                    foundTraceListener = true;
                }
            }

            return foundTraceListener;
        }

        private static bool IsListenerRegistered(TraceListener candidate)
        {
            bool foundTraceListener = false;
            foreach (TraceListener traceListener in Trace.Listeners)
            {
                //test that it is exactly our listener object, not just a question of type.
                if (traceListener == candidate)
                {
                    //yeah, we found one.
                    foundTraceListener = true;
                }
            }

            return foundTraceListener;
        }

        [Test]
        public void TraceListenerRegistration()
        {
            //Is it already registered?  (shouldn't be for our test to be good)
            if (IsListenerRegistered(typeof(LogListener)))
            {
                Log.Write(LogMessageSeverity.Warning, "Unit Tests", "There is already a log listener registered, some tests may not return correct results.", null);
            }

            //now go and create a new one.
            LogListener newListener = new LogListener();

            Trace.Listeners.Add(newListener);

            Assert.IsTrue(IsListenerRegistered(newListener), "The log listener was not found in the trace listener collection.");

            //and now remove it
            Trace.Listeners.Remove(newListener);

            //is it there?
            Assert.IsFalse(IsListenerRegistered(newListener), "The log listener was still found in the trace listener collection after being removed.");
        }

        private static void CheckDataRateFormat(long value, string expectedFormat)
        {
            string actualFormat = CLRListener.FormatDataRate(value);
            Assert.AreEqual(expectedFormat, actualFormat, "Expected data rate format ");
        }

        [Test]
        public void CLRListenerTests()
        {
            // Try a bunch of real-world values
            CheckDataRateFormat(9600, "9.6 Kbps");
            CheckDataRateFormat(14400, "14.4 Kbps");
            CheckDataRateFormat(19200, "19.2 Kbps");
            CheckDataRateFormat(28800, "28.8 Kbps");
            CheckDataRateFormat(38400, "38.4 Kbps");
            CheckDataRateFormat(56000, "56 Kbps");
            CheckDataRateFormat(100000, "100 Kbps");
            CheckDataRateFormat(1000000, "1 Mbps");
            CheckDataRateFormat(10000000, "10 Mbps");
            CheckDataRateFormat(11000000, "11 Mbps");
            CheckDataRateFormat(54000000, "54 Mbps");
            CheckDataRateFormat(100000000, "100 Mbps");
            CheckDataRateFormat(155000000, "155 Mbps");
            CheckDataRateFormat(1000000000, "1 Gbps");
            CheckDataRateFormat(10000000000, "10 Gbps");
            CheckDataRateFormat(100000000000, "100 Gbps");
            CheckDataRateFormat(1000000000000, "1000 Gbps");
            CheckDataRateFormat(10000000000000, "10000 Gbps");

            // Check handling of special cases and boundary conditions
            CheckDataRateFormat(0, "0 Kbps");
            CheckDataRateFormat(1, "0.001 Kbps");
            CheckDataRateFormat(12, "0.012 Kbps");
            CheckDataRateFormat(123, "0.123 Kbps");
            CheckDataRateFormat(999, "0.999 Kbps");
            CheckDataRateFormat(1000, "1 Kbps");
            CheckDataRateFormat(1200, "1.2 Kbps");
            CheckDataRateFormat(1230, "1.23 Kbps");
            CheckDataRateFormat(1234, "1.234 Kbps");
            CheckDataRateFormat(12000, "12 Kbps");
            CheckDataRateFormat(12300, "12.3 Kbps");
            CheckDataRateFormat(12340, "12.34 Kbps");
            CheckDataRateFormat(12345, "12.345 Kbps");
            CheckDataRateFormat(123000, "123 Kbps");
            CheckDataRateFormat(123400, "123.4 Kbps");
            CheckDataRateFormat(123450, "123.45 Kbps");
            CheckDataRateFormat(123456, "123.456 Kbps");
            CheckDataRateFormat(999000, "999 Kbps");
            CheckDataRateFormat(999999, "999.999 Kbps");
            CheckDataRateFormat(1000000, "1 Mbps");
            CheckDataRateFormat(1200000, "1.2 Mbps");
            CheckDataRateFormat(1230000, "1.23 Mbps");
            CheckDataRateFormat(1234000, "1.234 Mbps");
            CheckDataRateFormat(1234500, "1234.5 Kbps");
            CheckDataRateFormat(1234560, "1234.56 Kbps");
            CheckDataRateFormat(1234567, "1234.567 Kbps");
            CheckDataRateFormat(9999000, "9.999 Mbps");
            CheckDataRateFormat(9999100, "9999.1 Kbps");
            CheckDataRateFormat(10000001, "10 Mbps");
            CheckDataRateFormat(10000100, "10 Mbps");
            CheckDataRateFormat(12000000, "12 Mbps");
            CheckDataRateFormat(12300000, "12.3 Mbps");
            CheckDataRateFormat(12340000, "12.34 Mbps");
            CheckDataRateFormat(12345000, "12.345 Mbps");
            CheckDataRateFormat(12345600, "12.345 Mbps");
            CheckDataRateFormat(12345670, "12.345 Mbps");
            CheckDataRateFormat(12345678, "12.345 Mbps");
            CheckDataRateFormat(123000000, "123 Mbps");
            CheckDataRateFormat(123400000, "123.4 Mbps");
            CheckDataRateFormat(123450000, "123.45 Mbps");
            CheckDataRateFormat(123456000, "123.456 Mbps");
            CheckDataRateFormat(123456700, "123.456 Mbps");
            CheckDataRateFormat(123456780, "123.456 Mbps");
            CheckDataRateFormat(123456789, "123.456 Mbps");
            CheckDataRateFormat(999999999, "999.999 Mbps");
            CheckDataRateFormat(1000000000, "1 Gbps");
            CheckDataRateFormat(1200000000, "1.2 Gbps");
            CheckDataRateFormat(1230000000, "1.23 Gbps");
            CheckDataRateFormat(1234000000, "1.234 Gbps");
            CheckDataRateFormat(1234500000, "1234.5 Mbps");
            CheckDataRateFormat(1234560000, "1234.56 Mbps");
            CheckDataRateFormat(1234567000, "1234.567 Mbps");
            CheckDataRateFormat(1234567800, "1234.567 Mbps");
            CheckDataRateFormat(1234567890, "1234.567 Mbps");
            CheckDataRateFormat(9999000000, "9.999 Gbps");
            CheckDataRateFormat(9999100000, "9999.1 Mbps");
            CheckDataRateFormat(9999999000, "9999.999 Mbps");
            CheckDataRateFormat(9999999999, "9999.999 Mbps");
            CheckDataRateFormat(12345678901, "12.345 Gbps");
            CheckDataRateFormat(123456789012, "123.456 Gbps");
            CheckDataRateFormat(1234567890123, "1234.567 Gbps");
            CheckDataRateFormat(12345678901234, "12345.678 Gbps");
        }
      

        [Test]
        public void WriteTrace()
        {
            EnsureTraceListenerRegistered();

            Log.Write(LogMessageSeverity.Information, "Unit Tests", "Writing out trace listener messages...", null);
            Trace.TraceInformation("This is a trace information message");
            Trace.TraceInformation("This is a trace information message with two insertions: #1:{0} #2:{1}", "First insertion", "Second insertion");
            Trace.TraceWarning("This is a trace information message");
            Trace.TraceWarning("This is a trace information message with two insertions: #1:{0} #2:{1}", "First insertion", "Second insertion");
            Trace.TraceError("This is a trace information message");
            Trace.TraceError("This is a trace information message with two insertions: #1:{0} #2:{1}", "First insertion", "Second insertion");
            Trace.Write("this is a string message");
            Trace.Write(new ArgumentException("This is an argument exception"));
            Trace.Write("This is a trace write message", "This is a trace write category");
            Trace.WriteIf(false, "this is a string message");
            Trace.WriteIf(false, new ArgumentException("This is an argument exception"));
            Trace.WriteIf(false, "This is a trace write message", "This is a trace write category");
            Log.Write(LogMessageSeverity.Information, "Unit Tests", "There should have been 18 trace listener messages written out.", null);
            // ToDo: That count seems off for the number of calls above, see if this count needs to be updated.
        }

#if DEBUG
        //test trace scenarios that freak NUnit
        [Test]
        [Ignore("These tests interfere with NUnit and will always appear to fail")]
        public void WriteFailTrace()
        {
            EnsureTraceListenerRegistered();

            //these scenarios are here because they interfere with NUnit - it treats these as causing the test to fail regardless.
            Trace.Fail("This is a trace Fail message");
            Trace.Fail("This is a trace fail message", "This is the detail message for the trace fail message");
            Trace.Assert(false);
            Trace.Assert(false, "This is a false trace assertion message");
            Trace.Assert(false, "This is a false trace assertion message", "This is the detail message for the trace fail message");
            
        }
#endif

        [OneTimeTearDown]
        public void TearDown()
        {
            EnsureTraceListenerUnregistered();
        }
    }
}
