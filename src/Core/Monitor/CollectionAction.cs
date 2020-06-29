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
namespace Gibraltar.Monitor
{
    /// <summary>
    /// The different possible actions that were performed on a collection
    /// </summary>
    public enum CollectionAction
    {
        /// <summary>
        /// No changes were made.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// An item was added to the collection.
        /// </summary>
        Added = 1,

        /// <summary>
        /// An item was removed from the collection.
        /// </summary>
        Removed = 2,

        /// <summary>
        /// An item was updated in the collection.
        /// </summary>
        Updated = 3,

        /// <summary>
        /// The entire collection was cleared.
        /// </summary>
        Cleared = 4
    }
}
