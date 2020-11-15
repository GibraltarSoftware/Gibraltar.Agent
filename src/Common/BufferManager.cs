
using System;
using System.Collections.Generic;

namespace Gibraltar
{
    /// <summary>
    /// Provides a set of buffers for manageable memory reuse
    /// </summary>
    /// <remarks>All methods on the buffer manager are threadsafe.</remarks>
    public class BufferManager
    {
        private readonly object m_Lock = new object();
        private readonly Stack<byte[]> m_FreeIndexPool;
        private readonly int m_BufferSize;

        private long m_TotalBuffers;

        /// <summary>
        /// Indicates the buffer manager had to expand the buffer collection to satisfy new requests
        /// </summary>
        public event BufferSizeEventHandler BufferExpanded;

        /// <summary>
        /// Define the buffer characteristics
        /// </summary>
        /// <param name="initialBuffers">The number of buffers to start with</param>
        /// <param name="bufferSize">The number of bytes to use in each buffer</param>
        /// <remarks>The buffers are immediately created from a contiguous block
        /// of memory.</remarks>
        public BufferManager(int initialBuffers, int bufferSize)
        {
            m_BufferSize = bufferSize;
            m_FreeIndexPool = new Stack<byte[]>(initialBuffers);

            //this doesn't *really* make it in one block of RAM - we could do better
            //but it gets a lot more complicated and hard to make truly opaque.  This
            //is likely good enough for our purposes.
            for (int curBufferIndex = 0; curBufferIndex < initialBuffers; curBufferIndex++)
            {
                AddBuffer();
            }

            m_TotalBuffers = initialBuffers;
        }

        /// <summary>
        /// Sets a buffer from the buffer pool
        /// </summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public byte[] AllocateBuffer()
        {
            lock (m_Lock)
            {
                //first check if we can recycle a buffer...
                if (m_FreeIndexPool.Count == 0)
                    AddBuffer();

                return m_FreeIndexPool.Pop();
            }
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the buffer pool
        /// </summary>
        public void FreeBuffer(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            lock (m_Lock)
            {
                m_FreeIndexPool.Push(buffer);
            }
        }

        #region Protected Properties and Methods

        /// <summary>
        /// Called to raise the BufferExpanded event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnBufferExpanded(BufferSizeEventArgs e)
        {
            var tempEvent = BufferExpanded;
            if (tempEvent != null)
            {
                tempEvent.Invoke(this, e);
            }
        }

        #endregion

        #region Private Properties and Methods

        private void AddBuffer()
        {
            long totalBuffers;
            lock (m_Lock)
            {
                m_FreeIndexPool.Push(new byte[m_BufferSize]);
                m_TotalBuffers++;
                totalBuffers = m_TotalBuffers;
            }

            OnBufferExpanded(new BufferSizeEventArgs(totalBuffers, m_BufferSize));
        }

        #endregion
    }

    /// <summary>
    /// Buffer manager size event data
    /// </summary>
    public class BufferSizeEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new buffer size event arguments
        /// </summary>
        /// <param name="totalBuffers"></param>
        /// <param name="bufferSize"></param>
        public BufferSizeEventArgs(long totalBuffers, int bufferSize)
        {
            BufferCount = totalBuffers;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// The number of buffers associated with the buffer manager
        /// </summary>
        public long BufferCount { get; private set; }

        /// <summary>
        /// The number of bytes in each individual buffer
        /// </summary>
        public int BufferSize { get; private set; }

        /// <summary>
        /// The total number of bytes used by all buffers associated with the manager
        /// </summary>
        public long TotalBufferSize { get { return BufferCount * BufferSize; } }
    }

    /// <summary>
    /// Delegate definition for buffer size events
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    public delegate void BufferSizeEventHandler(object source, BufferSizeEventArgs e);
}
