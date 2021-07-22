
using System;
using System.Net;

namespace Gibraltar.Agent.Net
{
    /// <summary>
    /// Basic Authentication credentials for authenticating with the server
    /// </summary>
    public sealed class BasicAuthenticationProvider : IServerAuthenticationProvider
    {
        /// <summary>
        /// Create a new instance of the HTTP Basic Authentication Provider with the specified username and password
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public BasicAuthenticationProvider(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// The user name to use for basic authentication
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password to use for basic authentication
        /// </summary>
        public string Password { get; set; }

        /// <inheritdoc />
        public bool IsAuthenticated
        {
            get
            {
                //we don't need to pre-authenticate to get a token so we say yes.
                return true;
            }
        }

        /// <inheritdoc />
        public bool LogoutIsSupported
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void Login(Uri entryUri, WebClient client)
        {
        }

        /// <inheritdoc />
        public void Logout(Uri entryUri, WebClient client)
        {
        }

        /// <inheritdoc />
        public void PreProcessRequest(Uri entryUri, WebClient client, string resourceUrl, bool requestSupportsAuthentication)
        {
            client.Credentials = new NetworkCredential(UserName, Password);
        }
    }
}
