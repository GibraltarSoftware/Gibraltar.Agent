
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
    [SampledMetric("SimpleMetricUsage", "Temperature")]
    public class UserInstancedObject
    {
        private static int m_BaseTemperature = 10;
        private readonly int m_InstanceNumber;

        public UserInstancedObject(int instanceNumber)
        {
            m_InstanceNumber = instanceNumber;
        }

        public static void SetTemperature(int baseTemperature)
        {
            m_BaseTemperature = baseTemperature;
        }

        [SampledMetricInstanceName]
        public string GetMetricInstanceName()
        {
            return string.Format("Experiment {0}", m_InstanceNumber);
        }

        [SampledMetricValue("Experiment Temperature", SamplingType.RawCount, null,
                            Description="This tracks the temperature of the various experiments.")]
        public int Temperature { get { return m_BaseTemperature + m_InstanceNumber; } }
    }
}
