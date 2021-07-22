
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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Gibraltar
{
    /// <summary>
    /// Specialized BindingList that projects the list of distinct values from a property of another BindingList
    /// </summary>
    /// <typeparam name="TItem">The type of items in the source list</typeparam>
    /// <typeparam name="TValue">The type of the projected value</typeparam>
    public class DistinctValueList<TItem, TValue> : BindingList<NameValuePair<TValue>>, IBindingListListener<TItem>, IDisposable
        where TValue : class, IEquatable<TValue>, IComparable<TValue>
    {
        private class LookupEntry<TValue1>
        {
            public int Count { get; set; }
            public NameValuePair<TValue1> NameValue { get; set; }
        }

        /// <summary>
        /// Delegate to a function that can return a display caption for a value
        /// </summary>
        /// <param name="item">Parent object the value came from</param>
        /// <param name="value">The value to be converted to a string caption</param>
        /// <returns>String caption for the passed value</returns>
        public delegate string FormatCaption(TItem item, TValue value);

        private readonly PropertyInfo m_PropertyInfo;
        private readonly FormatCaption m_FormatCaption;
        private readonly Dictionary<TValue, LookupEntry<TValue>> m_Lookup;
        private readonly bool m_IncludeAll;
        private readonly bool m_IncludeNull;
        private readonly IEqualityComparer<TValue> m_Comparer;
        private int m_NullCount;
        private bool m_Updating;

        /// <summary>
        /// Create a specialized BindingList projected the list of distinct values associated with 
        /// </summary>
        /// <param name="valuePropertyName">Property name used to retrieve value</param>
        /// <param name="includeAll">True if list should contain an entry for "(All)"</param>
        /// <param name="includeNull">True if list should contain an entry for "(None)"</param>
        /// <param name="comparer">Optional IEqualityComparer for determining whether an item is distinct</param>
        /// <param name="formatCaption">Optional function to convert value into a caption string, otherwise ToString used.</param>
        public DistinctValueList(string valuePropertyName, bool includeAll, bool includeNull = true,
                                 IEqualityComparer<TValue> comparer = null, FormatCaption formatCaption = null)
        {
            var ti = typeof (TItem);
            var tv = typeof (TValue);
            m_PropertyInfo = ti.GetProperty(valuePropertyName);

            if (m_PropertyInfo == null)
                throw new ArgumentException(string.Format("Could not find a public property {0}.{1}", ti.Name,
                                                          valuePropertyName));

            if (!tv.IsAssignableFrom(m_PropertyInfo.PropertyType))
                throw new ArgumentException(string.Format("Expected {0}.{1} to return type {2}", ti.Name,
                                                          valuePropertyName, tv.Name));
            //we also do direct comparisons with the comparer, so be ready for that..
            m_Comparer = comparer ?? EqualityComparer<TValue>.Default;

            // Create the lookup dictionary using the custom comparer, if provided
            m_Lookup = new Dictionary<TValue, LookupEntry<TValue>>(m_Comparer);

            m_FormatCaption = formatCaption;
            m_IncludeAll = includeAll;
            m_IncludeNull = includeNull;
            m_NullCount = 0;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            RaiseListChangedEvents = false;
            Clear();
        }

        /// <summary>
        /// Should be called before making updates to the list
        /// </summary>
        public void BeginUpdate()
        {
            Debug.Assert(m_Updating == false, "BeginUpdate called twice with no intervening EndUpdate");
            m_Updating = true;
        }

        /// <summary>
        /// Should be called after completing a set of updates to the list
        /// </summary>
        public void EndUpdate()
        {
            Debug.Assert(m_Updating == true, "EndUpdate called without a preceding call to BeginUpdate");

            var list = new List<NameValuePair<TValue>>(m_Lookup.Count); //it's at least that..
            
            //add the data items first - we need to sort them.
            foreach (var entry in m_Lookup.Values)
                list.Add(entry.NameValue);

            //if it's a string mode then we have to sort differently.
            if (typeof(TValue) == typeof(string))
            {
                list.Sort((a, b) => CompareValues(a.Name, b.Name));
            }
            else
            {
                list.Sort((a, b) => CompareValues(a.Value, b.Value)); //we can't use the default type.
            }
            
            //now prepend our optional override items...
            if (m_IncludeNull && m_NullCount > 0)
                list.Insert(0, new NameValuePair<TValue>("(None)", null));

            if (m_IncludeAll)
                list.Insert(0, new NameValuePair<TValue>("(All)", null));

            Synchronize(list);
            m_Updating = false;
        }

        private int CompareValues(string left, string right)
        {
            return string.Compare(left, right, StringComparison.CurrentCultureIgnoreCase);
        }

        private int CompareValues(TValue target, TValue other)
        {
            int comparison;

            //use the IComparable implementation, which is non-generic, so we have to do null checks first.
            if (ReferenceEquals(target, null))
            {
                if (ReferenceEquals(other, null))
                    comparison = 0;
                else
                    //anything is greater than null.
                    comparison = -1;
            }
            else
            {
                comparison = target.CompareTo(other);
            }

            return comparison;
        }

        /// <summary>
        /// Should be called at the start of a reset on the reference BindingList
        /// </summary>
        public void Reset()
        {
            m_Lookup.Clear();
            m_NullCount = 0;
        }

        /// <summary>
        /// Inspect the item and add another value to our list if we don't already have it
        /// </summary>
        /// <param name="item">Item that might include another distinct property value</param>
        /// <remarks>This method should be called between BeginUpdate and EndUpdate</remarks>
        public void Add(TItem item)
        {
            // Start by reading the value of the property
            var value = (TValue)m_PropertyInfo.GetValue(item, null);

            // For null values, just increment a counter.
            // For none-null values, check if we've already seen them before bothering to format a caption
            // If it's new, add it to the dictionary. Otherwise, increment our reference count

            if (m_Comparer.Equals(value, null))
            {
                m_NullCount++;
            }
            else
            {
                LookupEntry<TValue> entry;
                if (m_Lookup.TryGetValue(value, out entry))
                {
                    entry.Count++;
                }
                else
                {
                    var caption = m_FormatCaption == null ? value.ToString() : m_FormatCaption(item, value);
                    var nameValuePair = new NameValuePair<TValue>(caption, value);
                    entry = new LookupEntry<TValue> {Count = 1, NameValue = nameValuePair};
                    m_Lookup.Add(value, entry);
                }
            }
        }

        /// <summary>
        /// Indicates if the distinct value list contains the specified value.
        /// </summary>
        /// <param name="value">The value to check for</param>
        /// <returns>True if the item is currently present, false otherwise</returns>
        public bool Contains(TValue value)
        {
            return m_Lookup.ContainsKey(value);
        }

        /// <summary>
        /// Inspect the item and remove it from our list if it was the last reference to that value
        /// </summary>
        /// <param name="item">Item that might be the last reference to a distinct property value</param>
        /// <remarks>This method should be called between BeginUpdate and EndUpdate</remarks>
        public void Remove(TItem item)
        {
            // Start by reading the value of the property
            var value = (TValue) m_PropertyInfo.GetValue(item, null);

            // Decrement the appropriate counter associated with the property value
            // If the reference count for a value drops to zero, remove it from the dictionary
            if (value == null)
            {
                m_NullCount--;
                Debug.Assert(m_NullCount >= 0, "Reference count dropped below zero for null value" );
            }
            else
            {
                LookupEntry<TValue> entry;
                if (m_Lookup.TryGetValue(value, out entry))
                {
                    entry.Count--;
                    if (entry.Count <= 0)
                        m_Lookup.Remove(value);

                    Debug.Assert(entry.Count >= 0, "Reference count dropped below zero for \"" + entry.NameValue.Name + "\"");
                }
                else
                {
                    // NOTE: This shouldn't happen. But throwing exception is too harsh for production
                    var caption = m_FormatCaption == null ? value.ToString() : m_FormatCaption(item, value);
                    Debug.Assert(true, "Attempt to remove an item we hadn't seen before: \"" + caption + "\"");
                }
            }
        }

        /// <summary>
        /// Called during a sequence of updates when an item is added to the list
        /// </summary>
        public void Change(TItem item)
        {
            //we're hoping nothing you do a distinct value on is changing..
        }

        /// <summary>
        /// Synchronize the list with a specific list of name value pairs
        /// </summary>
        /// <remarks>
        /// This method assumes that the list is already in sorted order.
        /// Typically, this method is called internally by EndUpdate as part of
        /// the logic to process a set of changes to a FilteredBindingList
        /// </remarks>
        public void Synchronize(List<NameValuePair<TValue>> list)
        {
            bool done = false;
            var newListIndex = 0;
            var oldListIndex = 0;

            while (!done)
            {
                if (newListIndex < list.Count)
                {
                    if (oldListIndex < Count)
                    {
                        // If both lists still have items, compare the caption strings
                        NameValuePair<TValue> newEntry = list[newListIndex];
                        NameValuePair<TValue> oldEntry = this[oldListIndex];

                        int comparison = 0;

                        if (typeof(TValue) == typeof(string))
                        {
                            comparison = CompareValues(newEntry.Name, oldEntry.Name);
                        }
                        else
                        {
                            comparison = CompareValues(newEntry.Value, oldEntry.Value); 
                        }

                        if (comparison == 0)
                        {
                            // If the items are equal, there is no change. Advance both indexes
                            newListIndex++;
                            oldListIndex++;
                        }
                        else if (comparison < 0)
                        {
                            // If the newItem is earlier, then it needs to be inserted before the other
                            Insert(oldListIndex, newEntry);
                            newListIndex++;
                            oldListIndex++; //because *now* the item we just inserted is in the old list at the index position.
                        }
                        else
                        {
                            // If the newItem is later, then the older item should be removed
                            RemoveAt(oldListIndex);
                        }
                    }
                    else
                    {
                        // If we still have new items and no more old items, insert at the end
                        Insert(Count, list[newListIndex]);
                        newListIndex++;
                        oldListIndex++; //because we just inserted, changing effectively where the current pointer is.
                    }
                }
                else
                {
                    if (oldListIndex < Count)
                    {
                        // If we've run out of new items, remove the remaining old items
                        RemoveAt(oldListIndex);
                    }
                    else
                    {
                        // Finally!  We've gotten to the end of both lists
                        done = true;
                    }
                }
            }
        }
    }
}
