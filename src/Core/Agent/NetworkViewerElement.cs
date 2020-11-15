
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System.Configuration;

#endregion

namespace Gibraltar.Agent
{
    /// <summary>
    /// The application configuration information for viewing the session over a network connection
    /// </summary>
    public class NetworkViewerElement : ConfigurationSection
    {
        /// <summary>
        /// False by default, enables connecting a viewer remotely over a network when true.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// True by default, enables connecting a viewer on the local computer when true.
        /// </summary>
        [ConfigurationProperty("allowLocalClients", DefaultValue = true, IsRequired = false)]
        public bool AllowLocalClients { get { return (bool)this["allowLocalClients"]; } set { this["allowLocalClients"] = value; } }

        /// <summary>
        /// False by default, enables connecting a viewer from another computer when true.
        /// </summary>
        /// <remarks>Requires a server configuration section</remarks>
        [ConfigurationProperty("allowRemoteClients", DefaultValue = false, IsRequired = false)]
        public bool AllowRemoteClients { get { return (bool)this["allowRemoteClients"]; } set { this["allowRemoteClients"] = value; } }

        /// <summary>
        /// The maximum number of queued messages waiting to be dispatched to viewers.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be written exceeds the
        /// maximum queue length the log writer will switch to a synchronous mode to 
        /// catch up.  This will not cause the client to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        [ConfigurationProperty("maxQueueLength", DefaultValue = 2000, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 50000)]
        public int MaxQueueLength { get { return (int)this["maxQueueLength"]; } set { this["maxQueueLength"] = value; } }
    }
}
