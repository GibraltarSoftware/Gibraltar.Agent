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
using Gibraltar.Server.Client.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Get the current server configuration information for the server
    /// </summary>
    /// <remarks>We rely on this being anonymously accessible.  First, for performance reasons and second because it's used as a Ping by the agent.</remarks>
    public class HubConfigurationGetRequest : WebChannelRequestBase
    {
        /// <summary>
        /// Create a new sessions version request
        /// </summary>
        public HubConfigurationGetRequest()
            : base(true, false)
        {
        }

        #region Public Properties and Methods

        /// <summary>
        /// The current server configuration from the server.
        /// </summary>
        public HubConfigurationXml Configuration { get; private set; }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Implemented by inheritors to perform the request on the provided web client.
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnProcessRequest(IWebChannelConnection connection)
        {
            byte[] requestedHubConfigurationRawData = connection.DownloadData("/Configuration.xml");

            //and now do it without using XMLSerializer since that doesn't work in the agent.
            Configuration = DataConverter.ByteArrayToHubConfigurationXml(requestedHubConfigurationRawData);
        }

        #endregion
    }
}

