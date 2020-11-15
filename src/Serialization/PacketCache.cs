
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
using System.Collections;
using System.Collections.Generic;
using Gibraltar.Serialization;

#endregion File Header

#pragma warning disable 1591
namespace Gibraltar.Serialization
{
    public class PacketCache : IEnumerable<ICachedPacket>
    {
        private readonly List<ICachedPacket> m_Cache;
        private readonly Dictionary<Guid, int> m_Index;

        public PacketCache()
        {
            m_Cache = new List<ICachedPacket>();
            m_Index = new Dictionary<Guid, int>();
        }

        public int AddOrGet(ICachedPacket packet)
        {
            int index;
            if (m_Index.TryGetValue(packet.ID, out index))
                return index;

            index = m_Cache.Count;
            m_Cache.Add(packet);
            m_Index.Add(packet.ID, index);
            return index;
        }

        public bool Contains(ICachedPacket packet)
        {
            return m_Index.ContainsKey(packet.ID);
        }

        public int Count { get { return m_Cache.Count; } }

        public void Clear()
        {
            m_Cache.Clear();
            m_Index.Clear();
        }

        public ICachedPacket this[int index] { get { return index >= 0 && index < m_Cache.Count ? m_Cache[index] : null; } }

        public ICachedPacket this[Guid id]
        {
            get
            {
                int index;
                if (m_Index.TryGetValue(id, out index))
                    return m_Cache[index];
                else
                    return null;
            }
        }

        #region IEnumerable<ICachedPacket> Members

        IEnumerator<ICachedPacket> IEnumerable<ICachedPacket>.GetEnumerator()
        {
            return m_Cache.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<ICachedPacket>)this).GetEnumerator();
        }

        #endregion
    }
}