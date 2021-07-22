
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

#endregion File Header

namespace Gibraltar.Serialization.Internal
{
    internal class SimpleMemoryStream : MemoryStream
    {
        private readonly byte[] m_Buffer;
        private long m_Position;
        private long m_Length;

        public SimpleMemoryStream(int size)
        {
            m_Buffer = new byte[size];
        }

        public override long Position { get { return m_Position; } set { m_Position = value; } }

        public override long Length { get { return m_Length; } }

        public void SetLength(int length)
        {
            m_Length = length;
        }

        public override byte[] GetBuffer()
        {
            return m_Buffer;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            long availableBytes = m_Length - m_Position;
            if (count > availableBytes)
                count = (int)availableBytes;
            Buffer.BlockCopy(m_Buffer, (int)m_Position, array, offset, count);
            m_Position += count;
            return count;
        }

        public override int ReadByte()
        {
            if (m_Position < m_Length)
                return m_Buffer[m_Position++];
            else
                return -1;
        }
    }
}