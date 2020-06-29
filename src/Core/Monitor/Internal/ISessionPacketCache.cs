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

namespace Gibraltar.Monitor.Internal
{
    /// <summary>
    /// Provides lookup services for packet factories to find other session-related packets
    /// </summary>
    /// <remarks>Implemented by the session object and the network viewer client</remarks>
    internal interface ISessionPacketCache
    {
        /// <summary>
        /// The set of threads in the current session
        /// </summary>
        ThreadInfoCollection Threads { get; }

        ApplicationUserCollection Users { get; }
    }
}
