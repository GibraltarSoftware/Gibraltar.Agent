

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Gibraltar.Server.Client.Data;
using Loupe.Extensibility.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// A web channel specifically designed to work with the Loupe Server.
    /// </summary>
    [DebuggerDisplay("{EndUserTestUrl} Status: {Status}")]
    public class HubConnection : IDisposable
    {
        /// <summary>
        /// The web request header to add for our hash
        /// </summary>
        public const string SHA1HashHeader = "X-Gibraltar-Hash";

        internal const string LogCategory = "Loupe.Repository.Server";
        public const string LoupeServiceServerName = "hub.gibraltarsoftware.com";
        private const string LoupeServiceEntryPath = "Customers/{0}/";
        private const string ApplicationKeyEntryPath = "Agent/{0}/";

        /// <summary>
        /// The version number for the new Gibraltar 3.0 features
        /// </summary>
        public static readonly Version Hub30ProtocolVersion = new Version(1, 2);

        /// <summary>
        /// The latest version of the protocol we understand
        /// </summary>
        public static readonly Version ClientProtocolVersion = Hub30ProtocolVersion;

        private readonly object m_Lock = new object();
        private readonly object m_ChannelLock = new object();

        //these are the root connection parameters from the configuration.
        private readonly bool m_UseGibraltarService;
        private readonly string m_ApplicationKey;
        private readonly string m_CustomerName;
        private readonly bool m_UseSsl;
        private readonly string m_Server;
        private readonly int m_Port;
        private readonly string m_ApplicationBaseDirectory;
        private readonly string m_Repository;

        private string m_TestUrl;
        private WebChannel m_CurrentChannel;  //the current hub we're connected to. //PROTECTED BY CHANNELLOCK
        private bool m_EnableLogging; //PROTECTED BY LOCK

        //status information
        private readonly object m_StatusLock = new object();
        private volatile bool m_HaveTriedToConnect; //volatile instead of lock to avoid locks in locks
        private Guid? m_ServerRepositoryId; //PROTECTED BY STATUSLOCK
        private HubStatus m_Status; //PROTECTED BY STATUSLOCK
        private string m_StatusMessage; //PROTECTED BY STATUSLOCK
        private DateTimeOffset? m_ExpirationDt; //PROTECTED BY STATUSLOCK
        private string m_PublicKey; //PROTECTED BY STATUSLOCK
        private Version m_ProtocolVersion; //PROTECTED BY STATUSLOCK
        private NetworkConnectionOptions m_AgentLiveStreamOptions; //PROTECTED BY STATUSLOCK
        private NetworkConnectionOptions m_ClientLiveStreamOptions; //PROTECTED BY STATUSLOCK

        //Security information.  if SupplyCredentials is set, then the other three items must be set.
        private bool m_UseCredentials; //PROTECTED BY LOCK
        private bool m_UseRepositoryCredentials; //PROTECTED BY LOCK
        private Guid m_ClientRepositoryId; //PROTECTED BY LOCK
        private string m_KeyContainerName; //PROTECTED BY LOCK
        private bool m_UseMachineStore; //PROTECTED BY LOCK

        /// <summary>
        /// Raised whenever the connection state changes.
        /// </summary>
        public event ChannelConnectionStateChangedEventHandler ConnectionStateChanged;

        /// <summary>
        /// Raised to complete the CanConnectAsync sequence
        /// </summary>
        public static event HubConnectionCanConnectEventHandler CanConnectCompleted;

        /// <summary>
        /// Create a new server connection using the provided configuration
        /// </summary>
        /// <param name="configuration"></param>
        public HubConnection(IServerConfiguration configuration)
            : this(configuration.ApplicationKey, configuration.UseGibraltarService, configuration.CustomerName, configuration.Server, configuration.Port, 
            configuration.UseSsl, configuration.ApplicationBaseDirectory, configuration.Repository)
        {
            
        }

        /// <summary>
        /// Create a new hub connection using explicit connection parameters.
        /// </summary>
        /// <param name="useGibraltarService">Indicates if the Gibraltar Loupe Service should be used instead of a private server</param>
        /// <param name="applicationKey">The application key to use to communicate with the Loupe Server</param>
        /// <param name="customerName">The unique customer name when using the Gibraltar Loupe Service</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <param name="repository">The specific repository on the server for a private server.  Only applies to a private server.</param>
        public HubConnection(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory, string repository)
        {
            m_UseGibraltarService = useGibraltarService;
            m_ApplicationKey = applicationKey;

            if (m_UseGibraltarService)
            {
                m_CustomerName = customerName;
            }
            else
            {
                m_Server = server;
                m_Port = port;
                m_UseSsl = useSsl;
                m_ApplicationBaseDirectory = applicationBaseDirectory;
                m_Repository = repository;
            }
        }

        /// <summary>
        /// The logger to use in this process
        /// </summary>
        public static IClientLogger Logger { get; set; }

        #region Public Properties and Methods

        /// <summary>
        /// Indicates if logging for events on the web channel is enabled or not.
        /// </summary>
        public bool EnableLogging
        {
            get { return m_EnableLogging; }
            set
            {
                lock (m_Lock)
                {
                    if (value != m_EnableLogging)
                    {
                        m_EnableLogging = value;

                        //update the existing channel, if necessary.
                        lock (m_ChannelLock)
                        {
                            if (m_CurrentChannel != null)
                            {
                                m_CurrentChannel.EnableLogging = m_EnableLogging;
                            }

                            System.Threading.Monitor.PulseAll(m_ChannelLock);
                        }
                    }

                    System.Threading.Monitor.PulseAll(m_Lock);
                }
            }
        }

        /// <summary>
        /// Identify our relationship Id and credential configuration for communicating with the server.
        /// </summary>
        public void SetCredentials(Guid clientRepositoryId, bool useApiKey, string keyContainerName, bool useMachineStore)
        {
            if (clientRepositoryId.Equals(Guid.Empty))
                throw new ArgumentNullException(nameof(clientRepositoryId));

            if (string.IsNullOrEmpty(keyContainerName))
                throw new ArgumentNullException(nameof(keyContainerName));

            lock (m_Lock)
            {
                m_UseCredentials = true;
                m_UseRepositoryCredentials = useApiKey;
                m_ClientRepositoryId = clientRepositoryId;
                m_KeyContainerName = keyContainerName;
                m_UseMachineStore = useMachineStore;

                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        /// <summary>
        /// Attempts to connect to the server and returns information about the connection status.
        /// </summary>
        /// <param name="status">The server status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <remarks>This method will keep the connection if it is made, improving efficiency if you are then going to use the connection.</remarks>
        /// <returns>True if the configuration is valid and the server is available, false otherwise.</returns>
        public bool CanConnect(out HubStatus status, out string statusMessage)
        {
            bool canConnect = false;
            status = HubStatus.Maintenance;
            statusMessage = null;
            Guid? serverRepositoryId = null;
            DateTimeOffset? expirationDt = null;
            string publicKey = null; //the public key published by the server.
            NetworkConnectionOptions agentOptions, clientOptions;
            Version protocolVersion = m_ProtocolVersion;

            lock (m_ChannelLock)
            {
                //if we have a current connection we'll need to see if we can keep using it
                if (m_CurrentChannel != null)
                {
                    bool channelIsUp = true;
                    try
                    {
                        HubConfigurationGetRequest configurationGetRequest = new HubConfigurationGetRequest();
                        m_CurrentChannel.ExecuteRequest(configurationGetRequest, 1); //we'd like it to succeed, so we'll give it one retry 

                        //now, if we got back a redirect we need to go THERE to get the status.
                        HubConfigurationXml configurationXml = configurationGetRequest.Configuration;
                        if ((configurationXml.redirectRequested) || (configurationXml.status != HubStatusXml.available))
                        {
                            channelIsUp = false;
                        }
                        else
                        {
                            //we can just keep using this connection, so lets do that.
                            canConnect = true;
                            status = HubStatus.Available;
                        }
                        publicKey = configurationXml.publicKey;
                    }
                    catch (Exception ex)
                    {
                        if (!Logger.SilentMode)
                            Logger.Write(LogMessageSeverity.Information, ex, false, LogCategory, "Unable to get server configuration, connection will be assumed unavailable.", "Due to an exception we were unable to retrieve the server configuration.  We'll assume the server is in maintenance until we can succeed.  Exception: {0}\r\n", ex.Message);
                        channelIsUp = false;
                    }

                    //drop the connection - we might do better, unless we're already at the root.
                    if ((channelIsUp == false)
                        && (IsRootHub(m_CurrentChannel.HostName, m_CurrentChannel.Port, m_CurrentChannel.UseSsl, m_CurrentChannel.ApplicationBaseDirectory) == false))
                    {
                        //we aren't, lets drop this channel.
                        CurrentChannel = null;
                    }
                }

                //if we don't have a connection (either we didn't before or we just invalidated the current connection) get a new one.
                if (m_CurrentChannel == null)
                {
                    CurrentChannel = Connect(out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentOptions, out clientOptions);

                    if (status == HubStatus.Available)
                    {
                        canConnect = true;
                    }
                }

                System.Threading.Monitor.PulseAll(m_ChannelLock);
            }

            //before we return, lets set our status to track what we just calculated.
            lock (m_StatusLock)
            {
                m_Status = status;
                m_StatusMessage = statusMessage;
                m_ServerRepositoryId = serverRepositoryId;
                m_ExpirationDt = expirationDt;
                m_PublicKey = publicKey;

                if (status == HubStatus.Available)
                {
#if DEBUG
                    if ((protocolVersion == null) && (m_ProtocolVersion != null) && (Debugger.IsAttached))
                        Debugger.Break(); // Stop in debugger, ignore in production.
#endif

                    ProtocolVersion = protocolVersion;
                }
            }

            return canConnect;
        }

        /// <summary>
        /// Attempts to connected to the specified server and returns information about the connection status.  The connection is then dropped.
        /// </summary>
        /// <param name="configuration">The configuration to test</param>
        /// <param name="status">The hub status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <returns>True if the configuration is valid and the server is available, false otherwise.</returns>
        public static bool CanConnect(IServerConfiguration configuration, out HubStatus status, out string statusMessage)
        {
            //forward to the override with everything broken out
            return CanConnect(configuration.ApplicationKey, configuration.UseGibraltarService, configuration.CustomerName, configuration.Server, configuration.Port, configuration.UseSsl,
                configuration.ApplicationBaseDirectory, configuration.Repository, out status, out statusMessage);
        }

        /// <summary>
        /// Attempts to connected to the specified server and returns information about the connection status.  The connection is then dropped.
        /// </summary>
        /// <param name="applicationKey">The application key to use to communicate with the Loupe Server</param>
        /// <param name="useGibraltarService">Indicates if the Loupe Service should be used instead of a private server</param>
        /// <param name="customerName">The unique customer name when using the Gibraltar Loupe Service</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <param name="repository">The specific repository on the server for a private server.  Only applies to a private server.</param>
        /// <param name="status">The server status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <returns>True if the configuration is valid and the server is available, false otherwise.</returns>
        public static bool CanConnect(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory, string repository,
            out HubStatus status, out string statusMessage)
        {
            bool canConnect = false;
            Guid? serverRepositoryId;
            DateTimeOffset? expirationDt;
            Version protocolVersion;
            string publicKey;
            NetworkConnectionOptions agentOptions, clientOptions;
            using (WebChannel channel = Connect(applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory, repository,
                out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentOptions, out clientOptions))
            {
                if ((channel != null) && (status == HubStatus.Available))
                {
                    //wait, one last check - what about protocol?
                    if (protocolVersion < Hub30ProtocolVersion)
                        statusMessage = "The server is implementing an older, incompatible version of the server protocol.";
                    else
                        canConnect = true;
                }
            }

            return canConnect;
        }

        /// <summary>
        /// Attempts to connected to the specified server and determine information about the connection status.
        /// </summary>
        /// <param name="applicationKey">The application key to use to communicate with the Loupe Server</param>
        /// <param name="useGibraltarService">Indicates if the Gibraltar Loupe Service should be used instead of a private server</param>
        /// <param name="customerName">The unique customer name when using the Gibraltar Loupe Service</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <param name="repository">The specific repository on the server for a private server.  Only applies to a private server.</param>
        /// <remarks>Results are supplied by raising the CanConnectCompleted event. This method returns immediately (but possibly after the event is raised)</remarks>
        public static void CanConnectAsync(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory, string repository)
        {
            object[] args = {applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory, repository };

            ThreadPool.QueueUserWorkItem(AsyncCanConnect, args);
        }

        /// <summary>
        /// Synchronously execute the provided request.
        /// </summary>
        /// <param name="newRequest"></param>
        /// <param name="maxRetries">The maximum number of times to retry the connection.  Use -1 to retry indefinitely.</param>
        public void ExecuteRequest(IWebRequest newRequest, int maxRetries)
        {
            //make sure we have a channel
            WebChannel channel = CurrentChannel; //this throws exceptions when it can't connect and is threadsafe.

            //if we have a channel and NOW get an exception, here is where we would recheck the status of our connection.
            try
            {
                channel.ExecuteRequest(newRequest, maxRetries);
            }
            catch (WebChannelAuthorizationException ex)
            {
                //request better credentials..
                Logger.Write(LogMessageSeverity.Warning, ex, true, LogCategory,
                    "Requesting updated credentials for the server connection due to " + ex.GetType(),
                    "We're going to assume the user can provide current credentials.\r\nDetails: {0}", ex.Message);
                if (CachedCredentialsManager.UpdateCredentials(channel, m_ClientRepositoryId, false))
                {
                    //they entered new creds.. lets give them a go.
                    ExecuteRequest(newRequest, maxRetries);
                }
                else
                {
                    //they canceled, lets call it.
                    throw;
                }
            }
            catch (WebChannelConnectFailureException)
            {
                //clear our current channel and try again if we're on a child server.
                if (IsRootHub(channel.HostName, channel.Port, channel.UseSsl, channel.ApplicationBaseDirectory) == false)
                {
                    channel = ResetChannel(); //safely clears the current channel and gets a fresh one if possible
                    channel.ExecuteRequest(newRequest, maxRetries);
                }
            }
        }

        /// <summary>
        /// Create a new subscription to this server for the supplied repository information and shared secret.
        /// </summary>
        /// <param name="repositoryXml"></param>
        /// <remarks></remarks>
        /// <returns>The client repository information retrieved from the server.</returns>
        public ClientRepositoryXml CreateSubscription(ClientRepositoryXml repositoryXml)
        {
            var request = new ClientRepositoryUploadRequest(repositoryXml);

            //we have to use distinct credentials for this so we have to swap the credentials on the connection.
            lock (m_ChannelLock)
            {
                WebChannel channel = CurrentChannel;

                bool retry = false;
                do
                {
                    try
                    {
                        channel.ExecuteRequest(request, 1);
                    }
                    catch (WebChannelAuthorizationException ex)
                    {
                        //request better credentials..
                        Logger.Write(LogMessageSeverity.Warning, ex, true, LogCategory,
                                  "Requesting updated credentials for the server connection due to " + ex.GetType(),
                                  "We're going to assume the user can provide current credentials.\r\nDetails: {0}", ex.Message);
                        retry = CachedCredentialsManager.UpdateCredentials(channel, m_ClientRepositoryId, false);

                        if (retry == false)
                            throw;
                    }
                } while (retry);

                System.Threading.Monitor.PulseAll(m_ChannelLock);
            }

            return request.ResponseRepository;
        }


        /// <summary>
        /// Create a new subscription to this server for the supplied repository information and API Key.
        /// </summary>
        /// <param name="repositoryXml"></param>
        /// <param name="sharedSecret"></param>
        /// <remarks></remarks>
        /// <returns>The client repository information retrieved from the server.</returns>
        public ClientRepositoryXml CreateSubscription(ClientRepositoryXml repositoryXml, string sharedSecret)
        {
            var request = new ClientRepositoryUploadRequest(repositoryXml);

            //we have to use distinct credentials for this so we have to swap the credentials on the connection.
            lock (m_ChannelLock)
            {
                WebChannel channel = CurrentChannel;
                IWebAuthenticationProvider previousProvider = channel.AuthenticationProvider;
                channel.AuthenticationProvider = new CustomerCredentials(sharedSecret);

                try
                {
                    channel.ExecuteRequest(request, 1);
                }
                finally
                {
                    //reset the authentication provider
                    channel.AuthenticationProvider = previousProvider;
                }

                System.Threading.Monitor.PulseAll(m_ChannelLock);
            }

            return request.ResponseRepository;
        }

        /// <summary>
        /// Authenticate now (instead of waiting for a request to fail)
        /// </summary>
        public void Authenticate()
        {
            //get the current connection and authenticate it
            CurrentChannel.Authenticate();
        }

        /// <summary>
        /// Request the user provide updated credentials for this connection.
        /// </summary>
        /// <param name="repositoryId">The unique Id of the local repository this connection is associated with</param>
        public void UpdateCredentials(Guid repositoryId)
        {
            //code duplication - we're relying on our internal knowledge of how the DNS name gets assembled.
            var hostName = m_UseGibraltarService ? LoupeServiceServerName : m_Server;
            var entryUri = CachedCredentialsManager.GetEntryUri(hostName);
            CachedCredentialsManager.UpdateCredentials(entryUri, repositoryId, false);
        }

        /// <summary>
        /// Indicates if the connection is currently authenticated.
        /// </summary>
        /// <value>False if no connection, connection doesn't support authentication, or connection is not authenticated.</value>
        public bool IsAuthenticated
        {
            get
            {
                bool isAuthenticated = false;

                lock (m_ChannelLock)
                {
                    if ((m_CurrentChannel != null) && (m_CurrentChannel.AuthenticationProvider != null))
                    {
                        isAuthenticated = m_CurrentChannel.AuthenticationProvider.IsAuthenticated;
                    }

                    System.Threading.Monitor.PulseAll(m_ChannelLock);
                }

                return isAuthenticated;
            }
        }

        /// <summary>
        /// Indicates if the connection is currently connected without attempting a new connection
        /// </summary>
        /// <value>False if no connection.  Connection may fail at any time.</value>
        public bool IsConnected
        {
            get
            {
                bool isConnected = false;

                lock (m_ChannelLock)
                {
                    if (m_CurrentChannel != null)
                    {
                        switch (m_CurrentChannel.ConnectionState)
                        {
                            case ChannelConnectionState.Connected:
                            case ChannelConnectionState.TransferringData:
                                isConnected = true;
                                break;
                        }
                    }

                    System.Threading.Monitor.PulseAll(m_ChannelLock);
                }

                return isConnected;
            }
        }

        /// <summary>
        /// The server status as of the last operation
        /// </summary>
        public HubStatus Status
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_Status;
                }
            }
        }

        /// <summary>
        /// The server status message as of the last operation.
        /// </summary>
        public string StatusMessage
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_StatusMessage;
                }
            }
        }

        /// <summary>
        /// The unique id of the server repository this connection is connected to.
        /// </summary>
        public Guid? ServerRepositoryId
        {
            get
            {
                EnsureConnectAttempted();

                lock(m_StatusLock)
                {
                    return m_ServerRepositoryId;
                }
            }
        }

        /// <summary>
        /// The server subscription expiration date &amp; time, if it isn't a perpetual license.
        /// </summary>
        public DateTimeOffset? ExpirationDt
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_ExpirationDt;
                }
            }
        }

        /// <summary>
        /// The public key used for encrypting data sent to the server
        /// </summary>
        public string PublicKey
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_PublicKey;
                }
            }
        }

        /// <summary>
        /// The server protocol version implemented by the hub.  Will never be null.
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
#if DEBUG
                    if ((m_ProtocolVersion == null) && (Debugger.IsAttached))
                        Debugger.Break(); // Stop in debugger, ignore in production.
