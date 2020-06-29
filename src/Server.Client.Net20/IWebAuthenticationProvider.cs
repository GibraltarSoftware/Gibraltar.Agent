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

using System.Net;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Implemented to provide custom authentication for a web channel
    /// </summary>
    public interface IWebAuthenticationProvider
    {
        /// <summary>
        /// Indicates if the authentication provider believes it has authenticated with the channel
        /// </summary>
        /// <remarks>If false then no logout will be attempted, and any request that requires authentication will
        /// cause a login attempt without waiting for an authentication failure.</remarks>
        bool IsAuthenticated { get; }

        /// <summary>
        /// indicates if the authentication provider can perform a logout
        /// </summary>
        bool LogoutIsSupported { get; }

        /// <summary>
        /// Perform a login on the supplied channel
        /// </summary>
        /// <param name="channel">The channel object</param>
        /// <param name="client">A web client object to use to perform login operations.</param>
        void Login(WebChannel channel, WebClient client);

        /// <summary>
        /// Perform a logout on the supplied channel
        /// </summary>
        /// <param name="channel">The channel object</param>
        /// <param name="client">A web client object to use to perform logout operations.</param>
        void Logout(WebChannel channel, WebClient client);

        /// <summary>
        /// Perform per-request authentication processing.
        /// </summary>
        /// <param name="channel">The channel object</param>
        /// <param name="client">The web client that is about to be used to execute the request.  It can't be used by the authentication provider to make requests.</param>
        /// <param name="resourceUrl">The resource URL (with query string) specified by the client.</param>
        /// <param name="requestSupportsAuthentication">Indicates if the request being processed supports authentication or not.</param>
        /// <remarks>If the request doesn't support authentication, it's a best practice to not provide any authentication information.</remarks>
        void PreProcessRequest(WebChannel channel, WebClient client, string resourceUrl, bool requestSupportsAuthentication);
    }    
}
