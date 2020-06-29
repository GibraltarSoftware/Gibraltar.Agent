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
using System.IO;
using Gibraltar.Serialization.Internal;
using ThreadMonitor = System.Threading.Monitor;

#endregion File Header

namespace Gibraltar.Serialization
{
    /// <summary>
    /// A memory stream using two small buffers for efficient seeking of reading
    /// </summary>
    public class BufferedMemoryStream : Stream
    {
        private const int BufferSize = 64 * 1024;
        private readonly Stream m_FileStream;
        private readonly SimpleMemoryStream m_MemoryStream1;
        private readonly SimpleMemoryStream m_MemoryStream2;
        private SimpleMemoryStream m_MemoryStream;
        private readonly byte[] m_ByteBuffer = new byte[1];
        private readonly object m_WaitObject = new object();
        private IAsyncResult m_ReadStatus = null;
        private readonly long m_Length;
        private long m_Position;

        public BufferedMemoryStream(Stream stream)
        {
            m_FileStream = stream;
            m_Length = m_FileStream.Length;
            m_Position = 0;
            m_MemoryStream1 = new SimpleMemoryStream(BufferSize);
            m_MemoryStream2 = new SimpleMemoryStream(BufferSize);
            m_MemoryStream = m_MemoryStream1;
            BeginRead();
        }

        private void BeginRead()
        {
            // Toggle the double buffering
            SimpleMemoryStream buffer = m_MemoryStream;
            m_MemoryStream = m_MemoryStream == m_MemoryStream1 ? m_MemoryStream2 : m_MemoryStream1;

            buffer.SetLength(0);
            buffer.Position = 0;
            byte[] array = buffer.GetBuffer();
            m_ReadStatus = m_FileStream.BeginRead(array, 0, BufferSize, ReadCompleteHandler, buffer);
        }

        private void ReadCompleteHandler(IAsyncResult ar)
        {
            if (!ar.IsCompleted)
                return;

            try
            {
                ThreadMonitor.Enter(m_WaitObject);
                SimpleMemoryStream buffer = (SimpleMemoryStream)ar.AsyncState;
                int bytesRead = m_FileStream.EndRead(ar);
                //Trace.WriteLine("EndRead to m_MemoryStream" + (buffer == m_MemoryStream1 ? "1" : "2"));
                buffer.SetLength(bytesRead);
                buffer.Position = 0;
                m_ReadStatus = null;
                ThreadMonitor.Pulse(m_WaitObject);
            }
            finally
            {
                ThreadMonitor.Exit(m_WaitObject);
            }
        }

        ///<summary>
        ///When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        ///</summary>
        ///
        ///<returns>
        ///The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        ///</returns>
        ///
        ///<param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param>
        ///<param name="count">The maximum number of bytes to be read from the current stream. </param>
        ///<param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source. </param>
        ///<exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        ///<exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        ///<exception cref="T:System.ArgumentNullException">buffer is null. </exception>
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // calculate how much data is available in the current buffer
            long bytesAvailable = m_MemoryStream.Length - m_MemoryStream.Position;

            // read the available data in the current buffer, if any
            int bytesRead = 0;
            if (bytesAvailable > 0)
                bytesRead = m_MemoryStream.Read(buffer, offset, count);

            //Trace.WriteLine("Read " + bytesRead + " bytes from m_MemoryStream" + (m_MemoryStream == m_MemoryStream1 ? "1" : "2"));

            // If we read the requested number of bytes, we're done
            if (bytesRead == count)
            {
                m_Position += bytesRead;
                return bytesRead;
            }

