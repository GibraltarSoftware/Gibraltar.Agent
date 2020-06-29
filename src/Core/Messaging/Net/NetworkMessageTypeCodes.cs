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
#pragma warning disable 1591
namespace Gibraltar.Messaging.Net
{
    public enum NetworkMessageTypeCode
    {
        Unknown = 0,
        LiveViewStartCommand = 1,
        LiveViewStopCommand = 2,
        SendSession = 3,
        SessionHeader = 4,
        GetSessionHeaders = 5,
        RegisterAnalystCommand = 6,
        RegisterAgentCommand = 7,
        SessionClosed = 8,
        PacketStreamStartCommand = 9,

        /// <summary>
        /// Measures the clock drift and latency between two computers
        /// </summary>
        ClockDrift = 10,

        /// <summary>
        /// Suspend sending session headers to the client
        /// </summary>
        PauseSessionHeaders = 11,

        /// <summary>
        /// Resume sending session headers to the client
        /// </summary>
        ResumeSessionHeaders = 12,
    }
}
