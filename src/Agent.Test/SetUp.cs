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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Gibraltar.Agent.Configuration;
using NUnit.Framework;

namespace Gibraltar.Agent.Test
{
    [SetUpFixture]
    public class SetUp
    {
        private AgentConfiguration m_Configuration;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //delete the existing local logs folder for us...
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Gibraltar\Local Logs\NUnit");
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Unable to clean out local logs directory due to " + ex.GetType());
            }

            Log.Initializing += Log_Initializing;
            m_Configuration = new AgentConfiguration();
            PublisherConfiguration publisher = m_Configuration.Publisher;
            publisher.ProductName = "NUnit";
            publisher.ApplicationName = "Gibraltar.Agent.Test";

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

            publisher.ApplicationDescription = "NUnit tests of the Gibraltar Agent Library";

            m_Configuration.SessionFile.EnableFilePruning = false;
            m_Configuration.ExportFile.Folder = @"C:\Data\Export";

            //if we need email server information set that
#if CONFIGURE_EMAIL
            EmailConfiguration email = e.Configuration.Email;
            email.Server = EmailServer;
            email.Port = EmailServerPort;
            email.User = EmailServerUser;
            email.Password = EmailServerPassword;
            email.UseSsl = EmailUseSsl;

            PackagerConfiguration packager = e.Configuration.Packager;
            packager.DestinationEmailAddress = EmailToAddress;
            packager.FromEmailAddress = EmailFromAddress;
#endif

            //force us to initialize logging
            Log.StartSession(m_Configuration);
            Trace.TraceInformation("Starting testing at {0} on computer {1}", DateTimeOffset.UtcNow, Log.SessionSummary.HostName);
        }

        void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            //lets spot check a few values to be sure they're the same.
            Assert.AreEqual(m_Configuration.Publisher.ProductName, e.Configuration.Publisher.ProductName);
            Assert.AreEqual(m_Configuration.Publisher.ApplicationName, e.Configuration.Publisher.ApplicationName);
            Assert.AreEqual(m_Configuration.Publisher.ApplicationVersion, e.Configuration.Publisher.ApplicationVersion);
            Assert.AreEqual(m_Configuration.SessionFile.EnableFilePruning, e.Configuration.SessionFile.EnableFilePruning);
        }        

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            //Tell our central log session we're shutting down nicely
            Log.EndSession();
        }
    }
}
