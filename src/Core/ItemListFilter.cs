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
    /// Implements a filter for a filtered binding list that translates from a list of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ItemListFilter<T>
    {
        private readonly FilteredBindingList<T> m_Target;
        private readonly ListContains<T> m_Comparer;
        private readonly Predicate<T> m_Predicate;

        private IList<T> m_List;

        /// <summary>
        /// Filter the target list to only include items from a specified list (based on the Items property of this class).
        /// </summary>
        /// <param name="target">List to be filtered</param>
        /// <param name="comparer">Comparison function, or null to use default comparison.</param>
        public ItemListFilter(FilteredBindingList<T> target = null, ListContains<T> comparer = null)
        {
            m_Target = target;
            m_Comparer = comparer;

            //this completely gratuitous optimization brought to you by the letter I and the number 1.
            m_Predicate = (m_Comparer == null) ? (Predicate<T>)PredicateDefaultComparer : (Predicate<T>)PredicateWithComparer;

            if (m_Target != null) m_Target.Filter = m_Predicate;
        }

        /// <summary>
        /// The set of items that are to be included when evaluating the filter.
        /// </summary>
        /// <remarks>setting this property will cause the related binding list to be immediately re-evaluated</remarks>
        public IList<T> Items
        {
            get { return m_List; }
            set
            {
                if (ReferenceEquals(m_List, value))
                    return;

                m_List = value;
                if (m_Target != null) m_Target.Filter = m_Predicate; //this causes the target to re-evaluate.
            }
        }

        /// <summary>
        /// The predicate used to match with this item list filter.
        /// </summary>
        public Predicate<T> Predicate { get { return m_Predicate; } } 

        /// <summary>
        /// Our filter predicate
        /// </summary>
        /// <param name="obj">The object being tested</param>
        /// <returns>True if in our list, false otherwise</returns>
        private bool PredicateWithComparer(T obj)
        {
            if ((m_List == null) || (m_List.Count == 0))
                return false;

            return m_Comparer(m_List, obj);
        }

        /// <summary>
        /// Our filter predicate
        /// </summary>
        /// <param name="obj">The object being tested</param>
        /// <returns>True if in our list, false otherwise</returns>
        private bool PredicateDefaultComparer(T obj)
        {
            if ((m_List == null) || (m_List.Count == 0))
                return false;

            return m_List.Contains(obj);
        }
    }

    /// <summary>
    /// Custom test to see if the list contains the item provided
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list of items to check</param>
    /// <param name="item">The item being looked for</param>
    /// <remarks>You can specify a higher performance comparison method knowing the
    /// constraints of the list or items than the default IList Contains.</remarks>
    /// <returns>True if the list contains the item, false otherwise</returns>
    public delegate bool ListContains<T>(IList<T> list, T item);
}
