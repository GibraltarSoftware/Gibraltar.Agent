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
using Gibraltar.Server.Client.Data;
using Gibraltar.Server.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Get the list of session fragment files for a session
    /// </summary>
    internal class SessionFilesGetRequest : WebChannelRequestBase
    {
        /// <summary>
        /// Create a new session headers request
        /// </summary>
        public SessionFilesGetRequest(Guid sessionId)
            : base(true, true)
        {
            SessionId = sessionId;
        }

        /// <summary>
        /// create a new request for the specified client and session.
        /// </summary>
        public SessionFilesGetRequest(Guid clientId, Guid sessionId)
            :base(true, false)
        {
            ClientId = clientId;
            SessionId = sessionId;
        }

        /// <summary>
        /// The unique Id of this client when being used from an Agent
        /// </summary>
        public Guid? ClientId { get; private set; }

        /// <summary>
        /// The unique Id of the session we want to get the existing files for
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The list of session files on the server
        /// </summary>
        public SessionFilesListXml Files { get; private set; }

        /// <summary>
        /// Implemented by inheritors to perform the request on the provided web client.
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnProcessRequest(IWebChannelConnection connection)
        {
            string url;
            if (ClientId.HasValue)
            {
                url = string.Format("Hosts/{0}/{1}", ClientId, GenerateResourceUri());
            }
            else
            {
                url = string.Format("{0}", GenerateResourceUri());
            }

            byte[] sessionFilesListRawData = connection.DownloadData(url);

            //even though it's a session list we can't actually deserialize it directly - because we cant use XmlSerializer
            //since the types will not necessarily be public.
            Files = DataConverter.ByteArrayToSessionFilesListXml(sessionFilesListRawData);
        }

        private string GenerateResourceUri()
        {
            return string.Format("Sessions/{0}/Files.xml", SessionId);
        }
    }
}
