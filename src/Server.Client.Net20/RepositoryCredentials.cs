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
using System.Security.Cryptography;
using System.Text;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Authentication credentials for a repository to a shared data service.
    /// </summary>
    public sealed class RepositoryCredentials : IWebAuthenticationProvider
    {
        /// <summary>
        /// The prefix for the authorization header for this credential type
        /// </summary>
        public const string AuthorizationPrefix = "Gibraltar-Repository";

        /// <summary>
        /// The HTTP Request header identifying the client repository
        /// </summary>
        public const string ClientRepositoryHeader = "X-Gibraltar-Repository";

        internal const string AuthorizationHeader = "Authorization";

        private readonly object m_Lock = new object();

        private string m_AccessToken; //PROTECTED BY LOCK

        /// <summary>
        /// Create a new set of repository credentials
        /// </summary>
        /// <param name="repositoryId">The owner Id to specify to the server (for example repository Id)</param>
        /// <param name="keyContainerName">The name of the key container to retrieve the private key from</param>
        /// <param name="useMachineStore">True to use the machine store instead of the user store for the digital certificate</param>
        public RepositoryCredentials(Guid repositoryId, string keyContainerName, bool useMachineStore)
        {
            if (repositoryId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(repositoryId), "The supplied repository Id is an empty guid, which can't be right.");

            RepositoryId = repositoryId;
            KeyContainerName = keyContainerName;
            UseMachineStore = useMachineStore;
        }

        #region Public Properties and Methods

        /// <summary>
        /// True to use the machine store instead of the user store for the digital certificate
        /// </summary>
        public bool UseMachineStore { get; private set; }

        /// <summary>
        /// The name of the key container to retrieve the private key from
        /// </summary>
        public string KeyContainerName { get; private set; }

        /// <summary>
        /// The owner Id to specify to the server (for example repository Id)
        /// </summary>
        public Guid RepositoryId { get; private set; }

        #endregion

        #region IWebAuthenticaitonProvider implementation

        /// <summary>
        /// Indicates if the authentication provider believes it has authenticated with the channel
        /// </summary>
        /// <remarks>If false then no logout will be attempted, and any request that requires authentication will
        /// cause a login attempt without waiting for an authentication failure.</remarks>
        public bool IsAuthenticated
        {
            get
            {
                bool isAuthenticated = false;

                //we have to always use a lock when handling the access token.
                lock (m_Lock)
                {
                    isAuthenticated = (string.IsNullOrEmpty(m_AccessToken) == false);
                    System.Threading.Monitor.PulseAll(m_Lock);
                }

                return isAuthenticated;
            }
        }

        /// <summary>
        /// indicates if the authentication provider can perform a logout
        /// </summary>
        bool IWebAuthenticationProvider.LogoutIsSupported { get { return false; } }

        /// <summary>
        /// Perform a login on the supplied channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="client"></param>
        void IWebAuthenticationProvider.Login(WebChannel channel, WebClient client)
        {
            //we do a lock around all of this even though we could block for a while because we want
            //to make sure someone else checking if we're authenticated, etc. waits for the resolution.
            lock (m_Lock)
            {
                //we need to get the access token for our repository id
                string requestUrl = string.Format("Repositories/{0}/AccessToken.bin", RepositoryId);
                string encryptedToken = client.DownloadString(requestUrl);

                //and here we WOULD decrypt the access token if it was encrypted
                m_AccessToken = encryptedToken;
                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        /// <summary>
        /// Perform a logout on the supplied channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="client"></param>
        void IWebAuthenticationProvider.Logout(WebChannel channel, WebClient client)
        {
            //we have to always use a lock when handling the access token.
            lock (m_Lock)
            {
                m_AccessToken = null;
                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        /// <summary>
        /// Perform per-request authentication processing.
        /// </summary>
        /// <param name="channel">The channel object</param>
        /// <param name="client">The web client that is about to be used to execute the request.  It can't be used by the authentication provider to make requests.</param>
        /// <param name="resourceUrl">The resource URL (with query string) specified by the client.</param>
        /// <param name="requestSupportsAuthentication">Indicates if the request being processed supports authentication or not.</param>
        void IWebAuthenticationProvider.PreProcessRequest(WebChannel channel, WebClient client, string resourceUrl, bool requestSupportsAuthentication)
        {
            //figure out the effective relative URL.
            string fullUrl = resourceUrl;
            if (string.IsNullOrEmpty(client.BaseAddress) == false)
            {
                fullUrl = client.BaseAddress + resourceUrl;
            }

            Uri clientUri = new Uri(fullUrl);

            //we're doing sets not adds to make sure we overwrite any existing value.
            if (requestSupportsAuthentication)
            {
                client.Headers[AuthorizationHeader] = AuthorizationPrefix + ": " + CalculateHash(clientUri.PathAndQuery);
                client.Headers[ClientRepositoryHeader] = RepositoryId.ToString();
            }
            else
            {
                //remove our repository header.
                client.Headers.Remove(ClientRepositoryHeader);
            }
        }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// Calculates the effective hash given the provided salt text.
        /// </summary>
        /// <param name="saltText"></param>
        /// <returns></returns>
        private string CalculateHash(string saltText)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            byte[] buffer;

            //we have to always use a lock when handling the access token.
            lock (m_Lock)
            {
                buffer = encoder.GetBytes(m_AccessToken + saltText);
                System.Threading.Monitor.PulseAll(m_Lock);
            }

            using (SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(cryptoTransformSHA1.ComputeHash(buffer));
            }
        }

        #endregion
    }
}
