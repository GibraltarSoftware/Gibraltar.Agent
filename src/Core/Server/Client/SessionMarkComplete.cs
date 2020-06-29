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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Gibraltar.Server.Client.Data;
using Gibraltar.Server.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Informs the server that the session is complete (assuming it is a protocol 1.2 or higher server)
    /// </summary>
    internal class SessionMarkComplete : WebChannelRequestBase
    {
        /// <summary>
        /// Create a new session header upload request.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="clientId"></param>
        public SessionMarkComplete(Guid sessionId, Guid clientId)
            : base(true, false)
        {
            SessionId = sessionId;
            ClientId = clientId;
        }

        /// <summary>
        /// The unique Id of this client
        /// </summary>
        public Guid ClientId { get; private set; }

        /// <summary>
        /// The unique Id of the session that is complete
        /// </summary>
        public Guid SessionId { get; private set; }

        /// <summary>
        /// Implemented by inheritors to perform the request on the provided web client.
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnProcessRequest(IWebChannelConnection connection)
        {
            string strRequestUrl = string.Format("Hosts/{0}/Sessions/{1}/session.xml", ClientId, SessionId);

            SessionXml sessionHeaderXml = new SessionXml();
            sessionHeaderXml.id = SessionId.ToString();
            sessionHeaderXml.isComplete = true;
            sessionHeaderXml.isCompleteSpecified = true;

            //we can't encode using XmlSerializer because it will only work with public types, and we 
            //aren't public if we get ILMerged into something.
            byte[] encodedXml = DataConverter.SessionXmlToByteArray(sessionHeaderXml);

            connection.UploadData(strRequestUrl, "POST", "text/xml", encodedXml);
        }
    }
}
