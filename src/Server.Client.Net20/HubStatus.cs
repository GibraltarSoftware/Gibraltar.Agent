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

using Gibraltar.Server.Client.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// The current status of a server that is accessible over the network
    /// </summary>
    public enum HubStatus
    {
        /// <summary>
        /// The current status couldn't be determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The server is accessible and operational.
        /// </summary>
        Available = HubStatusXml.available,

        /// <summary>
        /// The server has no license and should not be communicated with.
        /// </summary>
        Expired = HubStatusXml.expired,

        /// <summary>
        /// The server is currently undergoing maintenance and is not operational.
        /// </summary>
        Maintenance = HubStatusXml.maintenance,
    }
}
