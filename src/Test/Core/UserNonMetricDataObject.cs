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

#endregion File Header

namespace Gibraltar.Test.Core
{
    // ToDo: Compile turned off, but left around in case we want to port these test cases to Agent.Test.
    public class UserNonMetricDataObject : UserDataObject, IEventMetricTestInterface
    {
        private readonly int m_InstanceNum;
        public UserNonMetricDataObject(string instanceName, int instanceNum)
            : base(instanceName)
        {
            m_InstanceNum = instanceNum;
        }

        /// <summary>
        /// Our numeric instance num (so that inheritors can use it for ther IEventMetricTestInterface implementation)
        /// </summary>
        public int InstanceNum { get { return m_InstanceNum; } }

        #region IEventMetricTestInterface Implementation

        int IEventMetricTestInterface.InstanceNum { get { return m_InstanceNum; } }
        string IEventMetricTestInterface.StringProperty { get { return base.String; } }

        string IEventMetricTestInterface.StringMethod()
        {
            return "Method Version: " + base.String;
        }

        int IEventMetricTestInterface.IntProperty { get { return base.Int; } }

        #endregion
    }
}
