using System;
using System.Net;
using Gibraltar.Server.Client;

namespace Gibraltar.Agent.Internal
{
    internal class WebAuthenticationProvider : IWebAuthenticationProvider
    {
        private readonly Net.IServerAuthenticationProvider m_WrappedProvider;

        public WebAuthenticationProvider(Net.IServerAuthenticationProvider wrappedProvider)
        {
            m_WrappedProvider = wrappedProvider;
        }

        public bool IsAuthenticated { get { return m_WrappedProvider.IsAuthenticated; } }

        public bool LogoutIsSupported { get { return m_WrappedProvider.LogoutIsSupported; } }

        public void Login(WebChannel channel, WebClient client)
        {
            m_WrappedProvider.Login(channel.EntryUri, client);
        }

        public void Logout(WebChannel channel, WebClient client)
        {
            if (m_WrappedProvider.LogoutIsSupported)
            {
                m_WrappedProvider.Logout(channel.EntryUri, client);
            }
        }

        public void PreProcessRequest(WebChannel channel, WebClient client, string resourceUrl, bool requestSupportsAuthentication)
        {
            m_WrappedProvider.PreProcessRequest(channel.EntryUri, client, resourceUrl, requestSupportsAuthentication);
        }
    }
}
