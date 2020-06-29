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
using Gibraltar.Monitor;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Core
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void Listener()
        {
            //get the configuration class
            ListenerConfiguration curConfiguration = new AgentConfiguration().Listener;

            //store off the current configuration
            bool initialEnableConsole = curConfiguration.EnableConsole;
            bool initialCatchExcepitons = curConfiguration.CatchUnhandledExceptions;

            //now change each value and see that the change takes, but only changes the property it should.
            curConfiguration.EnableConsole = !initialEnableConsole;
            Assert.AreEqual(curConfiguration.EnableConsole, !initialEnableConsole);
            Assert.AreEqual(curConfiguration.CatchUnhandledExceptions, initialCatchExcepitons);
            //Assert.AreEqual(MonitorConfiguration.Listener.EnableConsoleRedirector, curConfiguration.EnableConsoleRedirector);
            
            //now set it back.
            curConfiguration.EnableConsole = initialEnableConsole;
            Assert.AreEqual(curConfiguration.EnableConsole, initialEnableConsole);
            Assert.AreEqual(curConfiguration.CatchUnhandledExceptions, initialCatchExcepitons);
            //Assert.AreEqual(MonitorConfiguration.Listener.EnableConsoleRedirector, curConfiguration.EnableConsoleRedirector);

            //now change each value and see that the change takes, but only changes the property it should.
            curConfiguration.CatchUnhandledExceptions = !initialCatchExcepitons;
            Assert.AreEqual(curConfiguration.CatchUnhandledExceptions, !initialCatchExcepitons);
            Assert.AreEqual(curConfiguration.EnableConsole, initialEnableConsole);
            //Assert.AreEqual(MonitorConfiguration.Listener.CatchUnhandledExceptions, curConfiguration.CatchUnhandledExceptions);

            //now set it back.
            curConfiguration.CatchUnhandledExceptions = initialCatchExcepitons;
            Assert.AreEqual(curConfiguration.CatchUnhandledExceptions, initialCatchExcepitons);
            Assert.AreEqual(curConfiguration.EnableConsole, initialEnableConsole);
            //Assert.AreEqual(MonitorConfiguration.Listener.CatchUnhandledExceptions, curConfiguration.CatchUnhandledExceptions);
        }
    }
}
