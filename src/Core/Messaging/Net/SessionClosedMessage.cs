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
using Gibraltar.Data;

#endregion File Header

#pragma warning disable 1591

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// Indicates that the identified session has been closed.
    /// </summary>
    public class SessionClosedMessage : NetworkMessage
    {
        private Guid m_SessionId;

        internal SessionClosedMessage()
        {
            TypeCode = NetworkMessageTypeCode.SessionClosed;
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Create a new session closed message for the specified session id
        /// </summary>
        /// <param name="sessionId"></param>
        public SessionClosedMessage(Guid sessionId)
            : this()
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get { return m_SessionId; } private set { m_SessionId = value; } }

        /// <summary>
        /// Write the packet to the stream
        /// </summary>
        protected override void OnWrite(Stream stream)
        {
            BinarySerializer.SerializeValue(stream, m_SessionId);
        }

        /// <summary>
        /// Read packet data from the stream
        /// </summary>
        protected override void OnRead(Stream stream)
        {
            BinarySerializer.DeserializeValue(stream, out m_SessionId);
        }
    }
}
