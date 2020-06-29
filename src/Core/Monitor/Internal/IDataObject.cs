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

using System;

namespace Gibraltar.Monitor.Internal
{
    /// <summary>
    /// Implemented by any extensible data object to connect to its unique Id which it shares with its extension object.
    /// </summary>
    internal interface IDataObject
    {
        /// <summary>
        /// The unique Id of the data object which it shares with its extension object.
        /// </summary>
        Guid Id { get; }
    }
}
