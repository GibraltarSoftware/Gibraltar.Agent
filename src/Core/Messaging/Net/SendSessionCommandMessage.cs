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

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// A command to have the agent send sessions to the server immediately
    /// </summary>
    public class SendSessionCommandMessage : NetworkMessage
    {
        private Guid m_SessionId;
        private SessionCriteria m_Criteria;

        internal SendSessionCommandMessage()
        {
            TypeCode = NetworkMessageTypeCode.SendSession;
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Create a new send session command for the specified session id and criteria
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="criteria"></param>
        public SendSessionCommandMessage(Guid sessionId, SessionCriteria criteria)
            : this()
        {
            SessionId = sessionId;
            Criteria = criteria;
        }

        /// <summary>
        /// The session Id to send
        /// </summary>
        public Guid SessionId { get { return m_SessionId; } set { m_SessionId = value; } }

        /// <summary>
        /// The criteria to use to send the session
        /// </summary>
        public SessionCriteria Criteria { get { return m_Criteria; } set { m_Criteria = value; } }

        /// <summary>
        /// Write the packet to the stream
        /// </summary>
        protected override void OnWrite(Stream stream)
        {
            BinarySerializer.SerializeValue(stream, m_SessionId);
            BinarySerializer.SerializeValue(stream, (int)m_Criteria);
        }

        /// <summary>
        /// Read packet data from the stream
        /// </summary>
        protected override void OnRead(Stream stream)
        {
            BinarySerializer.DeserializeValue(stream, out m_SessionId);

            int rawCriteria;
            BinarySerializer.DeserializeValue(stream, out rawCriteria);
            m_Criteria = (SessionCriteria)rawCriteria;
        }
    }
}
