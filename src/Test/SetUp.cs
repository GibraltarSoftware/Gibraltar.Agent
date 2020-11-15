
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
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test
{
    [SetUpFixture]
    public class SetUp
    {
        private bool m_HaveCanceled = false;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //connect up to the log event so we can test configuration.
            Log.Initializing += Log_Initializing;

            //Do our first initialize.
            Log.Initialize(null, true);

            //we shouldn't be up...
            if (Log.Initialized)
            {
                Trace.WriteLine("Logging is already initialized while starting up, so we won't be able to test delayed initialization.");
            }
            else
            {
                //try again which should really initialize it.
                Log.Initialize(null, false);
            }

            if (Log.Initialized == false)
            {
                //now we have a problem.
                Trace.WriteLine("Logging failed to initialize.");
            }

            //See how many Log items there are
            if (Log.Metrics.Count > 0)
            {
                //write out a message to the log indicating we're starting with metrics defined.
                Log.Write(LogMessageSeverity.Information, "Unit Tests", "Existing Metrics Detected", "There are already {0} metrics defined in the global log class.", Log.Metrics.Count);
            }

            //now we want to wait for our performance monitor to initialize before we proceed.
            DateTime perfMonitorInitStart = DateTime.Now;
            while ((Listener.Initialized == false) 
                && ((DateTime.Now - perfMonitorInitStart).TotalMilliseconds < 5000))
            {
                //just wait for it...
                Thread.Sleep(20);
            }

            //if we exited the while loop and it isn't done yet, it didn't get done fast enough.
            if (Listener.Initialized == false)
            {
                Log.Write(LogMessageSeverity.Warning, "Unit Tests", "Performance Monitor Initialization Failed", "Performance Monitor failed to complete its initialization after we waited {0} milliseconds.", 
                          (DateTime.Now - perfMonitorInitStart).TotalMilliseconds);
            }
            else
            {
                Log.Write(LogMessageSeverity.Information, "Unit Tests", "Performance Monitor Initialization Complete", "Performance Monitor completed initialization after we waited {0} milliseconds.",
                          (DateTime.Now - perfMonitorInitStart).TotalMilliseconds);
            }
        }

        void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            if (m_HaveCanceled == false)
            {
                //we haven't done our first cancel test.
                e.Cancel = true;
                m_HaveCanceled = true;
            }
            else
            {
                //set up our logging.
                PublisherConfiguration publisher = e.Configuration.Publisher;
                publisher.ProductName = "NUnit";
                publisher.ApplicationName = "Gibraltar.Test";

                //and now try to get the file version.  This is risky.
                object[] fileVersionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);

                if ((fileVersionAttributes != null) && (fileVersionAttributes.Length > 0))
                {
                    AssemblyFileVersionAttribute leadAttribute = fileVersionAttributes[0] as AssemblyFileVersionAttribute;

                    if (leadAttribute != null)
                    {
                        publisher.ApplicationVersion = new Version(leadAttribute.Version);
                    }
                }

                publisher.ApplicationDescription = "NUnit tests of the Gibraltar Core Library";

                e.Configuration.SessionFile.EnableFilePruning = false;
            }
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            //Tell our central log session we're shutting down nicely
            Log.EndSession(SessionStatus.Normal, 0, "Ending unit tests in Gibraltar.Test");
        }
    }
}
