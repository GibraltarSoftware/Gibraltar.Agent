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
    // ToDo: Compile turned off, but left around in case we want to port these test cases to Agent.Test.
    //[EventMetric("EventMetricTests", "Gibraltar.Monitor.Test", "UserSecondaryMetricDataObject")]
    public class UserSecondaryMetricDataObject : UserNonMetricDataObject, IEventMetricTestInterface
    {
        public UserSecondaryMetricDataObject(string instanceName, int instanceNum)
            : base(instanceName, instanceNum)
        {
        }

        #region IEventMetricTestInterface implicit implementation

        public string StringProperty { get { return "implicit: " + String; } }

        public string StringMethod()
        {
            return "Implicit Method Version: " + String;
        }

        public int IntProperty { get { return Int; } }

        #endregion
    }
}
