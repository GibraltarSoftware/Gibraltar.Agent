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
#region File Header

using System;
using System.Collections.Generic;
using Gibraltar.Monitor.Internal;

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    /// <summary>
    /// A collection of data extension objects that can be accessed by index or by unique ID.
    /// </summary>
    public class DataExtensionCollection : IList<DataExtension>
    {
        private readonly Analysis m_Analysis;
        private readonly Dictionary<Guid, DataExtension> m_Dictionary = new Dictionary<Guid, DataExtension>();
        private readonly List<DataExtension> m_List = new List<DataExtension>();
        private readonly object m_Lock = new object();

        public event EventHandler<CollectionChangedEventArgs<DataExtensionCollection, DataExtension>> CollectionChanged;

        public DataExtensionCollection(Analysis analysis)
        {
            //store off our analysis object
            m_Analysis = analysis;
        }

        /// <summary>
        /// Add a new data extension object for the specified target data object
        /// </summary>
        /// <param name="owner">The new data extension object to add</param>
        internal DataExtension Add(IDataObject owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "A data object must be provided to own the new data extension object.");
            }

            DataExtension newExtension;
            lock (m_Lock)
            {
                //make sure we don't already have it
                if (m_Dictionary.ContainsKey(owner.Id))
                {
                    throw new ArgumentException("There already exists a data extension object in the collection for the specified owner.", nameof(owner));
                }

                //we don't, create a new one and add it
                newExtension = new DataExtension(m_Analysis, owner);

                //add it to both lookup collections
                m_Dictionary.Add(newExtension.Id, newExtension);
                m_List.Add(newExtension);
            }

            //and fire our event outside the lock
            OnCollectionChanged(new CollectionChangedEventArgs<DataExtensionCollection, DataExtension>(this, newExtension, CollectionAction.Added));

            //then return the new object to our caller
            return newExtension;
        }

        #region Private Properties and Methods

        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<DataExtensionCollection, DataExtension> e)
        {
            //save the delegate field in a temporary field for thread safety
            EventHandler<CollectionChangedEventArgs<DataExtensionCollection, DataExtension>> tempEvent = CollectionChanged;

            if (tempEvent != null)
            {
                tempEvent(this, e);
            }
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection</param>
        /// <returns>true if the collection contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(Guid key)
        {
            lock (m_Lock)
            {
                //gateway to our inner dictionary 
                return m_Dictionary.ContainsKey(key);
            }
        }


        /// <summary>
        /// Retrieve an item from the collection by its key if present.  If not present, the default value of the object is returned.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the collection contains an element with the specified key; otherwise false.</returns>
        public bool TryGetValue(Guid key, out DataExtension value)
        {
            lock (m_Lock)
            {
                //gateway to our inner dictionary try get value
                return m_Dictionary.TryGetValue(key, out value);
            }
        }

        #endregion

        #region IEnumerable<DataExtension> Members

        IEnumerator<DataExtension> IEnumerable<DataExtension>.GetEnumerator()
        {
            //we use the list for enumeration
            return m_List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            //we use the list for enumeration
            return m_List.GetEnumerator();
        }

        #endregion

        #region IList<DataExtension> Members

        public int IndexOf(DataExtension item)
        {
            lock (m_Lock)
            {
                return m_List.IndexOf(item);
            }
        }

        public void Insert(int index, DataExtension item)
        {
            //we don't support setting an object by index; we are sorted.
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            DataExtension victim;
            lock (m_Lock)
            {
                //find the item at the requested location
                victim = m_List[index];
            }

            //and pass that to our normal remove method.  Must be outside the lock because it fires an event.
            Remove(victim);
        }

        public DataExtension this[int index]
        {
            get
            {
                lock (m_Lock)
                {
                    return m_List[index];
                }
            }
            set
            {
                //we don't want to support setting an object by index, we are sorted.
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<DataExtension> Members


        /// <summary>
        /// Add an existing DataExtension item to our collection.  It must be for the same analysis as this collection.
        /// </summary>
        /// <param name="item">The new DataExtension item to add.</param>
        public void Add(DataExtension item)
        {
            //we really don't want to support this method, but we have to for ICollection<T> compatibility.  So we're going to ruthlessly
            //verify that the data extension object was created correctly.

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            else if (item.Analysis != m_Analysis)
            {
                throw new ArgumentException("The data extension item was created for a different analysis and can't be associated with this collection.", nameof(item));
            }

            lock (m_Lock)
            {
                //make sure we don't already have it
                if (m_Dictionary.ContainsKey(item.Id))
                {
                    throw new ArgumentException("There already exists a data extension item in the collection for the specified owner.", nameof(item));
                }

                //add it to both lookup collections
                m_Dictionary.Add(item.Id, item);
                m_List.Add(item);
            }

            //and fire our event outside the lock
            OnCollectionChanged(new CollectionChangedEventArgs<DataExtensionCollection, DataExtension>(this, item, CollectionAction.Added));            
        }

        public void Clear()
        {
            int count;

            lock (m_Lock)
            {
                //Only do this if we HAVE something, since events are fired.
                count = m_List.Count;
                if (count > 0)
                {
                    m_List.Clear();
                    m_Dictionary.Clear();
                }
            }

            if (count > 0)
            {
                //and raise the event outside the lock (if anything changed) so our caller knows we're cleared
                OnCollectionChanged(new CollectionChangedEventArgs<DataExtensionCollection, DataExtension>(this, null, CollectionAction.Cleared));
            }
        }

        public bool Contains(DataExtension item)
        {
            lock (m_Lock)
            {
                //here we are relying on the fact that the comment object implements IComparable sufficiently to guarantee uniqueness
                return m_List.Contains(item);
            }
        }

        public void CopyTo(DataExtension[] array, int arrayIndex)
        {
            lock (m_Lock)
            {
                m_List.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (m_Lock)
                {
                    return m_List.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove the specified DataExtension item.  If the DataExtension isn't in the collection, no exception is thrown.
        /// </summary>
        /// <param name="item">The DataExtension item to remove.</param>
        public bool Remove(DataExtension item)
        {
            bool result;

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "A data extension item must be provided to remove it from the collection.");
            }

            lock (m_Lock)
            {
                //we have to remove it from both collections, and we better not raise an error if not there.
                result = m_Dictionary.Remove(item.Id);
                result |= m_List.Remove(item);
            }

            //and fire our event outside the lock if we changed something
            if (result)
            {
                OnCollectionChanged(new CollectionChangedEventArgs<DataExtensionCollection, DataExtension>(this, item, CollectionAction.Removed));
            }

            return result;
        }


        #endregion
    }
}
