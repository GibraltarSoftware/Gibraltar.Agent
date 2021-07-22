
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Gibraltar
{
    /// <summary>
    /// Maintain a queue of events between two FilteredBindingLists
    /// </summary>
    /// <remarks>
    /// This class is is not thread-safe.  It assumes that a BindingListController (or some other controlling entity) has
    /// full responsibility for concurrency management and thread marshalling
    /// </remarks>
    public class ListChangeBuffer<T>
    {
        private readonly FilteredBindingList<T> m_Input;
        private readonly FilteredBindingList<T> m_Output;
        private readonly Queue<ListChangedEventArgs> m_EventQueue; // list of changes to the inbound list that must be reflected in our outbound list
        private bool m_NeedsReset;      // Indicates that a full reset of the outbound list is needed
        private int m_AddCount;         // Number of add events in the pending queue
        private int m_ChangeCount;      // Number of change events in the pending queue
        private int m_DeleteCount;      // Number of delete events in the pending queue

        // This constant is used to decide whether we process queued events individually or
        // substitute a full reset instead.
        private const int ResetEventThreshold = 50; // Use reset if this many events or more are queued

        /// <summary>
        /// Initialize the object binding to the specified input and output lists
        /// </summary>
        public ListChangeBuffer(FilteredBindingList<T> input, FilteredBindingList<T> output, bool queueingEnabled = true)
        {
            // Store parameters
            m_Input = input;
            m_Output = output;
            QueueingEnabled = queueingEnabled;

            // Create a queue to hold pending list changed events
            m_EventQueue = new Queue<ListChangedEventArgs>();

            // Subscribe to events caused by changes our data source makes to our inbound list
            m_Input.ListChanged += InputChangeHandler;
        }

        /// <summary>
        /// Enable or disable queueing of events.
        /// </summary>
        /// <remarks>
        /// When queueing is disabled, change events on the input list are immediately 
        /// dispatched to the output list.
        /// </remarks>
        public bool QueueingEnabled { get; set; }

        /// <summary>
        /// Update the output list to match the input list by processing all queued events
        /// </summary>
        public void ProcessEvents()
        {
            if (m_NeedsReset || m_EventQueue.Count > 0)
            {
                try
                {
                    if (m_NeedsReset || m_AddCount + m_ChangeCount + m_DeleteCount >= ResetEventThreshold)
                        ProcessResetEvent();
                    else
                        ProcessPendingEvents();
                }
                catch (Exception ex)
                {
                    // TODO: Need exception handling
                    GC.KeepAlive(ex);
                }
                finally
                {
                    m_NeedsReset = false;
                    m_AddCount = 0;
                    m_ChangeCount = 0;
                    m_DeleteCount = 0;
                }
            }
        }

        /// <summary>
        /// Process list changed events initiated by our data source during a call to its Refresh method
        /// </summary>
        private void InputChangeHandler(object sender, ListChangedEventArgs e)
        {
            // If we're not queueing, just pass the event and get out
            if (!QueueingEnabled)
            {
                m_Output.SendListChanged(e);
                return;
            }

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
                    m_EventQueue.Enqueue(new ListChangedEventArgsItemAdded<T>(m_Input[e.NewIndex], e.NewIndex));
                    m_AddCount++;
                    break;

                case ListChangedType.ItemChanged:
                    m_EventQueue.Enqueue(e);
                    m_ChangeCount++;
                    break;

                case ListChangedType.ItemDeleted:
                    if (e is ListChangedEventArgsItemDeleted<T>)
                    {
                        m_EventQueue.Enqueue(e);
                        m_DeleteCount++;
                    }
                    break;

                case ListChangedType.ItemMoved:
                    throw new NotImplementedException("ListChangedType.ItemMoved is not supported");
            }

            // Another little optimization... once we've exceeded the event threshold, don't worry
            // about queueing events -- we're just going to reset anyway
            if (m_EventQueue.Count > ResetEventThreshold)
            {
                m_NeedsReset = true;
                m_EventQueue.Clear();
            }
        }

        /// <summary>
        /// Process a request to reset the output list
        /// </summary>
        /// <remarks>
        /// Check to be sure there actually are differences between input and output.
        /// If no diffs are found, do nothing.
        /// </remarks>
        private void ProcessResetEvent()
        {
            bool needsReset = false;

            if (m_Input.Count == m_Output.Count && m_ChangeCount == 0)
            {
                for (int i = 0; i < m_Input.Count; i++)
                {
                    if (!ReferenceEquals(m_Input[i], m_Output[i]))
                    {
                        needsReset = true;
                        break;
                    }
                }
            }
            else
            {
                needsReset = true;
            }

            if (needsReset)
            {
                m_Output.RaiseListChangedEvents = false; // temporarily suspend automatic notifications
                // We know the value was true because that was checked earlier

                try
                {
                    m_Output.Clear(); // Clear the downstream list in prep for adding everything back in
                    foreach (var item in m_Input)
                        m_Output.Add(item);
                }
                finally
                {
                    m_Output.RaiseListChangedEvents = true; // Re-enable eventing
                    m_Output.ResetBindings(); // Raise the Reset event on the downstream list
                }
            }
        }

        /// <summary>
        /// Process all the pending events in the queue
        /// </summary>
        private void ProcessPendingEvents()
        {
            foreach (var eventArgs in m_EventQueue)
            {
                switch (eventArgs.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                        m_Output.Add(((ListChangedEventArgsItemAdded<T>)eventArgs).Item);
                        break;

                    case ListChangedType.ItemChanged:
                        m_Output.ResetItem(eventArgs.NewIndex);
                        break;

                    case ListChangedType.ItemDeleted:
                        var deletedItem = ((ListChangedEventArgsItemDeleted<T>)eventArgs).Item;
                        var index = m_Output.IndexOf(deletedItem);
                        if (index >= 0)
                            m_Output.RemoveAt(index);
                        break;
                }
            }

            m_EventQueue.Clear();
        }
    }
}
