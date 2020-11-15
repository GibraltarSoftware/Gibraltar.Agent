

using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Traditional user/password credentials
    /// </summary>
    public sealed class UserCredentials : IWebAuthenticationProvider
    {
        /// <summary>
        /// The prefix for the authorization header for this credential type
        /// </summary>
        public const string AuthorizationPrefix = "Gibraltar-User-Credentials";

        /// <summary>
        /// The name of the parameter in the form post for the user name
        /// </summary>
        public const string UserNameParameter = "userName";

        /// <summary>
        /// The name of the parameter in the form post for the password
        /// </summary>
        public const string PasswordParameter = "password";

        private readonly object m_Lock = new object();

        private string m_AccessToken; //PROTECTED BY LOCK

        /// <summary>
        /// Create a new set of customer credentials
        /// </summary>
        /// <param name="repositoryId">The owner Id to specify to the server (for example repository Id)</param>
        /// <param name="userName">The user's name</param>
        /// <param name="password">The user's password</param>
        public UserCredentials(Guid repositoryId, string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(userName);

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(password);

            if (repositoryId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(repositoryId), "The supplied repository Id is an empty Guid, which can't be right.");

            RepositoryId = repositoryId;

            UserName = userName.Trim();
            Password = password.Trim();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The username to provide to the server
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password to provide to the server
        /// </summary>
        public string Password { get; set; }

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
        bool IWebAuthenticationProvider.LogoutIsSupported { get { return true; } }

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
                try //just so we can do our pulse...
                {
                    m_AccessToken = null;

                    bool retry = false;
                    do
                    {
                        var parameters = new NameValueCollection();
                        parameters.Add(UserNameParameter, UserName);
                        parameters.Add(PasswordParameter, Password);
                        retry = false;

                        //post our credentials and get back our session key. 
                        try
                        {
                            byte[] rawTokenBytes = client.UploadValues("Login", parameters);
                            m_AccessToken = Encoding.UTF8.GetString(rawTokenBytes);
                        }
                        catch (WebException ex) //Our caller expects us to throw web exceptions on fail so we have to be careful with our try/catch
                        {
                            //is this an access denied error?
                            if (ex.Status == WebExceptionStatus.ProtocolError)
                            {
                                //get the inner web response to figure out exactly what the deal is.
                                var response = (HttpWebResponse)ex.Response;
                                if (response.StatusCode == HttpStatusCode.Unauthorized) //it's an auth error
                                {
                                    if (CachedCredentialsManager.UpdateCredentials(channel, RepositoryId, false))
                                    {
                                        retry = true;
                                    }
                                }
                            }

                            if (!retry)
                                throw;
                        }
                    } while (retry);
                }
                finally
                {
                    System.Threading.Monitor.PulseAll(m_Lock);
                }
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

            //we're doing sets not adds to make sure we overwrite any existing value.
            if (requestSupportsAuthentication)
            {
                client.Headers[(string) RepositoryCredentials.AuthorizationHeader] = AuthorizationPrefix + ": " + m_AccessToken;
                client.Headers[(string) RepositoryCredentials.ClientRepositoryHeader] = RepositoryId.ToString();
            }
            else
            {
                //remove our repository header.
                client.Headers.Remove((string) RepositoryCredentials.ClientRepositoryHeader);
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
            var encoder = new UTF8Encoding();
            byte[] buffer;

            //we have to always use a lock when handling the access token.
            lock (m_Lock)
            {
                buffer = encoder.GetBytes(m_AccessToken + saltText);
                System.Threading.Monitor.PulseAll(m_Lock);
            }

            using (var cryptoTransformSha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(cryptoTransformSha1.ComputeHash(buffer));
            }
        }

        #endregion
    }
}
