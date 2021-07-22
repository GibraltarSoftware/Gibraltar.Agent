
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Gibraltar
{
    /// <summary>
    /// Implements a private thread pool separate from the main pool
    /// </summary>
    public class PrivateThreadPool: IDisposable
    {
        #region Private Class ThreadPoolRequest

        /// <summary>
        /// A single request that has been queued
        /// </summary>
        private class ThreadPoolRequest
        {
            public ThreadPoolRequest(WaitCallback callBack, object state)
            {
                CallBack = callBack;
                State = state;
                QueueEntry = DateTimeOffset.Now;
            }

            /// <summary>
            /// When the request was queued
            /// </summary>
            public DateTimeOffset QueueEntry { get; private set; }

            /// <summary>
            /// The delegate to execute
            /// </summary>
            public WaitCallback CallBack { get; private set; }

            /// <summary>
            /// Any state information provided to the object
            /// </summary>
            public object State { get; private set; }
        }

        #endregion

        private readonly object m_ThreadLock = new object();
        private readonly object m_QueueLock = new object();
        private readonly List<Thread> m_Threads = new List<Thread>(); //PROTECTED BY THREADLOCK
        private readonly Queue<ThreadPoolRequest> m_Requests = new Queue<ThreadPoolRequest>(); //PROTECTED BY QUEUELOCK

        private readonly string m_ThreadNamePrefix;
        private int m_MaxThreads;
        private int m_MinThreads;
        private ThreadPriority m_ThreadPriority;
        private volatile bool m_ShuttingDown;

        /// <summary>
        /// Raised when the thread pool is being shut down.
        /// </summary>
        public event EventHandler ShuttingDown;

        /// <summary>
        /// Raised when a background task throws an exception during execution
        /// </summary>
        public event PrivateTaskErrorEventHandler TaskException;

        /// <summary>
        /// Create a new private thread pool
        /// </summary>
        /// <param name="namePrefix">A name to use for each worker thread (a number will be appended)</param>
        /// <param name="maxThreads">The maximum number of threads to create</param>
        /// <param name="minThreads">The minimum number of threads to keep active</param>
        public PrivateThreadPool(string namePrefix, int maxThreads, int minThreads)
        {
            m_ThreadNamePrefix = string.IsNullOrEmpty(namePrefix) ? "Private Worker Thread": namePrefix;
            m_MaxThreads = maxThreads;
            m_MinThreads = minThreads;
        }

        /// <summary>
        /// The maximum number of threads to have active at a time
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return m_MaxThreads;
            }
            set
            {
                m_MaxThreads = value;
            }
        }

        /// <summary>
        /// The minimum number of threads to have active at a time
        /// </summary>
        public int MinThreads
        {
            get
            {
                return m_MinThreads;
            }
            set
            {
                m_MinThreads = value;
            }
        }

        /// <summary>
        /// The priority to use for the thread pool worker threads
        /// </summary>
        public ThreadPriority ThreadPriority
        {
            get
            {
                return m_ThreadPriority;
            }
            set
            {
                SetThreadPriority(value);
            }
        }

        /// <summary>
        /// Queues a method for execution. The method executes when a thread pool thread becomes available. 
        /// </summary>
        /// <param name="callBack">A WaitCallback representing the method to execute. </param>
        /// <returns>true if the method is successfully queued; OutOfMemoryException is thrown if the work item could not be queued.</returns>
        public bool QueueWorkItem(WaitCallback callBack)
        {
            return QueueWorkItem(callBack, null);
        }

        /// <summary>
        /// Queues a method for execution, and specifies an object containing data to be used by the method. The method executes when a thread pool thread becomes available. 
        /// </summary>
        /// <param name="callBack">A WaitCallback representing the method to execute. </param>
        /// <param name="state">An object containing data to be used by the method.</param>
        /// <returns>true if the method is successfully queued; OutOfMemoryException is thrown if the work item could not be queued.</returns>
        public bool QueueWorkItem(WaitCallback callBack, object state)
        {
            lock (m_QueueLock)
            {
                m_Requests.Enqueue(new ThreadPoolRequest(callBack, state));

                System.Threading.Monitor.PulseAll(m_QueueLock);
            }

            EnsureRunning();

            return true;
        }

        /// <summary>
        /// Stop processing on the threadpool
        /// </summary>
        /// <param name="gracePeriod">The number of milliseconds to let the threads gracefully exit before terminating them.</param>
        public void Shutdown(int gracePeriod)
        {
            bool performShutdown = false;

            //dump all of the current requests so threads will go idle as we are working to kill them all.
            lock(m_QueueLock)
            {
                if (m_ShuttingDown == false)
                {
                    performShutdown = true;

                    m_ShuttingDown = true;
                    m_Requests.Clear();
                }

                System.Threading.Monitor.PulseAll(m_QueueLock);
            }

            //if we're the one true thread that got here first, we get to raise the shutting down event.
            if (performShutdown)
            {
                OnShuttingDown();
            }

            //now we stall up to the grace period to see if they shut down nicely on their own.
            DateTimeOffset hardKillDate = DateTimeOffset.Now.AddMilliseconds(gracePeriod);
            lock(m_ThreadLock)
            {
                while((m_Threads.Count > 0) && (DateTimeOffset.Now < hardKillDate))
                {
                    System.Threading.Monitor.Wait(m_ThreadLock, 16);
                }

                System.Threading.Monitor.PulseAll(m_ThreadLock);
            }

            if (performShutdown)
            {
                //Kill any thread that's still running.
                List<Thread> runningThreads = null;
                lock(m_ThreadLock)
                {
                    runningThreads = new List<Thread>(m_Threads);

                    System.Threading.Monitor.PulseAll(m_ThreadLock);
                }

                foreach (Thread thread in runningThreads)
                {
                    try
                    {
                        thread.Abort();
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        #region Protected Properties and Methods

        /// <summary>
        /// Raises the ShuttingDown event
        /// </summary>
        protected virtual void OnShuttingDown()
        {
            EventHandler tempEvent = ShuttingDown;
            if (tempEvent != null)
            {
                try
                {
                    tempEvent(this, EventArgs.Empty);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Raises the TaskException event
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ex"></param>
        protected virtual void OnTaskException(object state, Exception ex)
        {
            PrivateTaskErrorEventHandler tempEvent = TaskException;
            if (tempEvent != null)
            {
                try
                {
                    tempEvent(this, new PrivateTaskErrorEventArgs(state, ex));
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// The execution loop for each background worker thread
        /// </summary>
        /// <param name="state"></param>
        private void AsyncWorkerThreadMain(object state)
        {
            try
            {
                while (m_ShuttingDown == false)
                {
                    ThreadPoolRequest nextRequest = null;

                    //see if there's an item to work on.
                    lock (m_QueueLock)
                    {
                        if (m_Requests.Count > 0)
                        {
                            nextRequest = m_Requests.Dequeue();
                        }
                        System.Threading.Monitor.PulseAll(m_QueueLock);
                    }

                    if (nextRequest != null)
                    {
                        ExecuteThreadPoolTask(nextRequest);
                    }

                    //now it's time to rest unless there are more items queued.
                    lock (m_QueueLock)
                    {
                        while ((m_ShuttingDown == false) && (m_Requests.Count == 0))
                        {
                            System.Threading.Monitor.Wait(m_QueueLock, 1000); //the timeout really should never come into play, but is there as a guarantee
                        }

                        System.Threading.Monitor.PulseAll(m_QueueLock);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Background task engine thread failed due to internal exception:\r\n{0}", ex.Message, ex);
            }
            finally
            {
                //since we're exiting, mark that the analysis thread has failed and clear our tracking variable.
                lock (m_ThreadLock)
                {
                    m_Threads.Remove(Thread.CurrentThread);

                    System.Threading.Monitor.PulseAll(m_ThreadLock);
                }

                //if we aren't shutting down then kick over a new thread.
                if (m_ShuttingDown == false)
                {
                    EnsureRunning();
                }
            }            
        }

        /// <summary>
        /// Execute the task on the current thread.
        /// </summary>
        /// <param name="request"></param>
        private void ExecuteThreadPoolTask(ThreadPoolRequest request)
        {            
            try
            {
                request.CallBack(request.State);
            }
            catch (Exception ex)
            {
                OnTaskException(request.State, ex);
            }
        }

        /// <summary>
        /// Makes sure at least our minimum number of threads are running.
        /// </summary>
        private void EnsureRunning()
        {
            lock (m_ThreadLock)
            {
                while (m_Threads.Count < MinThreads)
                {
                    //we have to create a new thread to get up to our count.
                    int threadNumber = m_Threads.Count + 1;
                    bool foundDuplicate = false;
                    string threadName;
                    do
                    {
                        threadName = string.Format("{0} {1}", m_ThreadNamePrefix, threadNumber);

                        //but...there may already be one with that id.
                        foreach (Thread thread in m_Threads)
                        {
                            if (thread.Name.Equals(threadName, StringComparison.OrdinalIgnoreCase))
                            {
                                foundDuplicate = true;
                                break;
                            }
                        }

                        if (foundDuplicate)
                        {
                            //increment and try again
                            threadNumber++;
                        }
                    } while (foundDuplicate);

                    Thread newThread = new Thread(AsyncWorkerThreadMain);
                    newThread.Name = threadName;
                    newThread.TrySetApartmentState(ApartmentState.MTA);
                    newThread.IsBackground = true; //we aren't going to keep the app running
                    newThread.Priority = m_ThreadPriority;
                    newThread.Start();
                    m_Threads.Add(newThread);
                }

                System.Threading.Monitor.PulseAll(m_ThreadLock);
            }
        }

        private void SetThreadPriority(ThreadPriority newValue)
        {
            lock(m_ThreadLock)
            {
                if (m_ThreadPriority == newValue)
                    return;

                m_ThreadPriority = newValue;
                foreach (Thread thread in m_Threads)
                {
                    thread.Priority = m_ThreadPriority;
                }

                System.Threading.Monitor.PulseAll(m_ThreadLock);
            }
        }

        #endregion
    }
}
