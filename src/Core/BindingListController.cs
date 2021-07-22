
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Gibraltar.Data;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Internal;
using Loupe.Extensibility.Data;

namespace Gibraltar
{
    /// <summary>
    /// Ensures safe, single-threaded access to a BindingList with intelligent
    /// updating of a downstream hierarchy of FilteredBindingList objects.
    /// </summary>
    /// <typeparam name="TList">The specific type of list containing the items</typeparam>
    /// <typeparam name="TItem">The type of item referenced in the BindingLists</typeparam>
    public class BindingListController<TList, TItem>
        where TList : FilteredBindingList<TItem>
    {
        private readonly TList m_RootList; // Root-level FilteredBindingList exposed by this controller
        private readonly IDynamicDataSource<TList> m_DataSource; // Actual source of updates to our Inbound binding list
        private ISynchronizeInvoke m_SyncObject; // Used to marshal updates of Outbound binding list to the proper thread
        private readonly TList m_InboundList; // Represents the inbound list as of the last update from our data source
        private readonly TList m_OutboundList; // We update the outbound list batching together changes to our inbound list

        private readonly object m_Lock = new object(); // Used for locks around critical sections
        private readonly object m_StatusLock = new object(); //used for summary status items.
        private readonly Queue<ListChangedEventArgs> m_EventQueue; // list of changes to the inbound list that must be reflected in our outbound list

        // These next few delegates are just here so we can get around declaring a delegate on each event
        private readonly SynchronizeListDelegate m_SynchronizeOutboundListDelegate;

        private readonly string m_LogCategory;

        private bool m_NeedsRefresh; // Indicates that we need to call refresh on our data source PROTECTED BY STATUSLOCK
        private bool m_Refreshing; // True while we're in the process of refreshing PROTECTED BY STATUSLOCK
        private DateTimeOffset m_LastRefresh; //the last time we *started* loading data.  PROTECTED BY STATUSLOCK

        private bool m_NeedsReset; // Indicates that a full reset of the outbound list is needed PROTECTED BY LOCK
        private int m_AddCount; // Number of add events in the pending queue PROTECTED BY LOCK
        private int m_ChangeCount; // Number of change events in the pending queue PROTECTED BY LOCK
        private int m_DeleteCount; // Number of delete events in the pending queue PROTECTED BY LOCK

        // This constant is used to decide whether we process queued events individually or
        // substitute a full reset instead.
        private const int ResetEventThreshold = 250; // Use reset if this many events or more are queued


        /// <summary>
        /// Create a controller binding it to the read data source and the syncObject associated with the
        /// thread to use for downstream UI updates.
        /// </summary>
        /// <param name="dataSource">This is the object that will update this controller's inbound BindingList</param>
        /// <param name="syncObject">This object implies the thread to marshal to for executing downstream updates</param>
        /// <param name="listArgs">List of additional arguments to be passed to the TList constructor</param>
        public BindingListController(IDynamicDataSource<TList> dataSource, ISynchronizeInvoke syncObject, params object[] listArgs)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            if (listArgs == null)
                listArgs = new object[0];

            // Store parameters for later use
            m_DataSource = dataSource;
            m_SyncObject = syncObject;
            m_LogCategory = dataSource.LogCategory;

            // Create BindingLists for inbound (i.e. upstream) changes from the data source,
            // and outbound (i.e. downstream) changes to be propagated to a hierarchy of
            // FilteredBindingList objects
            m_InboundList = CreateList(listArgs);
            m_OutboundList = CreateList(listArgs);

            // Subscribe to events caused by changes our data source makes to our inbound list
            m_InboundList.ListChanged += InboundListListChangeHandler;

            // Initialize delegates once because it's a tad more efficient than doing it on every event
            m_SynchronizeOutboundListDelegate = SynchronizeOutboundList;

            // Create a queue to hold pending list changed events
            m_EventQueue = new Queue<ListChangedEventArgs>();

            // Initialize state to reflect that we need a refresh to initialize our BindingList with the data source
            m_LastRefresh = DateTimeOffset.MinValue;
            m_NeedsRefresh = true;
            m_NeedsReset = true;


            TList list = CreateList(listArgs);
            m_RootList = list;
        }

