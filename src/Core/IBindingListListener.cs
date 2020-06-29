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
using System.Collections.Generic;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Interface allowing an object to be notified of changes to a FilteredBindingList
    /// </summary>
    /// <typeparam name="T">The type of item in the FilteredBindingList</typeparam>
// ReSharper disable TypeParameterCanBeVariant
    public interface IBindingListListener<T>
// ReSharper restore TypeParameterCanBeVariant
    {
        /// <summary>
        /// Called before the first of a sequence of changes
        /// </summary>
        void BeginUpdate();

        /// <summary>
        /// Called after the last of a sequence of changes
        /// </summary>
        void EndUpdate();

        /// <summary>
        /// Called to indicate that the list is changing entirely
        /// </summary>
        void Reset();

        /// <summary>
        /// Called during a sequence of updates when an item is added to the list
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Called during a sequence of updates when an item is updated that's in the list already
        /// </summary>
        void Change(T item);

        /// <summary>
        /// Called during a sequence of updates when an item is removed from the list
        /// </summary>
        void Remove(T item);
    }
}
