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
using System.ComponentModel;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Slightly extended BindingList to resolve problem with delete event
    /// </summary>
    /// <typeparam name="T">Type of item in list</typeparam>
    /// <remarks>
    /// Workaround as suggested here:
    /// http://connect.microsoft.com/VisualStudio/feedback/details/148506/listchangedtype-itemdeleted-is-useless-because-listchangedeventargs-newindex-is-already-gone
    /// </remarks>
    [Serializable]
    public class BindingListEx<T> : BindingList<T>
    {
        /// <summary>
        /// Raised when the binding list is starting a batch of updates
        /// </summary>
        public event EventHandler BeginUpdate;

        /// <summary>
        /// Raised when the binding list is ending a batch of updates
        /// </summary>
        public event EventHandler EndUpdate;
     

        #region Internal Properties and Methods

        /// <summary>
        /// Called the synchronization process is starting and the BeginUpdate event should be raised
        /// </summary>
        internal void SynchronizationStart()
        {
            OnBeginUpdate(EventArgs.Empty);
        }

        /// <summary>
        /// Signals the synchronization process is complete and the EndUpdate event should be raised.
        /// </summary>
        internal void SynchronizationEnd()
        {
            OnEndUpdate(EventArgs.Empty);
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Called to raise the BeginUpdate event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnBeginUpdate(EventArgs e)
        {
            var handler = BeginUpdate;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Called to raise the EndUpdate event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEndUpdate(EventArgs e)
        {
            var handler = EndUpdate;
            handler?.Invoke(this, e);
        }
   
        /// <summary>
        /// This method extends BindingList in a way that doesn't suck with regard to removing items
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            T victim = this[index];
            var deleted = new ListChangedEventArgsItemDeleted<T>(victim, index);
            OnListChanged(deleted);
            base.RemoveItem(index);
        }

        #endregion
    }

    /// <summary>
    /// This subclass allows us to pass a reference to the actual item being deleted
    /// </summary>
    [Serializable]
    public class ListChangedEventArgsItemDeleted<T> : ListChangedEventArgs
    {
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="item">The item being deleted</param>
        /// <param name="index">The index of the item being deleted, for backward compatibility.</param>
        internal ListChangedEventArgsItemDeleted(T item, int index)
            : base(ListChangedType.ItemDeleted, index, index)
        {
            Item = item;
        }

        /// <summary>
        /// This is the item that has been deleted from the collection.
        /// </summary>
        public T Item { get; private set; }
    }

    /// <summary>
    /// This subclass allows us to pass a reference to the actual item being added
    /// </summary>
    [Serializable]
    public class ListChangedEventArgsItemAdded<T> : ListChangedEventArgs
    {
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="item">The item being added</param>
        /// <param name="index">The index of the item being added, for backward compatibility.</param>
        internal ListChangedEventArgsItemAdded(T item, int index)
            : base(ListChangedType.ItemAdded, index, index)
        {
            Item = item;
        }

        /// <summary>
        /// This is the item that has been deleted from the collection.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var label = new StringBuilder(1024);
            label.AppendFormat("Change Type: {0} New Index: {1:N0} Old Index: {2:N0} Property Descriptor: {3}\r\n", ListChangedType, NewIndex, OldIndex, PropertyDescriptor);

            var objItem = Item as object;
            if (objItem == null)
            {
                label.AppendFormat("{0} Item: (Null)", typeof(T));
            }
            else
            {
                label.AppendFormat("{0} Item: {1}", typeof(T), Item);
            }

            return label.ToString();
        }
    }

}