        /// <summary>
        /// Create the root FilteredBindingList managed by this controller
        /// </summary>
        private TList CreateList(object[] listArgs)
        {
            // OK, so this looks really odd, so let me explain what's going on...
            // 1. We want FilteredBindingList<T> to be an abstract class so derived types can extend it
            // 2. We want those derived types to only be created by factory methods and not expose public constructors
            // 3. This controller needs to create an instance of the derived type to be our root list
            // 4. We may need to pass arguments to the constructor of the derived type
            // So... we use reflection to invoke a constructor of the derived class we might not otherwise have access to
            // and we pass along any extra arguments it might need. That root list will save those arguments and pass them
            // along to any further child lists it creates.

            var paramTypes = new Type[] { typeof(BindingListEx<TItem>), typeof(Predicate<TItem>), typeof(bool), listArgs.GetType() };
            var paramValues = new object[] { m_OutboundList, null, true, listArgs };

            var t = typeof(TList);
            var ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
            if (ci == null)
            {
                var typeName = t.Name;
                var msg = "Missing constructor: " + typeName + "(BindingListEx<T> dataSource, Predicate<T> filter, bool getUpdates, params object[] args)";
                throw new ArgumentException(msg);
            }
            return (TList)ci.Invoke(paramValues);
        }

        /// <summary>
        /// The synchronization object (if not provided when the list was created)
        /// </summary>
        /// <remarks>
        /// Ideally, this would be set in the constructor and never changed again.
        /// But, as a practical matter, sometimes we don't have the needed window handles
        /// initialized at the time we want to create our controllers so we need a means
        /// to set the thread marshaling object later.
        /// </remarks>
        public ISynchronizeInvoke SyncObject { get { return m_SyncObject; } set { m_SyncObject = value; } }

        /// <summary>
        /// Returns the root list associated with this controller
        /// </summary>
        public TList RootList { get { return m_RootList; } }

        /// <summary>
        /// Enables enhanced logging including of every synchronization event.
        /// </summary>
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// Thread-safe method to flag that data changes may require a refresh
        /// </summary>
        public void MarkDirty()
        {
            lock (m_StatusLock)
            {
                m_NeedsRefresh = true;
                System.Threading.Monitor.PulseAll(m_StatusLock);
            }
        }

        /// <summary>
        /// Indicates that the controller is not fully synchronized with the underlying IDynamicDataSource
        /// </summary>
        public bool IsDirty
        {
            get
            {
                lock (m_StatusLock)
                {
                    System.Threading.Monitor.PulseAll(m_StatusLock);

                    return m_NeedsRefresh || m_Refreshing;
                }
            }
        }

        /// <summary>
        /// The last time the repository data was refreshed from the underlying database
        /// </summary>
        public DateTimeOffset LastRefresh
        {
            get
            {
                lock (m_StatusLock)
                {
                    System.Threading.Monitor.PulseAll(m_StatusLock);

                    return m_LastRefresh;
                }
            }
        }