#endif
                    return m_ProtocolVersion ?? new Version(); //it's just better for everyone to never get a null.
                }
            }
            set
            {
                lock (m_StatusLock)
                {
#if DEBUG
                    if ((value == new Version(0, 0)) && (Debugger.IsAttached))
                        Debugger.Break();
#endif
                    m_ProtocolVersion = value;
                }
            }
        }

        /// <summary>
        /// Optional.  The connection information to the live proxy for agents to use, if configured
        /// </summary>
        /// <remarks>Null if no proxy is available.</remarks>
        public NetworkConnectionOptions AgentLiveStreamOptions
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_AgentLiveStreamOptions;
                }
            }
        }

        /// <summary>
        /// Optional.  The connection information to the live proxy for clients (like Analyst) to use, if configured
        /// </summary>
        /// <remarks>Null if no proxy is available.</remarks>
        public NetworkConnectionOptions ClientLiveStreamOptions
        {
            get
            {
                EnsureConnectAttempted();

                lock (m_StatusLock)
                {
                    return m_ClientLiveStreamOptions;
                }
            }
        }

        /// <summary>
        /// Reset the current connection and re-establish it, getting the latest server configuration.
        /// </summary>
        public void Reconnect()
        {
            ResetChannel();
        }


        /// <summary>
        /// Check the provided configuration information to see if it is valid for a connection, throwing relevant exceptions if not.
        /// </summary>
        /// <param name="useGibraltarService">Indicates if the Loupe Service should be used instead of a private server.</param>
        /// <param name="applicationKey">A key used to identify the server-side repository and application environment service for this session.</param>
        /// <param name="customerName">The unique customer name when using the Loupe Service.</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid with the specific problem indicated in the message</exception>
        public static void ValidateConfiguration(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory)
        {
            //check a special case:  There is NO configuration information to speak of.
            if ((useGibraltarService == false) && string.IsNullOrEmpty(applicationKey) && string.IsNullOrEmpty(customerName) && string.IsNullOrEmpty(server))
            {
                //no way you even tried to configure the SDS.  lets use a different message.
                throw new InvalidOperationException("No server connection configuration could be found");
            }

            if (useGibraltarService)
            {
                if (string.IsNullOrEmpty(applicationKey) && string.IsNullOrEmpty(customerName))
                    throw new InvalidOperationException("An application key or service name is required to use the Loupe Service,");
            }
            else
            {
                if (string.IsNullOrEmpty(server))
                    throw new InvalidOperationException("When using a self-hosted Loupe server a full server name is required");

                if (port < 0)
                    throw new InvalidOperationException("When overriding the connection port, a positive number must be specified.  Use zero to accept the default port.");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        /// <param name="releaseManaged"></param>
        protected virtual void Dispose(bool releaseManaged)
        {
            if (releaseManaged)
                CurrentChannel = null;
        }

        /// <summary>
        /// Raises the ConnectionStateChanged event
        /// </summary>
        /// <param name="state">The new connection state</param>
        /// <remarks>Note to inheritors:  be sure to call the base implementation to ensure the event is raised.</remarks>
        protected virtual void OnConnectionStateChanged(ChannelConnectionState state)
        {
            ChannelConnectionStateChangedEventHandler tempEvent = ConnectionStateChanged;

            if (tempEvent != null)
            {
                ChannelConnectionStateChangedEventArgs e = new ChannelConnectionStateChangedEventArgs(state);
                tempEvent.Invoke(this, e);
            }
        }

        #endregion

        #region

        /// <summary>
        /// Make sure we've at least tried to connect to the server.
        /// </summary>
        private void EnsureConnectAttempted()
        {
            if (m_HaveTriedToConnect == false)
            {
                WebChannel channel = CurrentChannel; //this will try to connect.
            }
        }

        /// <summary>
        /// Get the current web channel to use. If a valid operational channel can't be found an exception will be thrown.
        /// </summary>
        private WebChannel CurrentChannel
        {
            get
            {
                //we do everything within a lock to ensure our behavior is correct even if many threads are running around.
                lock (m_ChannelLock)
                {
                    //CAREFUL:  Between here and the end of this if do not access a property that
                    //checks m_HaveTriedToConnect because if so we get into a big bad loop.
                    if (m_CurrentChannel == null)
                    {
                        try
                        {
                            //try to connect.
                            HubStatus status;
                            string statusMessage;
                            Guid? serverRepositoryId;
                            DateTimeOffset? expirationDt;
                            Version protocolVersion;
                            string publicKey;
                            NetworkConnectionOptions agentLiveStream, clientLiveStream;
                            WebChannel newChannel = Connect(out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentLiveStream, out clientLiveStream);

                            //before we return, lets set our status to track what we just calculated.
                            lock (m_StatusLock)
                            {
                                m_Status = status;
                                m_StatusMessage = statusMessage;
                                m_ExpirationDt = expirationDt;
                                m_PublicKey = publicKey;
                                m_AgentLiveStreamOptions = agentLiveStream;
                                m_ClientLiveStreamOptions = clientLiveStream;
                                m_ServerRepositoryId = serverRepositoryId;

                                //only set the protocol version if we got something worthwhile.
                                if (m_Status == HubStatus.Available)
                                {
#if DEBUG
                                    if ((protocolVersion == null) && (m_ProtocolVersion != null) && (Debugger.IsAttached))
                                        Debugger.Break();
#endif
                                    ProtocolVersion = protocolVersion;
                                }
                            }

                            //if we couldn't connect we'll have a null channel (connect returns null)
                            if (newChannel == null)
                            {
                                throw new WebChannelConnectFailureException(statusMessage);
                            }

                            //otherwise we need to bind up our events and release the existing - use our setter for that
                            CurrentChannel = newChannel;
                        }
                        finally
                        {
                            //whether we succeeded or failed, we tried.
                            m_HaveTriedToConnect = true;
                        }
                    }

                    System.Threading.Monitor.PulseAll(m_ChannelLock);
                    return m_CurrentChannel;
                }
            }
            set
            {
                lock (m_ChannelLock)
                {
                    //are they the SAME? if so nothing to do
                    if (ReferenceEquals(value, m_CurrentChannel))
                        return;

                    //otherwise, release any existing connection...
                    if (m_CurrentChannel != null)
                    {
                        m_CurrentChannel.Dispose();
                        m_CurrentChannel.ConnectionStateChanged -= CurrentChannel_ConnectionStateChanged;
                        m_CurrentChannel = null;

                        m_HaveTriedToConnect = false;
                    }

                    //and establish the new connection.
                    if (value != null)
                    {
                        m_CurrentChannel = value;
                        m_CurrentChannel.ConnectionStateChanged += CurrentChannel_ConnectionStateChanged;
                    }

                    System.Threading.Monitor.PulseAll(m_ChannelLock);
                }
            }
        }

        /// <summary>
        /// Get a test URL to access through a web browser.
        /// </summary>
        public string EndUserTestUrl
        {
            get
            {
                if (string.IsNullOrEmpty(m_TestUrl))
                {
                    string fullHubUrl;
                    WebChannel channel = null;
                    try
                    {
                        //first try to resolve it through a real connection to determine the effective server based on redirection
                        HubStatus status;
                        string statusMessage;
                        Guid? serverRepositoryId;
                        DateTimeOffset? expirationDt;
                        Version protocolVersion;
                        string publicKey;
                        NetworkConnectionOptions agentLiveStream;
                        NetworkConnectionOptions clientLiveStream;
                        channel = Connect(m_ApplicationKey, m_UseGibraltarService, m_CustomerName, m_Server, m_Port, m_UseSsl, m_ApplicationBaseDirectory, m_Repository,
                                          out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentLiveStream, out clientLiveStream);

                        //if we weren't able to connect fully we will have gotten a null channel; create just a configured channel with the parameters.
                        if (channel == null)
                        {
                            channel = ConnectToHub(m_ApplicationKey, m_UseGibraltarService, m_CustomerName, m_Server, m_Port, m_UseSsl, m_ApplicationBaseDirectory, m_Repository);
                        }

                        fullHubUrl = channel.EntryUri.AbsoluteUri;
                    }
                    finally
                    {
                        if (channel != null)
                            channel.Dispose();
                    }

                    //if this is a server URL we need to pull off the HUB suffix to make it a valid Html Url.
                    if (string.IsNullOrEmpty(fullHubUrl) == false)
                    {
                        if (fullHubUrl.EndsWith("HUB", StringComparison.OrdinalIgnoreCase))
                        {
                            fullHubUrl = fullHubUrl.Remove(fullHubUrl.Length - 4); //-3 for HUB, -1 to offset length to start position                        
                        }
                        else if (fullHubUrl.EndsWith("HUB/", StringComparison.OrdinalIgnoreCase))
                        {
                            fullHubUrl = fullHubUrl.Remove(fullHubUrl.Length - 4); //-3 for HUB/, -1 to offset length to start position                        
                        }
                    }
                    m_TestUrl = fullHubUrl;
                }

                return m_TestUrl;
            }
        }

        /// <summary>
        /// Indicates if the server supports file fragments or just a single stream per session
        /// </summary>
        public bool SupportsFileFragments
        {
            get
            {
                var protocolVersion = ProtocolVersion;
                if (protocolVersion >= Hub30ProtocolVersion) //we introduced file fragments in 1.2
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Reset the stored channel and reconnect.
        /// </summary>
        /// <returns></returns>
        private WebChannel ResetChannel()
        {
            WebChannel newChannel;
            lock (m_ChannelLock)
            {
                //force the channel to drop..
                CurrentChannel = null;

                //and get a fresh one...
                newChannel = CurrentChannel;

                System.Threading.Monitor.PulseAll(m_ChannelLock);
            }
            return newChannel;
        }

        /// <summary>
        /// Performs the connection test on a background thread
        /// </summary>
        /// <param name="state"></param>
        private static void AsyncCanConnect(object state)
        {
            HubConnectionCanConnectEventArgs responseArgs = null;
            try //we're on a background thread, we have to catch exceptions or .NET will kill the process
            {
                object[] args = (object[])state;

                string applicationKey = (string) args[0];
                bool useGibraltarService = (bool)args[1];
                string customerName = (string)args[2];
                string server = (string)args[3];
                int port = (int)args[4];
                bool useSsl = (bool)args[5];
                string applicationBaseDirectory = (string)args[6];
                string repository = (string)args[7];
                HubStatus status;
                string statusMessage;

                bool canConnect = CanConnect(applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory, repository, out status, out statusMessage);

                responseArgs = new HubConnectionCanConnectEventArgs(null, false, status, statusMessage, canConnect);
            }
            catch (Exception ex)
            {
                responseArgs = new HubConnectionCanConnectEventArgs(ex, false, null, null, false);
            }
            finally
            {
                var temp = CanConnectCompleted;
                if (temp != null)
                {
                    temp(null, responseArgs);
                }
            }
        }

        /// <summary>
        /// Connect to the server (or another server if the configured server is redirecting)
        /// </summary>
        /// <param name="serverRepositoryId">The unique Id of the server repository being accessed</param>
        /// <param name="status">The hub status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <param name="expirationDt">Optional.  The date &amp; time the server account will expire, if it is expiring.  If null it is already expired or permanent.</param>
        /// <param name="protocolVersion">Optional.  The version of the server protocol implemented by the hub</param>
        /// <param name="publicKey">Optional.  The public key for encrypting data to the server</param>
        /// <param name="agentLiveStream">Optional.  The live stream connection information for agents if the server supports it</param>
        /// <param name="clientLiveStream">Optional.  The live stream connection information for clients (e.g. Loupe Desktop) if the server supports it</param>
        /// <returns>The last web channel it was able to connect to after processing redirections, if that channel is available.</returns>
        private WebChannel Connect(out HubStatus status, out string statusMessage, out Guid? serverRepositoryId, out DateTimeOffset? expirationDt, out Version protocolVersion, out string publicKey, out NetworkConnectionOptions agentLiveStream, out NetworkConnectionOptions clientLiveStream)
        {
            WebChannel newChannel = Connect(m_ApplicationKey, m_UseGibraltarService, m_CustomerName, m_Server, m_Port, m_UseSsl, m_ApplicationBaseDirectory, m_Repository,
                out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentLiveStream, out clientLiveStream);
            if (newChannel != null)
            {
                //copy our current settings into it.
                lock (m_Lock)
                {
                    newChannel.EnableLogging = m_EnableLogging;

                    if (m_UseCredentials)
                    {
                        newChannel.AuthenticationProvider = CachedCredentialsManager.GetCredentials(newChannel, m_UseRepositoryCredentials, m_ClientRepositoryId, m_KeyContainerName, m_UseMachineStore);
                    }

                    System.Threading.Monitor.PulseAll(m_Lock);
                }
            }

            return newChannel;
        }

        /// <summary>
        /// Connects to the specified server (or another server if this server is redirecting)
        /// </summary>
        /// <param name="applicationKey">The application key to use to communicate with the Loupe Server</param>
        /// <param name="useGibraltarService">Indicates if the Gibraltar Loupe Service should be used instead of a private server</param>
        /// <param name="customerName">The unique customer name when using the Gibraltar Loupe Service</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <param name="repository">The specific repository on the server for a private server.  Only applies to a private server.</param>
        /// <param name="serverRepositoryId">The unique Id of the server repository being accessed</param>
        /// <param name="status">The hub status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <param name="expirationDt">Optional.  The date &amp; time the service account will expire, if it is expiring.  If null it is already expired or permanent.</param>
        /// <param name="protocolVersion">Optional.  The version of the communication protocol implemented by the server</param>
        /// <param name="publicKey">Optional.  The public key for encrypting data to the server</param>
        /// <param name="agentLiveStream">Optional.  The live stream connection information for agents if the server supports it</param>
        /// <param name="clientLiveStream">Optional.  The live stream connection information for clients (e.g. Loupe Desktop) if the server supports it</param>
        /// <returns>The last web channel it was able to connect to after processing redirections.</returns>
        private static WebChannel Connect(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory, string repository,
            out HubStatus status, out string statusMessage, out Guid? serverRepositoryId, out DateTimeOffset? expirationDt, out Version protocolVersion, out string publicKey, out NetworkConnectionOptions agentLiveStream, out NetworkConnectionOptions clientLiveStream)
        {
            WebChannel channel = null;
            bool canConnect = true;
            status = HubStatus.Maintenance; //a reasonable default.
            statusMessage = null;
            serverRepositoryId = null;
            expirationDt = null;
            protocolVersion = new Version();
            publicKey = null;
            agentLiveStream = null;
            clientLiveStream = null;

            //first, is it a valid config?  No point in trying to connect if it's a bum config.
            try
            {
                ValidateConfiguration(applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory);
            }
            catch (Exception ex)
            {
                canConnect = false;
                statusMessage = "Invalid configuration: " + ex.Message;
            }

            //and now try to connect to the server
            try
            {
                channel = ConnectToHub(applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory, repository);
                HubConfigurationGetRequest configurationGetRequest = new HubConfigurationGetRequest();

                channel.AuthenticationProvider = CachedCredentialsManager.GetCachedCredentials(channel, false, Guid.Empty, null, false);
                try
                {
                    channel.ExecuteRequest(configurationGetRequest, 1); //we'd like it to succeed, so we'll give it one retry 
                }
                catch (WebChannelAuthorizationException ex)
                {
                    //request better credentials..
                    Logger.Write(LogMessageSeverity.Warning, ex, true, LogCategory,
                                 "Requesting updated credentials for the server connection due to " + ex.GetType(),
                                 "We're going to assume the user can provide current credentials.\r\nDetails: {0}", ex.Message);
                    if (CachedCredentialsManager.UpdateCredentials(channel, false))
                    {

                        channel.AuthenticationProvider = CachedCredentialsManager.GetCredentials(channel, false, Guid.Empty, null, false);

                        //they entered new creds.. lets give them a go.
                        channel.ExecuteRequest(configurationGetRequest, 1); //we'd like it to succeed, so we'll give it one retry 
                    }
                    else
                    {
                        //they canceled, lets call it.
                        throw;
                    }
                }

                //now, if we got back a redirect we need to go THERE to get the status.
                HubConfigurationXml configurationXml = configurationGetRequest.Configuration;
                if (configurationXml.redirectRequested)
                {
                    //recursively try again.
                    channel.Dispose();
                    channel = Connect(applicationKey, configurationXml.redirectUseGibraltarSds, configurationXml.redirectCustomerName, configurationXml.redirectHostName, configurationXml.redirectPort,
                                      configurationXml.redirectUseSsl, configurationXml.redirectApplicationBaseDirectory, configurationXml.redirectCustomerName,
                                      out status, out statusMessage, out serverRepositoryId, out expirationDt, out protocolVersion, out publicKey, out agentLiveStream, out clientLiveStream);
                }
                else
                {
                    //set the right status message
                    status = (HubStatus)configurationXml.status;

                    switch (status)
                    {
                        case HubStatus.Available:
                            break;
                        case HubStatus.Expired:
                            statusMessage = "The Server's license has expired.  " + (useGibraltarService ? "You can reactivate your license in seconds at www.GibraltarSoftware.com." : "To renew your license, run the Administration tool on the Loupe Server.");
                            break;
                        case HubStatus.Maintenance:
                            statusMessage = "The Server is currently undergoing maintenance and can't process requests.";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(status));
                    }

                    if (configurationXml.id != null)
                    {
                        serverRepositoryId = new Guid(configurationXml.id);
                    }

                    if (configurationXml.expirationDt != null)
                    {
                        expirationDt = DataConverter.FromDateTimeOffsetXml(configurationXml.expirationDt);
                    }

                    publicKey = configurationXml.publicKey;

                    if (string.IsNullOrEmpty(configurationXml.protocolVersion) == false)
                    {
                        protocolVersion = new Version(configurationXml.protocolVersion);
                    }

                    LiveStreamServerXml liveStreamConfig = configurationXml.liveStream;
                    if (liveStreamConfig != null)
                    {
                        agentLiveStream = new NetworkConnectionOptions { HostName = channel.HostName, Port = liveStreamConfig.agentPort, UseSsl = liveStreamConfig.useSsl };
                        clientLiveStream = new NetworkConnectionOptions { HostName = channel.HostName, Port = liveStreamConfig.clientPort, UseSsl = liveStreamConfig.useSsl };
                    }
                }
            }
            catch (WebChannelFileNotFoundException)
            {
                canConnect = false;
                if (useGibraltarService)
                {
                    //we'll treat file not found (e.g. customer never existed) as expired to get the right UI behavior.
                    status = HubStatus.Expired;
                    statusMessage = "The specified customer name is not valid";
                }
                else
                {
                    statusMessage = "The server does not support this service or the specified directory is not valid";
                }
            }
            catch (WebException ex)
            {
                canConnect = false;
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                statusMessage = response.StatusDescription; //by default we'll use the detailed description we got back from the web server.

                //we want to be somewhat more intelligent in our responses to decode what these might MEAN.
                if (useGibraltarService)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.BadRequest:
                            status = HubStatus.Expired;
                            statusMessage = "The specified customer name is not valid";
                            break;
                    }
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            statusMessage = "No service could be found with the provided information";
                            break;
                        case HttpStatusCode.BadRequest:
                            statusMessage = "The server does not support this service or the specified directory is not valid";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                canConnect = false;
                statusMessage = ex.Message;
            }

            //before we return make sure we clean up an errant channel if we don't need it.
            if ((canConnect == false) && (channel != null))
            {
                channel.Dispose();
                channel = null;
            }

            return channel;
        }

        /// <summary>
        /// Create a web channel to the specified server configuration.  Low level primitive that does no redirection.
        /// </summary>
        private static WebChannel ConnectToHub(string applicationKey, bool useGibraltarService, string customerName, string server, int port, bool useSsl, string applicationBaseDirectory, string repository)
        {
            WebChannel channel;

            if (useGibraltarService)
            {
                string entryPath = string.IsNullOrEmpty(applicationKey)
                    ? string.Format(LoupeServiceEntryPath, customerName) + "Hub"
                    : string.Format(ApplicationKeyEntryPath, applicationKey) + "Hub";

                channel = new WebChannel(Logger, true, LoupeServiceServerName, entryPath, ClientProtocolVersion);
            }
            else
            {
                //we need to create the right application base directory to get into Hub.
                string entryPath = EffectiveApplicationBaseDirectory(applicationKey, applicationBaseDirectory, repository) + "Hub";

                //and now we can actually create the channel!  Yay!
                channel = new WebChannel(Logger, useSsl, server, port, entryPath, ClientProtocolVersion);
            }

            return channel;
        }

        /// <summary>
        /// Combines application base directory (if not null) and repository (if not null) into one merged path.
        /// </summary>
        private static string EffectiveApplicationBaseDirectory(string applicationKey, string applicationBaseDirectory, string repository)
        {
            string effectivePath = applicationBaseDirectory ?? string.Empty;

            if (string.IsNullOrEmpty(effectivePath) == false)
            {
                //check for whether we need to Extension a slash.
                if (effectivePath.EndsWith("/") == false)
                {
                    effectivePath += "/";
                }
            }

            if (string.IsNullOrEmpty(applicationKey) == false)
            {
                //we have an app key for that effective path which gets priority over any repository.
                effectivePath += string.Format(ApplicationKeyEntryPath, applicationKey);
            }
            else if (string.IsNullOrEmpty(repository) == false)
            {
                //we want a specific repository - which was created for Loupe Service so it assumes everyone's a "customer".  Oh well.
                effectivePath += string.Format(LoupeServiceEntryPath, repository);
            }

            return effectivePath;
        }

        /// <summary>
        /// Indicates if we're on the original configured server (the "root") or have been redirected.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="useSsl"></param>
        /// <param name="applicationBaseDirectory"></param>
        /// <returns></returns>
        private bool IsRootHub(string hostName, int port, bool useSsl, string applicationBaseDirectory)
        {
            bool isSameHub = true;

            if (string.IsNullOrEmpty(hostName))
            {
                //can't be the same - invalid host
                isSameHub = false;
            }
            else
            {
                if (m_UseGibraltarService)
                {
                    if (hostName.Equals(LoupeServiceServerName, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        //it's the wrong server.
                        isSameHub = false;
                    }

                    string entryPath = string.Format(LoupeServiceEntryPath, m_CustomerName);

                    if (string.Equals(entryPath, applicationBaseDirectory) == false)
                    {
                        //it isn't the same customer
                        isSameHub = false;
                    }
                }
                else
                {
                    //simpler - we're looking for an exact match on each item.
                    if ((hostName.Equals(m_Server, StringComparison.OrdinalIgnoreCase) == false)
                        || (m_Port != port)
                        || (m_UseSsl != useSsl))
                    {
                        //it's the wrong server.
                        isSameHub = false;
                    }
                    else
                    {
                        //application base directory is more complicated - we have to take into account if we have a repository set or not.
                        var entryPath = EffectiveApplicationBaseDirectory(m_ApplicationKey, m_ApplicationBaseDirectory, m_Repository);

                        if (string.Equals(entryPath, applicationBaseDirectory) == false)
                        {
                            //it isn't the same repository
                            isSameHub = false;
                        }
                    }
                }
            }

            return isSameHub;
        }


        #endregion

        #region Event Handlers

        private void CurrentChannel_ConnectionStateChanged(object sender, ChannelConnectionStateChangedEventArgs e)
        {
            OnConnectionStateChanged(e.State);
        }

        #endregion
    }
}
