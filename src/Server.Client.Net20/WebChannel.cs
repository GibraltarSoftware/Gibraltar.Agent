

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using Loupe.Extensibility.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Provides in-order communication with a remote web server.
    /// </summary>
    [DebuggerDisplay("{HostName} State: {ConnectionState}")]
    public class WebChannel : IWebChannelConnection, IDisposable
    {
        private int m_RetryDelaySeconds = MinimumRetryDelaySeconds;
        private const int MinimumRetryDelaySeconds = 1;
        private const int MaximumRetryDelaySeconds = 120;
        private const int DefaultTimeoutSeconds = 120;
        private const string HeaderContentType = "Content-Type";
        private const string HeaderRequestMethod = "X-Request-Method";
        private const string HeaderRequestTimestamp = "X-Request-Timestamp";
        private const string HeaderRequestAppProtocolVersion = "X-Request-App-Protocol";

        /// <summary>
        /// The log category for the server client
        /// </summary>
        public const string LogCategory = "Loupe.Server.Client";

        private readonly object m_Lock = new object();
        private readonly object m_RequestLock = new object();
        private readonly object m_ResponseLock = new object();
        private static readonly Dictionary<string, bool> s_ServerUseCompatibilitySetting = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> s_ServerUseHttpVersion10Setting = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private volatile bool m_Cancel; 
        private IWebAuthenticationProvider m_AuthenticationProvider; //PROTECTED BY LOCK
        private ExtendedTimeoutWebClient m_Connection; //PROTECTED BY LOCK
        private Uri m_BaseAddress; //PROTECTED BY LOCK
        private volatile ChannelConnectionState m_ConnectionState;
        private DownloadDataCompletedEventArgs m_DownloadDataRespose; //PROTECTED BY REQUESTLOCK
        private AsyncCompletedEventArgs m_DownloadFileRespose; //PROTECTED BY REQUESTLOCK
        private DownloadStringCompletedEventArgs m_DownloadStringRespose; //PROTECTED BY REQUESTLOCK
        private UploadDataCompletedEventArgs m_UploadDataRespose; //PROTECTED BY REQUESTLOCK
        private UploadFileCompletedEventArgs m_UploadFileRespose; //PROTECTED BY REQUESTLOCK

        private readonly IClientLogger m_Logger;
        private readonly bool m_UseSsl;
        private readonly string m_HostName;
        private readonly int m_Port;
        private readonly string m_ApplicationBaseDirectory;
        private bool m_UseCompatibilityMethods;
        private bool m_UseHttpVersion10 = false; //used to force 1.0 instead of 1.1
        private bool m_FirstRequest = true; //used so we can defer checking some optimizations from construction until our first request.
        private bool m_RequestSupportsAuthentication; //crappy way of passing data from ExecuteRequest to PreProcessRequest
        private volatile bool m_Disposed;

        /// <summary>
        /// Raised whenever the connection state changes.
        /// </summary>
        public event ChannelConnectionStateChangedEventHandler ConnectionStateChanged;

        /// <summary>
        /// Create a new web channel to the specified host.
        /// </summary>
        public WebChannel(IClientLogger logger, string hostName)
            : this(logger, false, hostName, null, null)
        {
            
        }

        /// <summary>
        /// Create a new web channel to the specified host.
        /// </summary>
        public WebChannel(IClientLogger logger, bool useSsl, string hostName, string applicationBaseDirectory, Version appProtocolVersion)
            : this(logger, useSsl, hostName, useSsl ? 443 : 80, applicationBaseDirectory, appProtocolVersion)
        {
            
        }

        /// <summary>
        /// Create a new web channel to the specified host.
        /// </summary>
        public WebChannel(IClientLogger logger, bool useSsl, string hostName, int port, string applicationBaseDirectory, Version appProtocolVersion)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            
            if (hostName != null)
                m_HostName = hostName.Trim();

            if (string.IsNullOrEmpty(hostName))
                throw new ArgumentNullException(nameof(hostName), "A server name must be provided for a connection");

            m_Logger = logger;
            m_UseSsl = useSsl;            
            m_Port = port;
            m_ApplicationBaseDirectory = applicationBaseDirectory;
            AppProtocolVersion = appProtocolVersion;

            //format up base directory in case we get something we can't use.  It has to have leading & trailing slashes.
            if (string.IsNullOrEmpty(m_ApplicationBaseDirectory) == false)
            {
                m_ApplicationBaseDirectory = m_ApplicationBaseDirectory.Trim();
                if (m_ApplicationBaseDirectory.StartsWith("/") == false)
                {
                    m_ApplicationBaseDirectory = "/" + m_ApplicationBaseDirectory;
                }

                if (m_ApplicationBaseDirectory.EndsWith("/") == false)
                {
                    m_ApplicationBaseDirectory = m_ApplicationBaseDirectory + "/";
                }
            }

            //enable TLS 1.2
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Optional.  The version number to specify in the protocol header.
        /// </summary>
        public Version AppProtocolVersion { get; set; }

        /// <summary>
        /// Indicates if logging for events on the web channel is enabled or not.
        /// </summary>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// The authentication provider to use for any requests that require authentication.
        /// </summary>
        public IWebAuthenticationProvider AuthenticationProvider 
        { 
            get
            {
                return m_AuthenticationProvider;
            }
            set
            {
                lock (m_Lock)
                {
                    m_AuthenticationProvider = value;

                    System.Threading.Monitor.PulseAll(m_Lock);
                }
            }
        }

        /// <summary>
        /// The DNS name of the server being connected to.
        /// </summary>
        public string HostName { get { return m_HostName; } }

        /// <summary>
        /// The port number being used
        /// </summary>
        public int Port { get { return m_Port; } }

        /// <summary>
        /// Indicates if the channel is encrypted using SSL
        /// </summary>
        public bool UseSsl { get { return m_UseSsl; } }

        /// <summary>
        /// The path from the root of the web server to the start of the application (e.g. the virtual directory path)
        /// </summary>
        public string ApplicationBaseDirectory { get { return m_ApplicationBaseDirectory; } }

        /// <summary>
        /// The complete Uri to the start of all requests that can be executed on this channel.
        /// </summary>
        public Uri EntryUri
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_BaseAddress != null)
                        return m_BaseAddress;
                }

                return new Uri(CalculateBaseAddress());
            }
        }

        /// <summary>
        /// The current connection state of the channel.
        /// </summary>
        public ChannelConnectionState ConnectionState { get { return m_ConnectionState; } }

        /// <summary>
        /// Synchronously execute the provided request.
        /// </summary>
        /// <param name="newRequest"></param>
        /// <param name="maxRetries">The maximum number of times to retry the connection.  Use -1 to retry indefinitely.</param>
        public void ExecuteRequest(IWebRequest newRequest, int maxRetries)
        {
            if (newRequest == null)
                throw new ArgumentNullException(nameof(newRequest));

            if ((newRequest.RequiresAuthentication) && (m_AuthenticationProvider == null))
                throw new ArgumentException("The request requires authentication and no authentication provider is available.", nameof(newRequest));

            EnsureConnectionInitialized();

            m_Cancel = false;
            bool requestComplete = false;
            bool lastCallWasAuthentication = false; //indicates if we just tried an auth, so another 401 means no go.
            int errorCount = 0;

            try
            {
                while ((m_Cancel == false) 
                    && (requestComplete == false) 
                    && ((maxRetries < 0) || (errorCount <= maxRetries)))
                {
                    try
                    {
                        if (m_ConnectionState == ChannelConnectionState.Disconnected)
                            SetConnectionState(ChannelConnectionState.Connecting);
                        else if (m_ConnectionState == ChannelConnectionState.Connected)
                            SetConnectionState(ChannelConnectionState.TransferringData);

                        if((newRequest.RequiresAuthentication) && (m_AuthenticationProvider.IsAuthenticated == false))
                        {
                            //no point in waiting for the failure, go ahead and authenticate now.
                            Authenticate();
                        }

                        //Now, because we know we're not MT-safe we can do this "pass around"
                        m_RequestSupportsAuthentication = newRequest.SupportsAuthentication;

                        newRequest.ProcessRequest(this);
                        SetConnectionState(ChannelConnectionState.Connected);
                        requestComplete = true;
                    }
                    catch (WebException ex)
                    {
                        //but WHY did we fail?
                        switch (ex.Status)
                        {
                                //find the terminal failures...
                            case WebExceptionStatus.MessageLengthLimitExceeded:
                                throw;
                            case WebExceptionStatus.ProtocolError:
                                //get the inner web response to figure out exactly what the deal is.
                                HttpWebResponse response = (HttpWebResponse)ex.Response;
                                if (response.StatusCode == HttpStatusCode.NotFound)
                                {
                                    //throw our dedicated file not found exception.
                                    throw new WebChannelFileNotFoundException("File not found", ex, response.ResponseUri);
                                }

                                if (response.StatusCode == HttpStatusCode.Unauthorized) //it's an auth error
                                {
                                    if ((m_AuthenticationProvider != null) && newRequest.SupportsAuthentication //we can do an auth
                                        && (lastCallWasAuthentication == false)) //and we didn't just try to do an auth..
                                    {
                                        if (EnableLogging) m_Logger.Write(LogMessageSeverity.Information, ex, true, LogCategory, "Attempting to authenticate to server", "Because we got an HTTP 401 error from the server we're going to attempt to authenticate with our server credentials and try again.  Status Description:\r\n{0}", response.StatusDescription);
                                        lastCallWasAuthentication = true;
                                        Authenticate();
                                    }
                                    else
                                    {
                                        //create a new exception that tells our caller it's an authorization problem.
                                        throw new WebChannelAuthorizationException("Username or password not valid", ex, response.ResponseUri);
                                    }
                                }
                                else if ((response.StatusCode == HttpStatusCode.MethodNotAllowed) && (m_UseCompatibilityMethods == false))
                                {
                                    //most likely we did a delete or put and the caller doesn't support that, enable compatibility methods.
                                    m_UseCompatibilityMethods = true;
                                    SetUseCompatiblilityMethodsOverride(m_HostName, m_UseCompatibilityMethods); //so we don't have to repeatedly run into this for this server
                                    if (EnableLogging) m_Logger.Write(LogMessageSeverity.Information, ex, true, LogCategory, "Switching to http method compatibility mode", "Because we got an HTTP 405 error from the server we're going to turn on Http method compatibility translation and try again.  Status Description:\r\n{0}", response.StatusDescription);
                                }
                                else if ((response.StatusCode == HttpStatusCode.ExpectationFailed) && (m_UseHttpVersion10 == false))
                                {
                                    //most likely we are talking to an oddball proxy that doesn't support keepalive (like on a train or plane, seriously..)
                                    m_UseHttpVersion10 = true;
                                    SetUseHttpVersion10Override(m_HostName, m_UseHttpVersion10); //so we don't have to repeatedly run into this for this server
                                    if (EnableLogging) m_Logger.Write(LogMessageSeverity.Information, ex, true, LogCategory, "Switching to http 1.0 compatibility mode", "Because we got an HTTP 417 error from the server we're going to turn on Http 1.0 compatibility translation and try again.  Status Description:\r\n{0}", response.StatusDescription);
                                }
                                else
                                {
                                    //we don't have a specific error for this, just throw the underlying exception.
                                    throw;
                                }
                                break;
                            default:
                                //assume a retryable connection error
                                if (EnableLogging) m_Logger.Write(LogMessageSeverity.Warning, ex, true, LogCategory, "Connection error while making web channel request", "We received a communication exception while executing the current request on our channel.  Since it isn't an authentication exception we're going to retry the request.\r\nRequest:{0}\r\nError Count:{1}\r\nException:\r\n{0}", newRequest, errorCount, ex.Message);
                                SetConnectionState(ChannelConnectionState.Disconnected);

                                errorCount++;
                                lastCallWasAuthentication = false; //so if we get another request to authenticate we'll give it a shot.

                                if (CanRetry(maxRetries, errorCount))
                                {
                                    if (errorCount > 1)
                                    {
                                        //back down our rate.
                                        SleepForConnection();
                                    }                                    
                                }
                                else
                                {
                                    //we can't retry any more - throw an exception
                                    if ((ex.Status == WebExceptionStatus.ConnectFailure) || (ex.Status == WebExceptionStatus.NameResolutionFailure))
                                    {
                                        //throw a specific exception here.
                                        throw new WebChannelConnectFailureException("Unable to connect to the destination server", ex, (ex.Response == null) ? null : ex.Response.ResponseUri);
                                    }
                                    else if (ex.Status == WebExceptionStatus.ConnectionClosed)
                                    {
                                        throw new WebChannelConnectFailureException("Server refused connection", ex, (ex.Response == null) ? null : ex.Response.ResponseUri);
                                    }
                                    else
                                    {
                                        throw new WebChannelException(ex.Message, ex, (ex.Response == null) ? null : ex.Response.ResponseUri);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //this is just here so we can log exceptions that we're letting get thrown.
                if (EnableLogging) m_Logger.Write(LogMessageSeverity.Verbose, ex, true, LogCategory, ex.Message, "While executing a web channel request an exception was thrown, which will be thrown to our caller.\r\nRequest: {0}\r\n", newRequest);
                throw;
            }
        }

        /// <summary>
        /// Authenticate now (instead of waiting for a request to fail)
        /// </summary>
        public void Authenticate()
        {
            if (EnableLogging) m_Logger.Write(LogMessageSeverity.Verbose, LogCategory, "Attempting to authenticate communication channel", null);
            IWebAuthenticationProvider authenticationProvider;
            lock(m_Lock)
            {
                authenticationProvider = m_AuthenticationProvider;
                System.Threading.Monitor.PulseAll(m_Lock);
            }

            if (authenticationProvider == null)
            {
                if (EnableLogging) m_Logger.Write(LogMessageSeverity.Verbose, LogCategory, "Unable to authenticate communication channel", "There is no authentication provider available to process the current authentication request.");
                return; //nothing to do.
            }

            EnsureConnectionInitialized();

            //we do our own try catch JUST for translating errors that mean access denied.
            try
            {
                ResetChannel(null); //we may have set headers, etc, and we're passing them the raw connection.
                m_AuthenticationProvider.Login(this, m_Connection);
            }
            catch (WebException ex)
            {
                //is this an access denied error?
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    //get the inner web response to figure out exactly what the deal is.
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        //throw our dedicated file not found exception.
                        throw new WebChannelFileNotFoundException("File not found", ex, response.ResponseUri);
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized) //it's an auth error
                    {
                        //create a new exception that tells our caller it's an authorization problem.
                        throw new WebChannelAuthorizationException("Username or password not valid", ex, ex.Response.ResponseUri);
                    }
                }

                //otherwise we just re-throw what we got.
                throw;
            }
        }

        /// <summary>
        /// Cancel the current request.
        /// </summary>
        public void Cancel()
        {
            m_Cancel = true;

            //if we're in an async operation, cancel that too.
            lock (m_ResponseLock)
            {
                if ((m_Connection != null) && (m_Connection.IsBusy))
                    m_Connection.CancelAsync();

                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }

            lock(m_Lock)
            {
                //we're just doing this to force sleepers to awaken
                System.Threading.Monitor.PulseAll(m_Lock);
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

        /// <summary>
        /// Downloads the resource with the specified URI to a byte array
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns>A byte array containing the body of the response from the resource</returns>
        public byte[] DownloadData(string relativeUrl, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, null);

                System.Threading.Monitor.PulseAll(m_RequestLock);

                return m_Connection.DownloadData(relativeUrl);
            }
        }


        /// <summary>
        /// Downloads the resource with the specified URI to a byte array
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="additionalHeaders">Extra headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns>A byte array containing the body of the response from the resource</returns>
        public byte[] DownloadData(string relativeUrl, IList<NameValuePair<string>> additionalHeaders, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, null, additionalHeaders);

                byte[] results;
                try
                {
                    results = m_Connection.DownloadData(relativeUrl);

                    //can't use this one as long as we're using web client.
                    //results = DownloadDataAsync(relativeUrl, timeout ?? DefaultTimeoutSeconds);
                }
                finally
                {
                    ResetChannel(additionalHeaders);
                }

                System.Threading.Monitor.PulseAll(m_RequestLock);

                return results;
            }
        }

        /// <summary>
        /// Downloads the resource with the specified URI to a local file.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="destinationFileName"></param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        public void DownloadFile(string relativeUrl, string destinationFileName, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, null);

                m_Connection.DownloadFile(relativeUrl, destinationFileName);
               
                System.Threading.Monitor.PulseAll(m_RequestLock);
            }
        }

        /// <summary>
        /// Downloads the resource with the specified URI to a string
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns></returns>
        public string DownloadString(string relativeUrl, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, null);

                System.Threading.Monitor.PulseAll(m_RequestLock);

                return m_Connection.DownloadString(relativeUrl);
            }
        }

        /// <summary>
        /// Uploads the provided byte array to the specified URI using the provided method.
        /// </summary>
        /// <param name="relativeUrl">The URI of the resource to receive the data. This URI must identify a resource that can accept a request sent with the method.</param>
        /// <param name="method">The HTTP method used to send the string to the resource.  If null, the default is POST</param>
        /// <param name="contentType">The content type to inform the server of for this file</param>
        /// <param name="data"></param>
        /// <param name="additionalHeaders">Extra headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns>A byte array containing the body of the response from the resource</returns>
        public byte[] UploadData(string relativeUrl, string method, string contentType, byte[] data, IList<NameValuePair<string>> additionalHeaders, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, ref method, contentType, additionalHeaders);

                System.Threading.Monitor.PulseAll(m_RequestLock);

                return m_Connection.UploadData(relativeUrl, method, data);
            }
        }

        /// <summary>
        /// Uploads the specified local file to the specified URI using the specified method
        /// </summary>
        /// <param name="relativeUrl">The URI of the resource to receive the file. This URI must identify a resource that can accept a request sent with the method requested.</param>
        /// <param name="method">The HTTP method used to send the string to the resource.  If null, the default is POST</param>
        /// <param name="contentType">The content type to inform the server of for this file</param>
        /// <param name="sourceFileNamePath"></param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns>A byte array containing the body of the response from the resource</returns>
        public byte[] UploadFile(string relativeUrl, string method, string contentType, string sourceFileNamePath, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, ref method, contentType, null);

                System.Threading.Monitor.PulseAll(m_RequestLock);

                return m_Connection.UploadFile(relativeUrl, method, sourceFileNamePath);
            }
        }

        /// <summary>
        /// Uploads the specified string to the specified resource, using the specified method
        /// </summary>
        /// <param name="relativeUrl">The URI of the resource to receive the string. This URI must identify a resource that can accept a request sent with the method requested.</param>
        /// <param name="method">The HTTP method used to send the string to the resource.  If null, the default is POST</param>
        /// <param name="contentType">The content type to inform the server of for this file</param>
        /// <param name="data">The string to be uploaded. </param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns>A string containing the body of the response from the resource</returns>
        public string UploadString(string relativeUrl, string method, string contentType, string data, int? timeout)
        {
            lock (m_RequestLock)
            {
                PreProcessRequest(ref relativeUrl, ref method, contentType, null);

                System.Threading.Monitor.PulseAll(m_RequestLock);
                
                return m_Connection.UploadString(relativeUrl, method, data);
            }
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Dispose managed objects
        /// </summary>
        /// <param name="releaseManagedObjects"></param>
        protected virtual void Dispose(bool releaseManagedObjects)
        {
            if (releaseManagedObjects && (m_Disposed == false))
            {
                Cancel();
                if (m_Connection != null)
                {
                    m_Connection.Dispose();
                    m_Connection = null;
                }
                m_Disposed = true;
            }
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
                var e = new ChannelConnectionStateChangedEventArgs(state);
                tempEvent.Invoke(this, e);
            }
        }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// A synchronous wrapper around the async download data feature.  Used because async operations don't timeout.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="timeout">The maximum amount of time to wait, in seconds</param>
        /// <returns></returns>
        private byte[] DownloadDataAsync(string relativeUrl, int timeout)
        {
            m_DownloadDataRespose = null;
            m_Connection.DownloadDataCompleted += Connection_DownloadDataCompleted;

            try
            {
                m_Connection.DownloadDataAsync(GenerateUri(relativeUrl));

                //and now we wait, because we're a block around an async operation.
                DownloadDataCompletedEventArgs response;
                DateTimeOffset requestDeadline = DateTimeOffset.UtcNow.AddSeconds(timeout);
                lock (m_ResponseLock)
                {
                    while (m_Connection.IsBusy) 
                    {
                        System.Threading.Monitor.Wait(m_ResponseLock, 256); 

                        if (requestDeadline <= DateTimeOffset.UtcNow)
                        {
                            m_Connection.CancelAsync();
                        }
                    }
                    //and here we must be complete...
                    response = m_DownloadDataRespose;

                    System.Threading.Monitor.PulseAll(m_ResponseLock);
                }

                if (response == null)
                    throw new GibraltarNetworkException("Unable to complete the network request, no response could be determined.");

                //if the result of the download was an exception we need to rethrow that now.
                if (response.Error != null)
                    throw response.Error;

                if (response.Cancelled)
                    throw new GibraltarTimeoutException(string.Format("The request did not complete within the allowed time of {0} seconds.", timeout));

                return response.Result;
            }
            finally
            {
                m_Connection.DownloadDataCompleted -= Connection_DownloadDataCompleted;
                m_DownloadDataRespose = null;
            }
        }

        /// <summary>
        /// A synchronous wrapper around the async download file feature.  Used because async operations don't timeout.
        /// </summary>
        /// <returns></returns>
        private void DownloadFileAsync(string relativeUrl, string destinationFileName, int timeout)
        {
            m_DownloadFileRespose = null;
            m_Connection.DownloadFileCompleted += Connection_DownloadFileCompleted;

            try
            {

                m_Connection.DownloadFileAsync(GenerateUri(relativeUrl), destinationFileName);

                //and now we wait, because we're a block around an async operation.
                AsyncCompletedEventArgs response;
                DateTimeOffset requestDeadline = DateTimeOffset.UtcNow.AddSeconds(timeout);
                lock (m_ResponseLock)
                {
                    while (m_Connection.IsBusy)
                    {
                        System.Threading.Monitor.Wait(m_ResponseLock, 1000);

                        if (requestDeadline <= DateTimeOffset.UtcNow)
                        {
                            m_Connection.CancelAsync();
                        }
                    }
                    //and here we must be complete...
                    response = m_DownloadFileRespose;

                    System.Threading.Monitor.PulseAll(m_ResponseLock);
                }

                if (response == null)
                    throw new GibraltarNetworkException("Unable to complete the network request, no response could be determined.");

                //if the result of the download was an exception we need to rethrow that now.
                if (response.Error != null)
                    throw response.Error;

                if (response.Cancelled)
                    throw new GibraltarTimeoutException(string.Format("The request did not complete within the allowed time of {0} seconds.", timeout));

                return;
            }
            finally
            {
                m_Connection.DownloadFileCompleted -= Connection_DownloadFileCompleted;
                m_DownloadFileRespose = null;
            }
        }

        /// <summary>
        /// A synchronous wrapper around the async download string feature.  Used because async operations don't timeout.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="timeout">The number of seconds to wait for a response to the request</param>
        /// <returns></returns>
        private string DownloadStringAsync(string relativeUrl, int timeout)
        {
            m_DownloadDataRespose = null;
            m_Connection.DownloadStringCompleted += Connection_DownloadStringCompleted;

            try
            {
                m_Connection.DownloadStringAsync(GenerateUri(relativeUrl));

                //and now we wait, because we're a block around an async operation.
                DownloadStringCompletedEventArgs response;
                DateTimeOffset requestDeadline = DateTimeOffset.UtcNow.AddSeconds(timeout);
                lock (m_ResponseLock)
                {
                    while (m_Connection.IsBusy)
                    {
                        System.Threading.Monitor.Wait(m_ResponseLock, 256);

                        if (requestDeadline <= DateTimeOffset.UtcNow)
                        {
                            m_Connection.CancelAsync();
                        }
                    }

                    //and here we must be complete...
                    response = m_DownloadStringRespose;

                    System.Threading.Monitor.PulseAll(m_ResponseLock);
                }

                if (response == null)
                    throw new GibraltarNetworkException("Unable to complete the network request, no response could be determined.");

                //if the result of the download was an exception we need to rethrow that now.
                if (response.Error != null)
                    throw response.Error;

                if (response.Cancelled)
                    throw new GibraltarTimeoutException(string.Format("The request did not complete within the allowed time of {0} seconds.", timeout));

                return response.Result;
            }
            finally
            {
                m_Connection.DownloadStringCompleted -= Connection_DownloadStringCompleted;
                m_DownloadDataRespose = null;
            }
        }

        /// <summary>
        /// A synchronous wrapper around the async upload data feature.  Used because async operations don't timeout.
        /// </summary>
        /// <returns></returns>
        private byte[] UploadDataAsync(string relativeUrl, string method, byte[] data)
        {
            m_UploadDataRespose = null;
            m_Connection.UploadDataCompleted += Connection_UploadDataCompleted;

            try
            {
                m_Connection.UploadDataAsync(GenerateUri(relativeUrl), method, data);

                //and now we wait, because we're a block around an async operation.
                UploadDataCompletedEventArgs response;
                lock (m_ResponseLock)
                {
                    while (m_UploadDataRespose == null)
                    {
                        System.Threading.Monitor.Wait(m_ResponseLock, 1000); //the timeout shouldn't be necessary due to events, but better safe than sorry...
                    }

                    //and here we must be complete...
                    response = m_UploadDataRespose;

                    System.Threading.Monitor.PulseAll(m_ResponseLock);
                }

                //if the result of the download was an exception we need to rethrow that now.
                if (response.Error != null)
                    throw response.Error;

                return response.Result;
            }
            finally
            {
                m_Connection.UploadDataCompleted -= Connection_UploadDataCompleted;
                m_UploadDataRespose = null;
            }
        }

        /// <summary>
        /// A synchronous wrapper around the async upload data feature.  Used because async operations don't timeout.
        /// </summary>
        /// <returns></returns>
        private byte[] UploadFileAsync(string relativeUrl, string method, string sourceFileNamePath)
        {
            m_UploadFileRespose = null;
            m_Connection.UploadFileCompleted += Connection_UploadFileCompleted;

            try
            {
                m_Connection.UploadFileAsync(GenerateUri(relativeUrl), method, sourceFileNamePath);

                //and now we wait, because we're a block around an async operation.
                UploadFileCompletedEventArgs response;
                lock (m_ResponseLock)
                {
                    while (m_UploadFileRespose == null)
                    {
                        System.Threading.Monitor.Wait(m_ResponseLock, 1000); //the timeout shouldn't be necessary due to events, but better safe than sorry...
                    }

                    //and here we must be complete...
                    response = m_UploadFileRespose;

                    System.Threading.Monitor.PulseAll(m_ResponseLock);
                }

                //if the result of the download was an exception we need to rethrow that now.
                if (response.Error != null)
                    throw response.Error;

                return response.Result;
            }
            finally
            {
                m_Connection.UploadFileCompleted -= Connection_UploadFileCompleted;
                m_UploadFileRespose = null;
            }
        }

        private void EnsureConnectionInitialized()
        {
            lock (m_Lock)
            {
                if (m_Connection == null)
                {
                    var newConnection = new ExtendedTimeoutWebClient();

                    newConnection.BaseAddress = CalculateBaseAddress();
                    newConnection.Encoding = Encoding.UTF8;
                    newConnection.Timeout = 120 * 1000; //2 minutes

                    newConnection.Proxy = WebRequest.DefaultWebProxy;
                    if (newConnection.Proxy == null)
                    {
                        newConnection.Proxy = WebRequest.GetSystemWebProxy();
                    }

                    if ((newConnection.Proxy != null) && (newConnection.Proxy.Credentials == null))
                    {
                        newConnection.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }

                    m_BaseAddress = new Uri(newConnection.BaseAddress);
                    m_Connection = newConnection;
                }

                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        /// <summary>
        /// Indicates if we can continue retrying.
        /// </summary>
        /// <param name="maxRetries"></param>
        /// <param name="errorCount"></param>
        /// <returns></returns>
        private bool CanRetry(int maxRetries, int errorCount)
        {
            return ((maxRetries < 0) || (errorCount <= maxRetries));
        }

        private string CalculateBaseAddress()
        {
            bool usePort = true;
            if ((m_UseSsl == false) && ((m_Port == 0) || (m_Port == 80)))
            {
                usePort = false;
            }
            else if ((m_UseSsl) && ((m_Port == 0) || (m_Port == 443)))
            {
                usePort = false;
            }

            StringBuilder baseAddress = new StringBuilder(1024);

            baseAddress.AppendFormat("{0}://{1}", UseSsl ? "https" : "http", HostName);

            if (usePort)
            {
                baseAddress.AppendFormat(":{0}", m_Port);
            }

            if (string.IsNullOrEmpty(m_ApplicationBaseDirectory) == false)
            {
                baseAddress.Append(m_ApplicationBaseDirectory);
            }

            return baseAddress.ToString();
        }

        /// <summary>
        /// Clears request-specific channel data (such as headers) so they don't accidentally bleed into a new request.
        /// </summary>
        private void ResetChannel(IList<NameValuePair<string>> additionalHeaders)
        {
            m_Connection.Headers.Remove(HeaderRequestMethod);
            m_Connection.Headers.Remove(HeaderRequestTimestamp);
            m_Connection.Headers.Remove(HeaderRequestAppProtocolVersion);

            if (additionalHeaders != null)
            {
                foreach (NameValuePair<string> additionalHeader in additionalHeaders)
                {
                    m_Connection.Headers.Remove((string) additionalHeader.Name);
                }
            }
        }

        /// <summary>
        /// Performs request processing just prior to execution but after the IWebRequest has called.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="contentType"></param>
        private void PreProcessRequest(ref string relativeUrl, string contentType)
        {
            string method = "GET";
            PreProcessRequest(ref relativeUrl, ref method, contentType, null);
        }

        /// <summary>
        /// Performs request processing just prior to execution but after the IWebRequest has called.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="contentType"></param>
        /// <param name="additionalHeaders"></param>
        private void PreProcessRequest(ref string relativeUrl, string contentType, IList<NameValuePair<string>> additionalHeaders)
        {
            string method = "GET";
            PreProcessRequest(ref relativeUrl, ref method, contentType, additionalHeaders);
        }

        /// <summary>
        /// Performs request processing just prior to execution but after the IWebRequest has called.
        /// </summary>
        private void PreProcessRequest(ref string relativeUrl, ref string method, string contentType, IList<NameValuePair<string>> additionalHeaders)
        {
            //see if we've ever attempted a request - if not we're going to do some first time things.
            if (m_FirstRequest)
            {
                m_UseCompatibilityMethods = GetUseCompatiblilityMethodsOverride(m_HostName);
                m_UseHttpVersion10 = GetUseHttpVersion10Override(m_HostName);

                m_FirstRequest = false;
            }
            
            //get rid of any leading slashes
            relativeUrl = (relativeUrl.StartsWith("/") ? relativeUrl.Substring(1) : relativeUrl);

            //remove any single request-specific items from the call (like all the headers)
            ResetChannel(additionalHeaders);

            //put in any additional headers we got.  By doing them first, if they conflict with one of our headers
            //the conflict will be resolved in favor of the base implementation, forcing the dev to deal with their error first.
            if (additionalHeaders != null)
            {
                foreach (NameValuePair<string> additionalHeader in additionalHeaders)
                {
                    m_Connection.Headers[(string) additionalHeader.Name] = additionalHeader.Value;
                }
            }

            //set the content type for the request
            m_Connection.Headers[HeaderContentType] = contentType;

            //see if we need to override the method.  I'm just sick and tired of !@%@ IIS blocking PUT and DELETE.
            if (m_UseCompatibilityMethods)
            {
                method = method.ToUpperInvariant();
                if (method.Equals("PUT") || method.Equals("DELETE"))
                {
                    m_Connection.Headers[HeaderRequestMethod] = method;

                    //and override the method back to post, which will work.
                    method = "POST";
                }
            }

            m_Connection.UseHttpVersion10 = m_UseHttpVersion10;

            //add our request timestamp so everyone agrees (in ISO 8601 format)
            m_Connection.Headers[HeaderRequestTimestamp] = DateTimeOffset.UtcNow.ToString("o");

            //and if we have a protocol version the caller is using specify that so the server knows.
            if (AppProtocolVersion != null)
            {
                m_Connection.Headers[HeaderRequestAppProtocolVersion] = AppProtocolVersion.ToString();
            }

            //Extension our authentication headers if there is an authentication object
            if (m_AuthenticationProvider != null)
            {
                m_AuthenticationProvider.PreProcessRequest(this, m_Connection, relativeUrl, m_RequestSupportsAuthentication);
            }
        }

        private void SleepForConnection()
        {
            //adjust our delay because there's yet another error.
            int delayIncrement = Math.Min(m_RetryDelaySeconds * 2, 5);
            m_RetryDelaySeconds += delayIncrement;
            m_RetryDelaySeconds = Math.Min(m_RetryDelaySeconds, MaximumRetryDelaySeconds);

            //and wait that long.
            DateTimeOffset waitEndDt = DateTimeOffset.Now.AddSeconds(m_RetryDelaySeconds);
            DateTimeOffset currentTimestamp = DateTimeOffset.Now; //so we have reliable and consistent comparisons

            //we are using the lock so we can get poked if we're supposed to cancel.
            lock (m_Lock)
            {
                while ((m_Cancel == false) && (currentTimestamp < waitEndDt))
                {
                    //we wait up to the duration from here to the end date.  The math.min avoids magic timing problems with 0 and -1.
                    System.Threading.Monitor.Wait(m_Lock, Math.Min((int)(waitEndDt - currentTimestamp).TotalMilliseconds, 1));
                    currentTimestamp = DateTimeOffset.Now;
                }

                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        /// <summary>
        /// Set a new connection state, raising an event if it has changed.
        /// </summary>
        /// <param name="newState"></param>
        private void SetConnectionState(ChannelConnectionState newState)
        {
            bool stateChanged = false;

            //while we can atomically read or write connection state, we want to a get and set.
            lock(m_Lock)
            {
                if (newState != m_ConnectionState)
                {
                    stateChanged = true;
                    m_ConnectionState = newState;
                }

                System.Threading.Monitor.PulseAll(m_Lock);
            }

            //only raise the event if we changed the state, and now we're outside of the lock so it's safe.
            if (stateChanged)
                OnConnectionStateChanged(newState);
        }

        private Uri GenerateUri(string relativeUrl)
        {
            var targetUri = new Uri(m_BaseAddress, relativeUrl);

            return targetUri;
        }

        /// <summary>
        /// Get the current cached compatibility method setting for a server.
        /// </summary>
        /// <param name="server">The DNS name of the server</param>
        /// <returns></returns>
        private bool GetUseCompatiblilityMethodsOverride(string server)
        {
            //don't forget that we have to lock shared collections, they aren't threadsafe
            bool useCompatibilityMethods = true; //in the end it was just too painful to get all those exceptions.
            lock(s_ServerUseCompatibilitySetting)
            {
                bool rawValue;
                if (s_ServerUseCompatibilitySetting.TryGetValue(server, out rawValue))
                {
                    useCompatibilityMethods = rawValue;
                }

                System.Threading.Monitor.PulseAll(s_ServerUseCompatibilitySetting);
            }

            return useCompatibilityMethods;
        }

        /// <summary>
        /// Get the current cached compatibility method setting for a server.
        /// </summary>
        /// <param name="server">The DNS name of the server</param>
        /// <returns></returns>
        private bool GetUseHttpVersion10Override(string server)
        {
            //don't forget that we have to lock shared collections, they aren't threadsafe
            bool useHttpVerison10 = m_UseHttpVersion10;
            lock (s_ServerUseHttpVersion10Setting)
            {
                bool rawValue;
                if (s_ServerUseHttpVersion10Setting.TryGetValue(server, out rawValue))
                {
                    useHttpVerison10 = rawValue;
                }

                System.Threading.Monitor.PulseAll(s_ServerUseHttpVersion10Setting);
            }

            return useHttpVerison10;
        }

        /// <summary>
        /// Update the cached compatibility methods setting for a server (we assume a server will either need it or not)
        /// </summary>
        /// <param name="server">The DNS name of the server</param>
        /// <param name="useCompatibilityMethods">the new setting</param>
        private void SetUseCompatiblilityMethodsOverride(string server, bool useCompatibilityMethods)
        {
            //remember: generic collections are not threadsafe.
            lock(s_ServerUseCompatibilitySetting)
            {
                if (s_ServerUseCompatibilitySetting.ContainsKey(server))
                {
                    s_ServerUseCompatibilitySetting[server] = useCompatibilityMethods;
                }
                else
                {
                    s_ServerUseCompatibilitySetting.Add(server, useCompatibilityMethods);
                }

                System.Threading.Monitor.PulseAll(s_ServerUseCompatibilitySetting);
            }
        }


        /// <summary>
        /// Update the cached Http protocol version setting for a server (we assume a server will either need it or not)
        /// </summary>
        /// <param name="server">The DNS name of the server</param>
        /// <param name="useHttpVersion10">the new setting</param>
        private void SetUseHttpVersion10Override(string server, bool useHttpVersion10)
        {
            //remember: generic collections are not threadsafe.
            lock (s_ServerUseHttpVersion10Setting)
            {
                if (s_ServerUseHttpVersion10Setting.ContainsKey(server))
                {
                    s_ServerUseHttpVersion10Setting[server] = useHttpVersion10;
                }
                else
                {
                    s_ServerUseHttpVersion10Setting.Add(server, useHttpVersion10);
                }

                System.Threading.Monitor.PulseAll(s_ServerUseHttpVersion10Setting);
            }
        }

        #endregion

        #region Event Handlers

        private void Connection_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            //we use a lock for high performance hints back to the originating event.
            lock (m_ResponseLock)
            {
                m_DownloadDataRespose = e;
                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }
        }

        private void Connection_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //we use a lock for high performance hints back to the originating event.
            lock (m_ResponseLock)
            {
                m_DownloadFileRespose = e;
                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }
        }

        private void Connection_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //we use a lock for high performance hints back to the originating event.
            lock (m_ResponseLock)
            {
                m_DownloadStringRespose = e;
                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }
        }

        private void Connection_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            //we use a lock for high performance hints back to the originating event.
            lock (m_ResponseLock)
            {
                m_UploadDataRespose = e;
                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }
        }

        private void Connection_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            //we use a lock for high performance hints back to the originating event.
            lock (m_ResponseLock)
            {
                m_UploadFileRespose = e;
                System.Threading.Monitor.PulseAll(m_ResponseLock);
            }
        }

        #endregion
    }
}
