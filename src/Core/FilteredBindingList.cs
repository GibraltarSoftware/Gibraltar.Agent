using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Gibraltar
{
    /// <summary>
    /// Provides a filtered subset of an upstream BindingList.
    /// </summary>
    /// <typeparam name="T">The type of item in the BindingList</typeparam>
    public abstract class FilteredBindingList<T> : BindingListEx<T>, IDisposable
    {
        private readonly List<IBindingListListener<T>> m_Listeners = new List<IBindingListListener<T>>();
        private readonly BindingListEx<T> m_DataSource;
        private readonly object[] m_Args; // These are passed in our constructor and passed on to any child lists we create

        private Predicate<T> m_Filter;

        /// <summary>
        /// Creates an filtered BindingList based on applying the filter predicate to the referenced data source
        /// </summary>
        /// <param name="dataSource">Upstream BindingList of data to be filtered</param>
        /// <param name="filter">Predicate that returns true for items to expose downstream</param>
        /// <param name="getUpdates">If true, register for updates. Otherwise, returned list is static</param>
        /// <param name="args">List of additional parameters needed by derived classes</param>
        /// <remarks>
        /// This constructor is protected to only be visible to the subclasses. Those subclasses need to expose
        /// a constructor accepting these same arguments. The constructor of the subclasses can be private, 
        /// protected or public because it will be invoked through reflection.
        /// </remarks>
        protected FilteredBindingList(BindingListEx<T> dataSource, Predicate<T> filter, bool getUpdates, params object[] args)
        {
            // Store parameters.  Order here may be important for performance/null handling, so watch carefully!
            Filter = filter; // Set the properties to include special handling of null.
            m_DataSource = dataSource;
            m_Args = args;

            AllowNew = false;
            AllowEdit = m_DataSource == null ? true : m_DataSource.AllowEdit;
            AllowRemove = m_DataSource == null ? true : m_DataSource.AllowRemove;

            if (m_DataSource != null)
            {
                Reset();

                // Register for future change events, if requested
                if (getUpdates)
                {
                    m_DataSource.BeginUpdate += SourceBeginUpdate;
                    m_DataSource.ListChanged += SourceListChanged;
                    m_DataSource.EndUpdate += SourceEndUpdate;
                }
            }
        }


        #region Public Properties and Methods

        /// <summary>
        /// Return a child FilteredBindingList that will be dynamically updated
        /// </summary>
        /// <param name="filter">Additional filter to be applied to child list</param>
        public FilteredBindingList<T> Clone(Predicate<T> filter = null)
        {
            return CreateChildList(filter, true);
        }

        /// <summary>
        /// Return a static child FilteredBindingList that will not be dynamically updated
        /// </summary>
        /// <param name="filter">Additional filter to be applied to child list</param>
        public FilteredBindingList<T> Find(Predicate<T> filter)
        {
            return CreateChildList(filter, false);
        }

        private FilteredBindingList<T> CreateChildList(Predicate<T> filter, bool getUpdates)
        {
            // Create the root FilteredBindingList managed by this controller
            // We use reflection to invoke private constructor
            var paramTypes = new Type[] { GetType(), typeof(Predicate<T>), typeof(bool), m_Args.GetType() };
            var paramValues = new object[] { this, filter, getUpdates, m_Args };

            Type t = GetType(); // Invoke constructor of our derived type
            ConstructorInfo ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            return (FilteredBindingList<T>)ci.Invoke(paramValues);
        }

        /// <summary>
        /// Predicate specifying the items to pass through
        /// </summary>
        public Predicate<T> Filter
        {
            get { return m_Filter; }
            set
            {
                // Ensure that filter is never set to null
                if (value == null)
                    value = (item => true);

                //since the filter is a method we don't know that it being set to the same value *wont* change the outcome.
                //so we have to proceed and process it every time...
                m_Filter = value;

                if (m_DataSource == null)
                    return;

                Reset();
            }
        }

        /// <summary>
        /// Register a listener to be notified of changes to this list
        /// </summary>
        public void Register(IBindingListListener<T> listener)
        {
            m_Listeners.Add(listener);
            ResetOne(listener);
        }

        /// <summary>
        /// UnRegister a previously registered listener to no longer be notified of changes to this list
        /// </summary>
        public void UnRegister(IBindingListListener<T> listener)
        {
            m_Listeners.Remove(listener);
        }

        /// <summary>
        /// Expose access to the protected OnListChanged method so it can be invoked by ListChangeBuffer.
        /// </summary>
        public void SendListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (m_DataSource != null)
            {
                m_DataSource.BeginUpdate -= SourceBeginUpdate;
                m_DataSource.ListChanged -= SourceListChanged;
                m_DataSource.EndUpdate -= SourceEndUpdate;
            }

            m_Filter = null;

            m_Listeners.Clear();

            //if we really want to be free we have to release all of our event handles to property changed events.
            RaiseListChangedEvents = false;
            Clear();
        }

        #endregion

        #region Private Properties and Methods

        private void SourceBeginUpdate(object sender, EventArgs e)
        {
            OnBeginUpdate(e);
        }

        private void SourceEndUpdate(object sender, EventArgs e)
        {
            OnEndUpdate(e);
        }

        /// <summary>
        /// Event handler invoked when the upstream data source changes
        /// </summary>
        private void SourceListChanged(object sender, ListChangedEventArgs e)
        {
            if (!RaiseListChangedEvents)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    Reset();
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    OnListChanged(e);
                    break;

                case ListChangedType.ItemAdded:
                    AddItem(e);
                    break;

                case ListChangedType.ItemChanged:
                    ChangeItem(e.NewIndex);
                    break;

                case ListChangedType.ItemDeleted:
                    DeleteItem(e);
                    break;

                case ListChangedType.ItemMoved:
                    MoveItem(e.OldIndex, e.NewIndex);
                    break;
            }
        }

