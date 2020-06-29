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

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    /// <summary>
    /// A collection of marker objects that can be accessed by index or by unique ID.
    /// </summary>
    public class MarkerCollection : IList<Marker>
    {
        private readonly Analysis m_Analysis;
        private readonly Dictionary<Guid, Marker> m_Dictionary = new Dictionary<Guid, Marker>();
        private readonly SortedList<Marker, Marker> m_List = new SortedList<Marker, Marker>(); //Marker itself implements IComparable, so it will determine order
        private readonly object m_Lock = new object();

        public event EventHandler<CollectionChangedEventArgs<MarkerCollection, Marker>> CollectionChanged;

        public MarkerCollection(Analysis analysis)
        {
            //store off our analysis object
            m_Analysis = analysis;
        }

        /// <summary>
        /// Add a new marker object for the specified target data object
        /// </summary>
        public Marker Add()
        {
            //we don't, create a new one and add it
            Marker newMarker = new Marker(m_Analysis);

            lock (m_Lock)
            {
                //add it to both lookup collections
                m_Dictionary.Add(newMarker.Id, newMarker);
                m_List.Add(newMarker, newMarker);
            }

            //and fire our event outside the lock.
            OnCollectionChanged(new CollectionChangedEventArgs<MarkerCollection, Marker>(this, newMarker, CollectionAction.Added));

            //then return the new object to our caller
            return newMarker;
        }

        #region Private Properties and Methods

        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<MarkerCollection, Marker> e)
        {
            //save the delegate field in a temporary field for thread safety
            EventHandler<CollectionChangedEventArgs<MarkerCollection, Marker>> tempEvent = CollectionChanged;

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
        public bool TryGetValue(Guid key, out Marker value)
        {
            lock (m_Lock)
            {
                //gateway to our inner dictionary try get value
                return m_Dictionary.TryGetValue(key, out value);
            }
        }

        #endregion

        #region IEnumerable<Marker> Members

        IEnumerator<Marker> IEnumerable<Marker>.GetEnumerator()
        {
            //we use the sorted list for enumeration
            return m_List.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            //we use the sorted list for enumeration
            return m_List.GetEnumerator();
        }

        #endregion

        #region IList<Marker> Members

        public int IndexOf(Marker item)
        {
            lock (m_Lock)
            {
                return m_List.IndexOfKey(item);
            }
        }

        /// <summary>
        /// Not supported.  Items are always sorted by the marker's data.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Marker item)
        {
            //we don't support setting an object by index; we are sorted.
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            Marker victim;
            lock (m_Lock)
            {
                //find the item at the requested location
                victim = m_List.Values[index];
            }

            //and pass that to our normal remove method outside the lock because it fires an event
            Remove(victim);
        }

        /// <summary>
        /// Retrieve a marker by its numerical index.  Setting is not supported.
        /// </summary>
        /// <param name="index">The numerical index of the item to retrieve</param>
        /// <returns></returns>
        public Marker this[int index]
        {
            get
            {
                lock (m_Lock)
                {
                    return m_List.Values[index];
                }
            }
            set
            {
                //we don't want to support setting an object by index, we are sorted.  However we have to have this defined
                //to support IList.
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Retrieve a marker by its unique id.
        /// </summary>
        /// <param name="id">The unique Id of the item to retrieve</param>
        /// <returns></returns>
        public Marker this[Guid id]
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Dictionary[id];
                }
            }
        }
        #endregion

        #region ICollection<Marker> Members


        /// <summary>
        /// Add an existing Marker item to our collection.  It must be for the same analysis as this collection.
        /// </summary>
        /// <param name="item">The new Marker item to add.</param>
        public void Add(Marker item)
        {
            //we really don't want to support this method, but we have to for ICollection<T> compatibility.  So we're going to ruthlessly
            //verify that the marker object was created correctly.

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "A marker item must be provided to add it to the collection.");
            }
            else if (item.Analysis != m_Analysis)
            {
                throw new ArgumentException("The marker item was created for a different analysis and can't be associated with this collection.", nameof(item));
            }

            lock (m_Lock)
            {
                //make sure we don't already have it
                if (m_Dictionary.ContainsKey(item.Id))
                {
                    throw new ArgumentException("There already exists a marker item in the collection for the specified id.", nameof(item));
                }

                //add it to both lookup collections
                m_Dictionary.Add(item.Id, item);
                m_List.Add(item, item);
            }

            //and fire our event outside the lock
            OnCollectionChanged(new CollectionChangedEventArgs<MarkerCollection, Marker>(this, item, CollectionAction.Added));
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

            if (count > 0) {
                //and raise the event so our caller knows we're cleared
                OnCollectionChanged(new CollectionChangedEventArgs<MarkerCollection, Marker>(this, null, CollectionAction.Cleared));
            }
        }

        public bool Contains(Marker item)
        {
            lock (m_Lock)
            {
                //here we are relying on the fact that the comment object implements IComparable sufficiently to guarantee uniqueness
                return m_List.ContainsKey(item);
            }
        }

        public void CopyTo(Marker[] array, int arrayIndex)
        {
            lock (m_Lock)
            {
                m_List.Values.CopyTo(array, arrayIndex);
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
        /// Remove the specified Marker item.  If the Marker isn't in the collection, no exception is thrown.
        /// </summary>
        /// <param name="item">The Marker item to remove.</param>
        public bool Remove(Marker item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "A marker item must be provided to remove it from the collection.");
            }

            bool result;

            lock (m_Lock)
            {
                //we have to remove it from both collections, and we better not raise an error if not there.
                result = m_Dictionary.Remove(item.Id);

                //here we are relying on the IComparable implementation being a unique key and being fast.
                result |= m_List.Remove(item);
            }

            //and fire our event (outside the lock) if we changed something
            if (result)
            {
                OnCollectionChanged(new CollectionChangedEventArgs<MarkerCollection, Marker>(this, item, CollectionAction.Removed));
            }

            return result;
        }


        #endregion
    }
}
