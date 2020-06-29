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

#endregion File Header

namespace Gibraltar.Test.Core
{
    /// <summary>
    /// This is an interface that is used for testing different objects with reflection event monitoring
    /// </summary>
    // ToDo: Compile turned off, but left around in case we want to port these test cases to Agent.Test.
    //[EventMetric("EventMetricTests", "Gibraltar.Monitor.Test", "IEventMetricTestInterface", "Interface for simple event metrics, designed to allow testing of implementations using just interfaces")]
    public interface IEventMetricTestInterface
    {
        //[EventMetricInstanceName]
        int InstanceNum { get; }

        //[EventMetricValue("stringproperty", "String Property", "This is a property that returns a string")]
        string StringProperty { get; }

        //[EventMetricValue("stringmethod", "String Method", "This is a method that returns a string")]
        string StringMethod();

        //[EventMetricValue("intproperty", "Integer Property", "This is a property that returns an integer", IsDefaultValue = true)]
        int IntProperty { get; }
    }
}