/* Removed because it turns out it's too expensive to check for adds, removes, changes.
        /// <summary>
        /// Handles a full reset of this list to batch a large number of upstream changes
        /// </summary>
        private void ProcessResetEvent()
        {
            var upstreamIndex = 0;
            var downstreamIndex = 0;
            var raiseEvent = false;
            var done = false;

            // loop through the entire upstream collection checking if it changed in a material way
            while (!done)
            {
                // Once we've gone through all the upstream events, we're done (one way or the other)
                if (upstreamIndex < m_DataSource.Count)
                {
                    T item = m_DataSource[upstreamIndex];
                    if (m_Filter(item))
                    {
                        // If this item should be included downstream, check if it is in fact
                        // the next downstream object.
                        if (downstreamIndex < Count && ReferenceEquals(item, this[downstreamIndex]))
                        {
                            // if so, advance both list indexes
                            upstreamIndex++;
                            downstreamIndex++;
                        }
                        else
                        {
                            // if the next downstream item is not equal to the current upstream item,
                            // something has either been added or removed. Either way, we'll pass on the rest
                            done = true;
                            raiseEvent = true;
                        }
                    }
                    else
                    {
                        // Quietly skip over filtered upstream items
                        upstreamIndex++;
                    }
                }
                else
                {
                    // If we've run out of upstream items but still have downstream items,
                    // we should pass on the reset event to clear them
                    if (downstreamIndex < Count)
                        raiseEvent = true;
                    done = true;
                }
            }

            // This is the logic to pass on the reset event, if appropriate
            if (raiseEvent)
            {
                Reset();
            }
        }
*/

        /// <summary>
        /// Reset our downstream list
        /// </summary>
        private void Reset()
        {
            var prevRaiseListChangedEvents = RaiseListChangedEvents;
            RaiseListChangedEvents = false; // temporarily suspend automatic notifications

            OnBeginUpdate(EventArgs.Empty); //so people that directly subscribe to us know we're about to do something major.
            try
            {
                Clear(); // Clear the downstream list in prep for adding everything back in
                var activeListeners = m_Listeners.ToArray();
                foreach (var listener in activeListeners)
                {
                    listener.BeginUpdate();
                    listener.Reset();
                }

                foreach (var item in m_DataSource)
                {
                    if (m_Filter == null || m_Filter(item))
                    {
                        foreach (var listener in activeListeners)
                            listener.Add(item);

                        Add(item);
                    }
                }

                foreach (var listener in activeListeners)
                {
                    listener.EndUpdate();
                }
            }
            finally
            {
                //ORDER HERE IS IMPORTANT, it will introduce subtle bugs to change.
                RaiseListChangedEvents = prevRaiseListChangedEvents; // Re-enable eventing if it was before.
                ResetBindings(); // Raise the Reset event on the downstream list
                OnEndUpdate(EventArgs.Empty); //so people that directly subscribe to us know we're done doing something major.
            }
        }

        /// <summary>
        /// Reset just one downstream listener without affecting the others (used to initialize a listener)
        /// </summary>
        /// <param name="listener"></param>
        private void ResetOne(IBindingListListener<T> listener)
        {
            listener.BeginUpdate();
            listener.Reset();

            //just iterate over our outbound list since it's already filtered and we aren't changing it..
            foreach (var item in this)
            {
                listener.Add(item);
            }

            listener.EndUpdate();
        }

        /// <summary>
        /// Handle the ListChangedEvent for an added item
        /// </summary>
        private void AddItem(ListChangedEventArgs e)
        {
            T item = m_DataSource[e.NewIndex];
            if (m_Filter(item))
            {
                foreach (var listener in m_Listeners)
                {
                    listener.BeginUpdate();
                    listener.Add(item);
                    listener.EndUpdate();
                }

                Add(item);
            }
        }

        /// <summary>
        /// Handle the ListChangedEvent for a changed item
        /// </summary>
        private void ChangeItem(int upstreamIndex)
        {
            T item = m_DataSource[upstreamIndex];
            var downstreamIndex = IndexOf(item);
            bool wasIncluded = downstreamIndex >= 0;
            bool isIncluded = m_Filter(item);

            if (isIncluded)
            {
                foreach (var listener in m_Listeners)
                {
                    listener.BeginUpdate();
                    listener.Change(item);
                    listener.EndUpdate();
                }
                
                if (wasIncluded)
                {
                    // If the item was not filtered before and isn't filtered now, raise a changed event
                    var e = new ListChangedEventArgs(ListChangedType.ItemChanged, downstreamIndex);
                    OnListChanged(e);
                }
                else
                {
                    // If the item was filtered before, but changed so that it isn't filtered, add it
                    Add(item);
                }
            }
            else
            {
                if (wasIncluded)
                {
                    // If the change to the item now causes it to be filtered, remove it

                    foreach (var listener in m_Listeners)
                    {
                        listener.BeginUpdate();
                        listener.Remove(item);
                        listener.EndUpdate();
                    }
                    RemoveAt(downstreamIndex);
                }
                else
                {
                    // If the item was already filtered and still is filtered, there's nothing to do
                    return;
                }
            }
        }

        /// <summary>
        /// Handle the ListChangedEvent for a deleted item
        /// </summary>
        private void DeleteItem(ListChangedEventArgs e)
        {
            // Delete events are kinda broken in BindingList<T>. The event is invoked AFTER the item has
            // been deleted and includes a (now) useless index to the location where it USED TO BE.
            // We resolved this by overriding delete behavior in BindingListEx to raise an subclass of
            // ListChangedEventArgs that includes a reference to the deleted item.
            // 
            // However, we end getting called twice: once by our derived class and once by the base
            // BindingList<T>.  We work around this with the IF statement below that processes our
            // extended event and ignores the useless event raised by the base class.
            if (e is ListChangedEventArgsItemDeleted<T>)
            {
                // Process the event raised by BindingListEx
                T item = ((ListChangedEventArgsItemDeleted<T>)e).Item;
                var index = IndexOf(item);
                if (index >= 0)
                {
                    foreach (var listener in m_Listeners)
                    {
                        listener.BeginUpdate();
                        listener.Remove(item);
                        listener.EndUpdate();
                    }

                    RemoveItem(index);
                }
            }
            else
            {
                // Ignore the default events raised by the base BindingList
            }
        }

        /// <summary>
        /// Handle the ListChangedEvent for a moved item -- NOT SUPPORTED
        /// </summary>
        private void MoveItem(int oldIndex, int newIndex)
        {
            // This doesn't really occur in the real world
            // http://stackoverflow.com/questions/1236983/what-causes-a-listchangedtype-itemmoved-listchange-event-in-a-bindinglistt
            throw new NotImplementedException("ItemMoved event is not supported by FilteredBindingList");
        }

        #endregion
    }
}