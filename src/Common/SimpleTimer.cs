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
using System.Diagnostics;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Singleton facade around timers for internal testing 
    /// </summary>
    public static class SimpleTimer
    {
        private static object s_SyncObject = new object();
        private static readonly Dictionary<string, TimeCounter> s_Timers = new Dictionary<string, TimeCounter>();

        /// <summary>
        /// Start a named timer
        /// </summary>
        public static void Start(string timerName)
        {
            lock (s_SyncObject)
            {
                var timer = FindTimer(timerName);
                if (timer == null)
                {
                    timer = new TimeCounter(timerName);
                    s_Timers.Add(timerName, timer);
                }

                if (!timer.Stopwatch.IsRunning)
                    timer.Stopwatch.Start();
                timer.Count++;
            }
        }

        /// <summary>
        /// Stop a named timer
        /// </summary>
        public static void Stop(string timerName)
        {
            lock (s_SyncObject)
            {
                var timer = FindTimer(timerName);
                if (timer == null)
                {
                    return;
                }
                timer.Stopwatch.Stop();
            }
        }

        /// <summary>
        /// Stop a named timer and write the elapsed time to Trace
        /// </summary>
        public static void WriteLine(string timerName)
        {
            lock (s_SyncObject)
            {
                var timer = FindTimer(timerName);
                if (timer == null)
                    Trace.WriteLine("Undefined timer: " + timerName);
                else
                {
                    timer.Stopwatch.Stop();
                    var name = timer.Name;
                    var count = timer.Count;
                    var ms = timer.Stopwatch.ElapsedMilliseconds;
                    var avg = count == 0 ? 0 : ms / (double)count;
                    if (count == 1)
                        Trace.WriteLine("Timer " + name + ": " + ms);
                    else
                        Trace.WriteLine("Timer " + name + ": " + ms + " ms / " + count + " calls = " + avg.ToString("F3") + " ms / call");

                    timer.Reset();
                }
            }
        }

        /// <summary>
        /// Reset a named timer
        /// </summary>
        public static void Reset(string timerName)
        {
            lock (s_SyncObject)
            {
                var timer = FindTimer(timerName);
                if (timer != null)
                    timer.Reset();
            }
        }

        /// <summary>
        /// Write the elapsed time of a named timer to a StringBuilder, optionally reset the timer as well
        /// </summary>
        public static void Write(StringBuilder output, string timerName, bool reset)
        {
            lock (s_SyncObject)
            {
                var timer = FindTimer(timerName);
                if (timer == null)
                    output.AppendLine("Undefined timer: " + timerName);
                else
                {
                    var name = timer.Name;
                    var count = timer.Count;
                    var ms = timer.Stopwatch.ElapsedMilliseconds;
                    var avg = count == 0 ? 0 : ms / (double)count;
                    output.AppendLine("Timer " + name + ": " + ms + " ms / " + count + " calls = " + avg.ToString("F3") + " ms / call");
                    if (reset)
                        timer.Reset();
                }
            }
        }

        /// <summary>
        /// Write the elapsed time of a all named timers to a StringBuilder, optionally reset the timers as well
        /// </summary>
        public static void WriteAll(StringBuilder output, bool reset)
        {
            lock (s_SyncObject)
            {
                foreach (var timeCounter in s_Timers)
                {
                    var timer = timeCounter.Value;
                    var name = timer.Name;
                    var count = timer.Count;
                    var ms = timer.Stopwatch.ElapsedMilliseconds;
                    var avg = count == 0 ? 0 : ms / (double)count;
                    output.AppendLine("Timer " + name + ": " + ms + " ms / " + count + " calls = " + avg.ToString("F3") + " ms / call");
                    if (reset)
                        timer.Reset();
                }
            }
        }


        private static TimeCounter FindTimer(string timerName)
        {
            TimeCounter timer;
            return s_Timers.TryGetValue(timerName, out timer) ? timer : null;
        }

        private class TimeCounter
        {
            public string Name { get; private set; }
            public int Count { get; set; }
            public Stopwatch Stopwatch { get; private set; }

            public TimeCounter(string timerName)
            {
                Name = timerName;
                Count = 0;
                Stopwatch = new Stopwatch();
            }

            public void Reset()
            {
                Stopwatch.Reset();
                Count = 0;
            }
        }
    }
}
