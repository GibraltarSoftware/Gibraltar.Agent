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

using Gibraltar.Agent.Metrics;

#endregion File Header

namespace Gibraltar.Agent.Test.Metrics
{
    [SampledMetric("UserSampledObject", "Attributes.Unit Test Data")]
    public class UserSampledObject
    {
        private int m_PrimaryValue;
        private int m_SecondaryValue;
        private string m_InstanceName;

        public UserSampledObject()
        {
            m_PrimaryValue = 0;
            m_SecondaryValue = 1;
            m_InstanceName = "Dummy instance";
        }

        public UserSampledObject(int primaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = 1;
            m_InstanceName = "Dummy instance";
        }

        public UserSampledObject(int primaryValue, int secondaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = secondaryValue;
            m_InstanceName = "Dummy instance";
        }

        public UserSampledObject(string instanceName)
        {
            m_PrimaryValue = 0;
            m_SecondaryValue = 1;
            m_InstanceName = instanceName;
        }

        public UserSampledObject(string instanceName, int primaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = 1;
            m_InstanceName = instanceName;
        }

        public UserSampledObject(string instanceName, int primaryValue, int secondaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = secondaryValue;
            m_InstanceName = instanceName;
        }

        public void SetValue(int primaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = 1;
        }

        public void SetValue(int primaryValue, int secondaryValue)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = secondaryValue;
        }

        public void SetInstanceName(string instanceName)
        {
            m_InstanceName = instanceName;
        }

        [SampledMetricValue("IncrementalCount", SamplingType.IncrementalCount, null,
                       Description="Unit test sampled metric using the incremental count calculation routine")]
        [SampledMetricValue("IncrementalFraction", SamplingType.IncrementalFraction, null,
                       Description = "Unit test sampled metric using the incremental fraction calculation routine.  Rare, but fun.")]
        [SampledMetricValue("TotalCount", SamplingType.TotalCount, null,
                       Description = "Unit test sampled metric using the Total Count calculation routine.  Very common.")]
        [SampledMetricValue("TotalFraction", SamplingType.TotalFraction, null,
                       Description = "Unit test sampled metric using the Total Fraction calculation routine.  Rare, but rounds us out.")]
        [SampledMetricValue("RawCount", SamplingType.RawCount, null,
                       Description = "Unit test sampled metric using the Raw Count calculation routine, which we will then average to create sample intervals.")]
        [SampledMetricValue("RawFraction", SamplingType.RawFraction, null,
                       Description = "Unit test sampled metric using the Raw Fraction calculation routine.  Fraction types aren't common.")]
        public int PrimaryValue { get { return m_PrimaryValue; } set { m_PrimaryValue = value; } }

        [SampledMetricDivisor("IncrementalFraction")]
        [SampledMetricDivisor("TotalFraction")]
        [SampledMetricDivisor("RawFraction")]
        public int SecondaryValue { get { return m_SecondaryValue; } set { m_SecondaryValue = value; } }

        [SampledMetricInstanceName]
        public string GetInstanceName()
        {
            return m_InstanceName;
        }
    }
}
