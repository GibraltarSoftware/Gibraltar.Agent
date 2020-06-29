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
namespace Gibraltar.Agent.Test.Metrics
{
    /// <summary>
    /// This is a standin for any user defined data enumeration (not in our normal libraries)
    /// </summary>
    public enum UserDataEnumeration
    {
        /// <summary>
        /// The experiment completed successfully
        /// </summary>
        Success,

        /// <summary>
        /// The experiment was not completed because the user cancelled it
        /// </summary>
        Cancel,

        /// <summary>
        /// The experiment was terminated early because of a communication failure
        /// </summary>
        Quit
    }
}
