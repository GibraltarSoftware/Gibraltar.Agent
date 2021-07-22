
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

