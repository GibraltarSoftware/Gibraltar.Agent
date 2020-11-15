
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Event arguments for controls that provide a set of selected items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectedItemsEventArgs<T> : EventArgs
    {
        private readonly ReadOnlyCollection<T> m_SelectedItems;

        /// <summary>
        /// Create a new event arguments object with the provided selected items
        /// </summary>
        /// <param name="selectedItems"></param>
        public SelectedItemsEventArgs(IList<T> selectedItems)
        {
            m_SelectedItems = new ReadOnlyCollection<T>(selectedItems);
        }

        /// <summary>
        /// A read-only collection of selected items.
        /// </summary>
        public IList<T> SelectedItems { get { return m_SelectedItems; } }
    }
}