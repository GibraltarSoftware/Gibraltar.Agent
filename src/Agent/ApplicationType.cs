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
    /// The type of process the application was run as.
    /// </summary>
    public enum ApplicationType
    {
        /// <summary>
        /// The application type couldn't be determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A windows console application.  Can also include windows services running in console mode.
        /// </summary>
        Console = 1,

        /// <summary>
        /// A Windows Smart Client application (a traditional windows application)
        /// </summary>
        Windows = 2,

        /// <summary>
        /// A Windows Service application.
        /// </summary>
        Service = 3,

        /// <summary>
        /// A Web Application running in the ASP.NET framework.
        /// </summary>
        AspNet = 4,
    }
}
