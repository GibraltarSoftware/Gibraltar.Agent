
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
using Gibraltar.Data;

#endregion File Header

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// Indicates the live view session for the specified session Id be terminated
    /// </summary>
    public class LiveViewStopCommandMessage : NetworkMessage
    {
        private Guid m_ChannelId;
        private Guid m_SessionId;

        internal LiveViewStopCommandMessage()
        {
            TypeCode = NetworkMessageTypeCode.LiveViewStopCommand;
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Create a command to stop the specified live view channel
        /// </summary>
        public LiveViewStopCommandMessage(Guid channelId, Guid sessionId)
            : this()
        {
            ChannelId = channelId;
            SessionId = sessionId;
        }

        /// <summary>
        /// The channel Id of the viewer
        /// </summary>
        public Guid ChannelId { get { return m_ChannelId; } set { m_ChannelId = value; } }

        /// <summary>
        /// The session Id that is being viewed
        /// </summary>
        public Guid SessionId { get { return m_SessionId; } set { m_SessionId = value; } }

        /// <summary>
        /// Write the packet to the stream
        /// </summary>
        protected override void OnWrite(Stream stream)
        {
            BinarySerializer.SerializeValue(stream, m_ChannelId);
            BinarySerializer.SerializeValue(stream, m_SessionId);
        }

        /// <summary>
        /// Read packet data from the stream
        /// </summary>
        protected override void OnRead(Stream stream)
        {
            BinarySerializer.DeserializeValue(stream, out m_ChannelId);
            BinarySerializer.DeserializeValue(stream, out m_SessionId);
        }
    }
}
