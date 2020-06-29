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

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    /// <summary>
    /// A collection of comments, ordered by date/time and accessible by unique ID or numerical index.
    /// </summary>
    public class CommentCollection : IList<Comment>
    {
        //comment itself implements IComparable, so it will determine order
        private readonly SortedList<Comment, Comment> m_List = new SortedList<Comment, Comment>(); // LOCKED
        private readonly Dictionary<Guid, Comment> m_Dictionary = new Dictionary<Guid, Comment>(); // LOCKED
        private readonly object m_Lock = new object(); // Apparently Dictionaries and Lists are not internally threadsafe.

        /// <summary>
        /// Raised every time the collection's contents are changed to allow subscribers to automatically track changes.
        /// </summary>
        public event EventHandler<CollectionChangedEventArgs<CommentCollection, Comment>> CollectionChanged;


        #region Protected Properties and Methods

        /// <summary>
        /// Called whenever the collection changes.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>Note to inheritors:  If overriding this method, you must call the base implementation to ensure
        /// that the appropriate events are raised.</remarks>
        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<CommentCollection, Comment> e)
        {
            //save the delegate field in a temporary field for thread safety
            EventHandler<CollectionChangedEventArgs<CommentCollection, Comment>> tempEvent = CollectionChanged;

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
        public bool TryGetValue(Guid key, out Comment value)
        {
            lock (m_Lock)
            {
                //gateway to our inner dictionary try get value
                return m_Dictionary.TryGetValue(key, out value);
            }
        }

        #endregion

        #region IEnumerable<Comment> Members

        IEnumerator<Comment> IEnumerable<Comment>.GetEnumerator()
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

        #region IList<Comment> Members

        public int IndexOf(Comment item)
        {
            lock (m_Lock)
            {
                return m_List.IndexOfKey(item);
            }
        }

        public void Insert(int index, Comment item)
        {
            //we don't support setting an object by index; we are sorted.
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            Comment victim;
            lock (m_Lock)
            {
                //find the item at the requested location
                victim = m_List.Values[index];
            }

            //and pass that to our normal remove method.  Must be outside the lock because it must fire an event.
            Remove(victim);
        }

        public Comment this[int index]
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
                //we don't want to support setting an object by index, we are sorted.
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<Comment> Members

        /// <summary>
        /// Add the specified Comment item to the collection
        /// </summary>
        /// <param name="item">The new Comment item to add</param>
        public void Add(Comment item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "A new comment item must be provided to add it to the collection.");
            }

            lock (m_Lock)
            {
                //add it to both lookup collections
                m_Dictionary.Add(item.Id, item);
                m_List.Add(item, item);
            }

            //and fire our event
            OnCollectionChanged(new CollectionChangedEventArgs<CommentCollection, Comment>(this, item, CollectionAction.Added));
        }

        /// <summary>
        /// Clear the entire contents of the comment collection
        /// </summary>
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
                //and raise the event (outside the lock) so our caller knows we're cleared
                OnCollectionChanged(new CollectionChangedEventArgs<CommentCollection, Comment>(this, null, CollectionAction.Cleared));
            }
        }

        /// <summary>
        /// Indicates if the supplied collection object is present in the collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Comment item)
        {
            lock (m_Lock)
            {
                //here we are relying on the fact that the comment object implements IComparable sufficiently to guarantee uniqueness
                return m_List.ContainsKey(item);
            }
        }

        public void CopyTo(Comment[] array, int arrayIndex)
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
        /// Remove the specified Comment item.  If the Comment isn't in the collection, no exception is thrown.
        /// </summary>
        /// <param name="item">The Comment item to remove.</param>
        public bool Remove(Comment item)
        {
            bool result;

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "A comment item must be provided to remove it from the collection.");
            }

            lock (m_Lock)
            {
                //we have to remove it from both collections, and we better not raise an error if not there.
                result = m_Dictionary.Remove(item.Id);

                //here we are relying on the IComparable implementation being a unique key and being fast.
                result |= m_List.Remove(item);
            }

            //and fire our event if there was really something to remove
            if (result)
            {
                OnCollectionChanged( new CollectionChangedEventArgs<CommentCollection, Comment>(this, item, CollectionAction.Removed));
            }

            return result;
        }


        #endregion
    }

}
