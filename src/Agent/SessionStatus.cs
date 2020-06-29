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
namespace Gibraltar.Agent
{
    /// <summary>
    /// The current known disposition of the session
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// The final status of the session isn't known
        /// </summary>
        Unknown = Loupe.Extensibility.Data.SessionStatus.Unknown,

        /// <summary>
        /// The application is still running
        /// </summary>
        Running = Loupe.Extensibility.Data.SessionStatus.Running,

        /// <summary>
        /// The application closed normally
        /// </summary>
        Normal = Loupe.Extensibility.Data.SessionStatus.Normal,
        
        /// <summary>
        /// The application closed unexpectedly
        /// </summary>
        Crashed = Loupe.Extensibility.Data.SessionStatus.Crashed,
    }
}
