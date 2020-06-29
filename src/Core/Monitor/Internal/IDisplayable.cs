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
    /// A standard interface for ensuring an item can be displayed in user interfaces by providing an end user short caption and long description
    /// </summary>
    /// <remarks>Captions should be as short as feasible, typically less than 80 characters.  Descriptions can be considerably longer, but neither should
    /// have embedded formatting outside of normal carriage return and line feed.</remarks>
    internal interface IDisplayable
    {
        /// <summary>
        /// A short end-user display caption 
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// An extended description without formatting.
        /// </summary>
        string Description { get; }
    }
}
