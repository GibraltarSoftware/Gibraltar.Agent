
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Customer-level credentials (used to authenticate to an SDS using the SDS shared secret)
    /// </summary>
    public sealed class CustomerCredentials : IWebAuthenticationProvider
    {
        /// <summary>
        /// The prefix for the authorization header for this credential type
        /// </summary>
        public const string AuthorizationPrefix = "Gibraltar-Shared";

        /// <summary>
        /// Create a new set of customer credentials
        /// </summary>
        /// <param name="sharedSecret"></param>
        public CustomerCredentials(string sharedSecret)
        {   
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException(sharedSecret);

            SharedSecret = sharedSecret.Trim();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The shared secret assigned to the customer.
        /// </summary>
        public string SharedSecret { get; private set; }

        #endregion

        #region IWebAuthenticaitonProvider implementation

        /// <summary>
        /// Indicates if the authentication provider believes it has authenticated with the channel
        /// </summary>
        /// <remarks>If false then no logout will be attempted, and any request that requires authentication will
        /// cause a login attempt without waiting for an authentication failure.</remarks>
        public bool IsAuthenticated { get { return true; } }

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
            //nothing to do on login - we don't pre-auth.
        }

        /// <summary>
        /// Perform a logout on the supplied channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="client"></param>
        void IWebAuthenticationProvider.Logout(WebChannel channel, WebClient client)
        {
            //nothing to do on logout - we don't have state
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
            if (requestSupportsAuthentication == false)
                return;

            //figure out the effective relative URL.
            string fullUrl = resourceUrl;
            if (string.IsNullOrEmpty(client.BaseAddress) == false)
            {
                fullUrl = client.BaseAddress + resourceUrl;
            }

            Uri clientUri = new Uri(fullUrl);

            //we're doing sets not adds to make sure we overwrite any existing value.
            client.Headers[RepositoryCredentials.AuthorizationHeader] = AuthorizationPrefix + ": " + CalculateHash(clientUri.PathAndQuery);
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
            byte[] buffer = encoder.GetBytes(SharedSecret + saltText);

            using (SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
            {
                byte[] hash = cryptoTransformSHA1.ComputeHash(buffer);
                return Convert.ToBase64String(hash);
            }
        }

        #endregion    
    }
}