        /// <summary>
        /// Indicates if the initial session data has been loaded
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                lock (m_StatusLock)
                {
                    if (m_LastRefresh == DateTimeOffset.MinValue)
                        return false;

                    return true;
                }
            }
        }

        /// <summary>
        /// Thread-safe method to force data source to check for changes and initiate UI refresh of any changes
        /// </summary>
        public void Refresh(bool async = true, bool force = false)
        {
            //for optimal concurrency we're going to separate the checking if we really need to refresh
            //from performing the refresh.  That way not all threads are blocked by one thread requesting 
            //a synchronous refresh.
            bool performRefresh = false;
            lock (m_StatusLock)
            {
                //if we're already refreshing then we will queue another refresh for after this one in a force scenario
                if (m_Refreshing)
                {
                    m_NeedsRefresh = m_NeedsRefresh || force; //we could be setting it, or it could already be set.
                    if (async == false)
                    {
                        //we need to stall and wait for the refresh to finish since the caller is expecting new data
                        while (m_Refreshing)
                        {
                            System.Threading.Monitor.Wait(m_StatusLock, 1000);
                        }
                    }
                }
                else if (force || m_NeedsRefresh)
                {
                    performRefresh = true;
                    m_Refreshing = true;
                }

                System.Threading.Monitor.PulseAll(m_StatusLock);
            }

            if (performRefresh)
            {
                if (async)
                    ThreadPool.QueueUserWorkItem(state => AsyncProcessRefresh());
                else
                    ProcessRefresh();
            }
        }

        /// <summary>
        /// Worker thread method that actually performs the refresh
        /// </summary>
        private void AsyncProcessRefresh()
        {
            try
            {
                ProcessRefresh();
            }
            catch (Exception ex)
            {
                //we already logged it, we just need to keep the background thread from failing.
                GC.KeepAlive(ex);
            }
        }

        private OperationMetric GetMetric(string message)
        {
            if (VerboseLogging)
                return new OperationMetric(m_LogCategory, message);

            return null;
        }

        /// <summary>
        /// Worker thread method that actually performs the refresh
        /// </summary>
        private void ProcessRefresh()
        {
            using (GetMetric("Refresh binding list"))
            {
                try
                {
                    //we want to peg our last refresh to this, the *start* of the refresh to be sure we capture everything.
                    lock (m_StatusLock)
                    {
                        m_NeedsRefresh = false;
                        m_LastRefresh = DateTimeOffset.Now;

                        System.Threading.Monitor.PulseAll(m_StatusLock);
                    }

                    // Initialize our internal objects in a critical section
                    // It's probably not needed, but it's worth it to be extra careful
                    lock (m_Lock)
                    {
                        // Initialize flags before asking data source to refresh
                        m_NeedsReset = false;
                        m_AddCount = 0;
                        m_ChangeCount = 0;
                        m_DeleteCount = 0;
#if DUPLICATE_TEST
                        if (m_EventQueue.Count > 0) Debugger.Break();
#endif
                    }

                    // Ask the data source to refresh our inbound list.
                    // This will result in change events being queued.
                    // We don't hold the lock here because it might take
                    // a long time and we wouldn't want to block other
                    // threads that call Refresh or other methods
                    IList<IDynamicData> deferredUpdates;
                    m_DataSource.Synchronize(m_InboundList, out deferredUpdates);

                    // Finally, initiate processing of the UI update, if needed.
                    // We hold the lock here because we're messing with boolean flags.
                    // However, the actual processing of changes to the outbound list
                    // will be marshaled to another thread and done asynchronously.
                    // Therefore, we won't be holding this lock very long -- just
                    // long enough to initiate that request.
                    bool refreshCompleted = true;
                    lock (m_Lock)
                    {
                        if (m_NeedsReset || (m_EventQueue.Count > 0) ||
                            ((deferredUpdates != null) && (deferredUpdates.Count > 0)))
                        {
                            refreshCompleted = false;
                            ListChangedEventArgs[] changeQueue = m_EventQueue.ToArray();
                            m_EventQueue.Clear(); //because we don't want to see these events again.

                            ProcessEvents(changeQueue, deferredUpdates); // m_Refreshing not cleared until marshaled call completes
                        }
                    }

                    //in the cases where we don't have any more events to process, we're done refreshing now and
                    //we can take new changes.
                    if (refreshCompleted)
                    {
                        lock (m_StatusLock)
                        {
                            m_Refreshing = false;

                            System.Threading.Monitor.PulseAll(m_StatusLock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, m_LogCategory, string.Format("Unable to refresh the binding list from the data source due to {0} exception", ex.GetType()), "Changes up to the point of the failure will be reflected at a later refresh, and the collection is still considered dirty.\r\nException Details: {0}", ex.Message);

                    lock (m_StatusLock)
                    {
                        // Reset this true because we didn't complete the refresh process
                        m_NeedsRefresh = true;
                        m_Refreshing = false;

                        System.Threading.Monitor.PulseAll(m_StatusLock);
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Process list changed events initiated by our data source during a call to its Refresh method
        /// </summary>
        private void InboundListListChangeHandler(object sender, ListChangedEventArgs e)
        {
            lock (m_Lock)
            {
                // If we've already decided we're going to do a reset
                // we can ignore subsequent events
                if (m_NeedsReset)
                    return;

                // Handle the event based on it's event type
                switch (e.ListChangedType)
                {
                    case ListChangedType.Reset:
                    case ListChangedType.PropertyDescriptorAdded:
                    case ListChangedType.PropertyDescriptorChanged:
                    case ListChangedType.PropertyDescriptorDeleted:
                        m_NeedsReset = true;
                        m_EventQueue.Clear();
                        return; // Return in this case, not break, because nothing needs to be queued

                    case ListChangedType.ItemAdded:
#if DUPLICATE_TEST
                        if (m_InboundList.Count < 20)
                        {
                            if (m_OutboundList.Contains(m_InboundList[e.NewIndex]))
                                Debugger.Break();

                            foreach (var args in m_EventQueue)
                            {
                                if (args.ListChangedType == ListChangedType.ItemAdded)
                                {
                                    var enhancedArgs = args as ListChangedEventArgsItemAdded<TItem>;
                                    if (enhancedArgs != null)
                                    {
                                        if (ReferenceEquals(enhancedArgs.Item, m_InboundList[e.NewIndex])) Debugger.Break();
                                    }
                                }
                            }
/*
                            var newItem = m_InboundList[e.NewIndex] as ISessionSummary;

                            if (newItem != null)
                            {
                                foreach (ISessionSummary outboundItem in m_OutboundList)
                                {
                                    if (outboundItem.Id == newItem.Id)
                                        Debugger.Break();
                                }
                            }*/
                        }
#endif
                        m_EventQueue.Enqueue(new ListChangedEventArgsItemAdded<TItem>(m_InboundList[e.NewIndex], e.NewIndex));
                        m_AddCount++;
                        break;

                    case ListChangedType.ItemChanged:
                        m_EventQueue.Enqueue(e);
                        m_ChangeCount++;
                        break;

                    case ListChangedType.ItemDeleted:
                        if (e is ListChangedEventArgsItemDeleted<TItem>)
                        {
                            m_EventQueue.Enqueue(e);
                            m_DeleteCount++;
                        }
                        break;

                    case ListChangedType.ItemMoved:
                        throw new NotImplementedException("ListChangedType.ItemMoved is not supported");
                }

                // Another little optimization... once we've exceeded the event threshold, don't worry
                // about queuing events -- we're just going to reset anyway
                if (m_EventQueue.Count > ResetEventThreshold)
                {
                    m_NeedsReset = true;
                    m_EventQueue.Clear();
                }
            }
        }

        /// <summary>
        /// Perform thread marshaling to request updates to the outbound list
        /// on the appropriate thread.
        /// </summary>
        private void ProcessEvents(IList<ListChangedEventArgs> changeEvents, IList<IDynamicData> deferredUpdates)
        {
            if ((m_SyncObject == null) || (m_SyncObject.InvokeRequired == false))
            {
                SynchronizeOutboundList(changeEvents, deferredUpdates);
            }
            else
            {
                m_SyncObject.BeginInvoke(m_SynchronizeOutboundListDelegate, new object[] { changeEvents, deferredUpdates });
            }
        }

        /// <summary>
        /// Synchronize the outbound list
        /// </summary>
        /// <remarks>
        /// This method will be called on the proper thread because ProcessEvents handled marshaling.
        /// </remarks>
        private void SynchronizeOutboundList(IList<ListChangedEventArgs> changeEvents, IList<IDynamicData> deferredUpdates)
        {
            using (GetMetric("Synchronize Outbound list"))
            {
                try
                {
                    m_OutboundList.SynchronizationStart();

                    int deferredCount = (deferredUpdates == null) ? 0 : deferredUpdates.Count;

                    if (m_NeedsReset ||
                        ((m_AddCount + m_ChangeCount + m_DeleteCount + deferredCount) >= ResetEventThreshold))
                        ProcessResetEvent(deferredUpdates);
                    else
                        ProcessPendingEvents(changeEvents, deferredUpdates);
                }
                catch (Exception ex)
                {
                    Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, m_LogCategory, string.Format("Unable to synchronize the outbound list from the data source due to {0} exception", ex.GetType()), "Some changes might be reflected, but any other pending change will be lost.\r\nException Details: {0}", ex.Message);
                }
                finally
                {
                    lock (m_Lock)
                    {
                        m_NeedsReset = false;
                        m_AddCount = 0;
                        m_ChangeCount = 0;
                        m_DeleteCount = 0;
                    }

                    lock (m_StatusLock)
                    {
                        m_Refreshing = false; // Do this one last because it affects logic significantly
                        System.Threading.Monitor.PulseAll(m_StatusLock);
                    }

                    m_OutboundList.SynchronizationEnd();
                }
            }
        }

        /// <summary>
        /// Process all the pending events in the queue
        /// </summary>
        private void ProcessPendingEvents(IList<ListChangedEventArgs> changeEvents, IList<IDynamicData> deferredUpdates)
        {
            //the order here isn't critical but has a side effect- we don't want to double-issue
            //the change events for deferred updates.
            foreach (var eventArgs in changeEvents)
            {
                try
                {
                    switch (eventArgs.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
#if DUPLICATE_TEST
                            var newItem = ((ListChangedEventArgsItemAdded<TItem>)eventArgs).Item;
                            if (m_OutboundList.Contains(newItem))
                            {
                                var existingItem = m_OutboundList[m_OutboundList.IndexOf(newItem)];
                                bool referenceEqual = ReferenceEquals(newItem, existingItem);
                                Debugger.Break();
                            }
#endif
                            m_OutboundList.Add(((ListChangedEventArgsItemAdded<TItem>)eventArgs).Item);
                            break;

                        case ListChangedType.ItemChanged:
                            m_OutboundList.ResetItem(eventArgs.NewIndex);
                            break;

                        case ListChangedType.ItemDeleted:
                            var deletedItem = ((ListChangedEventArgsItemDeleted<TItem>)eventArgs).Item;
                            var index = m_OutboundList.IndexOf(deletedItem);
                            if (index >= 0)
                                m_OutboundList.RemoveAt(index);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, m_LogCategory, "Failed to process " + eventArgs.ListChangedType + " event change for list due to " + ex.GetType() + " exception", "We will continue with other events but the outbound list may be in an unexpected state.\r\nEvent: {0}\r\n{1}: {2}", eventArgs, ex.GetType(), ex.Message);
                }
            }

            //Now process all of our deferred updates. These will tend to sympathetically create more queued entries
            if (deferredUpdates != null)
            {
                foreach (var deferredUpdate in deferredUpdates)
                {
                    deferredUpdate.CommitChanges(m_SyncObject);
                }
            }

            //we've done all the queued items we care about, whatever's left is from the deferral process
            //and already issued implicitly on the outbound collections.
            lock (m_Lock)
            {
                m_EventQueue.Clear();
            }
        }

        private void ProcessResetEvent(IList<IDynamicData> deferredUpdates)
        {
            //Now, anything we're deferring we can now dump.  We've already captured that we'll have to reset...
            if (deferredUpdates != null)
            {
                foreach (var deferredUpdate in deferredUpdates)
                {
                    deferredUpdate.Clear(); // we don't want it to issue its item changed event
                }
            }

            m_OutboundList.RaiseListChangedEvents = false; // temporarily suspend automatic notifications
            // We know the value was true because that was checked earlier
            try
            {
                m_OutboundList.Clear(); // Clear the downstream list in prep for adding everything back in
                foreach (var item in m_InboundList)
                {
                    m_OutboundList.Add(item);
                }
            }
            finally
            {
                //order here is important.
                m_OutboundList.RaiseListChangedEvents = true; // Re-enable eventing
                m_OutboundList.ResetBindings(); // Raise the Reset event on the downstream list
            }
        }
    }

    internal delegate void SynchronizeListDelegate(IList<ListChangedEventArgs> changeEvents, IList<IDynamicData> deferredUpdates);
}
