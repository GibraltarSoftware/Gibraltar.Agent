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
using System.Net;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Extends the traditional web client to add a timeout property
    /// </summary>
    public class ExtendedTimeoutWebClient : WebClient
    {
        /// <summary>
        /// The number of seconds to wait for requests to timeout.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// True to force HTTP 1.0 instead of letting the framework go to the highest detected
        /// </summary>
        public bool UseHttpVersion10 { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </returns>
        /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.
        ///                 </param>
        protected override WebRequest GetWebRequest(System.Uri address)
        {
            var webRequest = base.GetWebRequest(address);

            if (webRequest == null)
                return webRequest;

            if (Timeout > 0)
                webRequest.Timeout = Timeout;

            if (UseHttpVersion10)
            {
                var httpRequest = webRequest as HttpWebRequest;
                if ((httpRequest != null) && (httpRequest.ProtocolVersion != HttpVersion.Version10))
                {
                    httpRequest.ProtocolVersion = HttpVersion.Version10;
                }
            }

            return webRequest;
        }
    }
}
