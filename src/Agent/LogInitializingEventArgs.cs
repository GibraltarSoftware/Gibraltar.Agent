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
using Gibraltar.Agent.Configuration;

#endregion


namespace Gibraltar.Agent
{
    /// <summary>
    /// Event arguments for the Log.Initialize event of the Loupe Agent Logging class.
    /// </summary>
    public class LogInitializingEventArgs : EventArgs
    {
        internal LogInitializingEventArgs(AgentConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// If set to true the initialization process will not complete and the agent will stay dormant.
        /// </summary>
        public bool Cancel { get; set; }


        /// <summary>
        /// The configuration for the agent to start with
        /// </summary>
        /// <remarks>The configuration will reflect the effect of the current application configuration file and Agent default values.</remarks>
        public AgentConfiguration Configuration { get; private set; }
    }
}