            // At this point, we have finished with the current buffer.  It's time to kick off
            // another read if there is more data in the file.  Also, check if there is
            // another buffer waiting for us.  If so, start reading from it.  If necessary,
            // Wait for the pending read to complete.
            try
            {
                // This logic needs to be synchronized with ReadCompleteHandler
                ThreadMonitor.Enter(m_WaitObject);

                // If a read is pending, let's wait for it to complete
                if (m_ReadStatus != null)
                    ThreadMonitor.Wait(m_WaitObject);

                // If there is more data in the file, kick off another async read
                if (m_FileStream.Position < m_FileStream.Length)
                    BeginRead();
                else
                    // if we've already read the last buffer, let's at least read it.
                    m_MemoryStream = m_MemoryStream == m_MemoryStream1 ? m_MemoryStream2 : m_MemoryStream1;

                // calculate how much data is available in the current buffer
                bytesAvailable = m_MemoryStream.Length - m_MemoryStream.Position;

                // read the available data in the current buffer, if any
                if (bytesAvailable > 0)
                {
                    // Get the additional data needed from the current buffer
                    int adjustedOffset = offset + bytesRead;
                    int remainingCount = count - bytesRead;
                    int additionalBytes = m_MemoryStream.Read(buffer, adjustedOffset, remainingCount);
                    //Trace.WriteLine("Read additional " + additionalBytes + " bytes from m_MemoryStream" + (m_MemoryStream == m_MemoryStream1 ? "1" : "2"));
                    bytesRead += additionalBytes;
                }
                m_Position += bytesRead;
                return bytesRead;
            }
            finally
            {
                ThreadMonitor.Exit(m_WaitObject);
            }
        }

        /// <summary>
        /// Read a single byte from the stream
        /// <remarks>
        /// "The default implementations of Stream.ReadByte and Stream.WriteByte create a new
        /// single-element byte array, and then call your implementations of Read and Write.
        /// When deriving from Stream, if you have an internal byte buffer, it is strongly
        /// recommended that you override these methods to access your internal buffer
        /// for substantially better performance"
        /// http://www1.cs.columbia.edu/~lok/csharp/refdocs/System.IO/types/Stream.html
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            if (Read(m_ByteBuffer, 0, 1) == 0)
                return -1;
            else
                return m_ByteBuffer[0];
        }

        ///<summary>
        ///When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        ///</summary>
        ///
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///When overridden in a derived class, sets the position within the current stream.
        ///</summary>
        ///
        ///<returns>
        ///The new position within the current stream.
        ///</returns>
        ///
        ///<param name="offset">A byte offset relative to the origin parameter. </param>
        ///<param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position. </param>
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///<exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        ///<summary>
        ///When overridden in a derived class, sets the length of the current stream.
        ///</summary>
        ///
        ///<param name="value">The desired length of the current stream in bytes. </param>
        ///<exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        ///<summary>
        ///When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        ///</summary>
        ///
        ///<param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream. </param>
        ///<param name="count">The number of bytes to be written to the current stream. </param>
        ///<param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream. </param>
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///<exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        ///<exception cref="T:System.ArgumentNullException">buffer is null. </exception>
        ///<exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        ///<summary>
        ///When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        ///</summary>
        ///
        ///<returns>
        ///true if the stream supports reading; otherwise, false.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public override bool CanRead { get { return true; } }

        ///<summary>
        ///When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        ///</summary>
        ///
        ///<returns>
        ///true if the stream supports seeking; otherwise, false.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public override bool CanSeek { get { return false; } }

        ///<summary>
        ///When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        ///</summary>
        ///
        ///<returns>
        ///true if the stream supports writing; otherwise, false.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public override bool CanWrite { get { return false; } }

        ///<summary>
        ///When overridden in a derived class, gets the length in bytes of the stream.
        ///</summary>
        ///
        ///<returns>
        ///A long value representing the length of the stream in bytes.
        ///</returns>
        ///
        ///<exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Length { get { return m_Length; } }

        ///<summary>
        ///When overridden in a derived class, gets or sets the position within the current stream.
        ///</summary>
        ///
        ///<returns>
        ///The current position within the stream.
        ///</returns>
        ///
        ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///<exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        ///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Position { get { return m_Position; } set { throw new NotSupportedException();} }
    }
}