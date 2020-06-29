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

using System;
using System.IO;
using Gibraltar.Data;

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// Sent by a Desktop to register itself with the remote server
    /// </summary>
    public class RegisterAnalystCommandMessage : NetworkMessage
    {
        private string m_UserName;
        private Guid m_RepositoryId;

        internal RegisterAnalystCommandMessage()
        {
            TypeCode = NetworkMessageTypeCode.RegisterAnalystCommand;
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Create a new registration for the specified client repository id and user name
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="userName"></param>
        public RegisterAnalystCommandMessage(Guid repositoryId, string userName)
            : this()
        {
            m_RepositoryId = repositoryId;
            m_UserName = userName;
        }

        /// <summary>
        /// The user running Analyst
        /// </summary>
        public string UserName { get { return m_UserName; } }

        /// <summary>
        /// The unique client repository id of the Analyst
        /// </summary>
        public Guid RepositoryId { get { return m_RepositoryId; } }

        /// <summary>
        /// Write the packet to the stream
        /// </summary>
        protected override void OnWrite(Stream stream)
        {
            BinarySerializer.SerializeValue(stream, m_RepositoryId);
            BinarySerializer.SerializeValue(stream, m_UserName);
        }

        /// <summary>
        /// Read packet data from the stream
        /// </summary>
        protected override void OnRead(Stream stream)
        {
            BinarySerializer.DeserializeValue(stream, out m_RepositoryId);
            BinarySerializer.DeserializeValue(stream, out m_UserName);
        }
    }
}
