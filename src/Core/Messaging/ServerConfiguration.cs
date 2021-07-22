
using System;
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Monitor;
using Gibraltar.Server.Client;

namespace Gibraltar.Messaging
{
    /// <summary>
    /// The configuration for the agent to connect with a session data server
    /// </summary>
    public class ServerConfiguration : IServerConfiguration
    {
        /// <summary>
        /// Create a new empty server configuration
        /// </summary>
        public ServerConfiguration()
        {
            
        }

        /// <summary>
        /// Initialize the server configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        public ServerConfiguration(ServerElement configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            //copy the configuration
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the server configuration from an XML document
        /// </summary>
        internal ServerConfiguration(XmlNode gibraltarNode)
        {
            //create an element object so we have something to draw defaults from.
            ServerElement baseline = new ServerElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("server");

            //copy the provided configuration
            if (node != null)
            {
                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                AutoSendSessions = AgentConfiguration.ReadValue(node, "autoSendSessions", baseline.AutoSendSessions);
                AutoSendOnError = AgentConfiguration.ReadValue(node, "autoSendOnError", baseline.AutoSendOnError);
                SendAllApplications = AgentConfiguration.ReadValue(node, "sendAllApplications", baseline.SendAllApplications);
                PurgeSentSessions = AgentConfiguration.ReadValue(node, "purgeSentSessions", baseline.PurgeSentSessions);
                UseGibraltarService = AgentConfiguration.ReadValue(node, "useGibraltarService", baseline.UseGibraltarService);
                ApplicationKey = AgentConfiguration.ReadValue(node, "applicationKey", baseline.ApplicationKey);
                CustomerName = AgentConfiguration.ReadValue(node, "customerName", baseline.CustomerName);
                Server = AgentConfiguration.ReadValue(node, "server", baseline.Server);
                Port = AgentConfiguration.ReadValue(node, "port", baseline.Port);
                UseSsl = AgentConfiguration.ReadValue(node, "useSsl", baseline.UseSsl);
                ApplicationBaseDirectory = AgentConfiguration.ReadValue(node, "applicationBaseDirectory", baseline.ApplicationBaseDirectory);
                Repository = AgentConfiguration.ReadValue(node, "repository", baseline.Repository);
            }
        }

        #region Public Properties and Methods

        /// <summary>
        /// True by default, disables server communication when false..
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether to automatically send session data to the server in the background.
        /// </summary>
        /// <remarks>Defaults to false, indicating data will only be sent on request via packager.</remarks>
        public bool AutoSendSessions { get; set; }

        /// <summary>
        /// Indicates whether to automatically send data to the server when error or critical messages are logged.
        /// </summary>
        /// <remarks>Defaults to true, indicating if the Auto Send Sessions option is also enabled data will be sent
        /// to the server after an error occurs (unless overridden by the MessageAlert event).</remarks>
        public bool AutoSendOnError { get; set; }
       
        /// <summary>
        /// Indicates whether to send data about all applications for this product to the server or just this application (the default)
        /// </summary>
        /// <remarks>Defaults to false, indicating just the current applications data will be sent</remarks>
        public bool SendAllApplications { get; set; }

        /// <summary>
        /// Indicates whether to remove sessions that have been sent from the local repository once confirmed by the server.
        /// </summary>
        public bool PurgeSentSessions { get; set; }

        /// <summary>
        /// The application key to use to communicate with the Loupe Server
        /// </summary>
        /// <remarks>Application keys identify the specific repository and optionally an application environment service
        /// for this session's data to be associated with.  The server administrator can determine by application key
        /// whether to accept the session data or not.</remarks>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// The unique customer name when using the Loupe Service
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Indicates if the Loupe Service should be used instead of a private server
        /// </summary>
        /// <remarks>If true then the customer name must be specified.</remarks>
        public bool UseGibraltarService { get; set; }

        /// <summary>
        /// Indicates if the connection should be encrypted with Ssl. 
        /// </summary>
        /// <remarks>Only applies to a private server.</remarks>
        public bool UseSsl { get; set; }

        /// <summary>
        /// The full DNS name of the server where the service is located
        /// </summary>
        /// <remarks>Only applies to a private server.</remarks>
        public string Server { get; set; }

        /// <summary>
        ///  An optional port number override for the server
        /// </summary>
        /// <remarks>Not required if the port is the traditional port (80 or 443).  Only applies to a private server.</remarks>
        public int Port { get; set; }

        /// <summary>
        /// The virtual directory on the host for the private service
        /// </summary>
        /// <remarks>Only applies to a private server.</remarks>
        public string ApplicationBaseDirectory { get; set; }

        /// <summary>
        /// The specific repository on the server to send the session to
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server running Enterprise Edition.</remarks>
        public string Repository { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            ServerElement baseline = new ServerElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("server");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "autoSendSessions", AutoSendSessions, baseline.AutoSendSessions);
            AgentConfiguration.WriteValue(newNode, "autoSendOnError", AutoSendOnError, baseline.AutoSendOnError);
            AgentConfiguration.WriteValue(newNode, "sendAllApplications", SendAllApplications, baseline.SendAllApplications);
            AgentConfiguration.WriteValue(newNode, "purgeSentSessions", PurgeSentSessions, baseline.PurgeSentSessions);
            AgentConfiguration.WriteValue(newNode, "useGibraltarService", UseGibraltarService, baseline.UseGibraltarService);
            AgentConfiguration.WriteValue(newNode, "applicationKey", ApplicationKey ?? string.Empty, baseline.ApplicationKey);
            AgentConfiguration.WriteValue(newNode, "customerName", CustomerName ?? string.Empty, baseline.CustomerName);
            AgentConfiguration.WriteValue(newNode, "server", Server ?? string.Empty, baseline.Server);
            AgentConfiguration.WriteValue(newNode, "port", Port, baseline.Port);
            AgentConfiguration.WriteValue(newNode, "useSsl", UseSsl, baseline.UseSsl);
            AgentConfiguration.WriteValue(newNode, "applicationBaseDirectory", ApplicationBaseDirectory ?? string.Empty, baseline.ApplicationBaseDirectory);
            AgentConfiguration.WriteValue(newNode, "repository", Repository ?? string.Empty, baseline.Repository);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        /// <summary>
        /// Load the specified configuration, overwriting our current configuration
        /// </summary>
        /// <param name="configuration">The configuration to clone</param>
        public void Load(ServerConfiguration configuration)
        {
            Enabled = configuration.Enabled;
            AutoSendSessions = configuration.AutoSendSessions;
            AutoSendOnError = configuration.AutoSendOnError;
            SendAllApplications = configuration.SendAllApplications;
            PurgeSentSessions = configuration.PurgeSentSessions;
            UseGibraltarService = configuration.UseGibraltarService;
            ApplicationKey = configuration.ApplicationKey;
            CustomerName = configuration.CustomerName;
            Server = configuration.Server;
            Port = configuration.Port;
            UseSsl = configuration.UseSsl;
            ApplicationBaseDirectory = configuration.ApplicationBaseDirectory;
            Repository = configuration.Repository;
        }

        /// <summary>
        /// Check the current configuration information to see if it's valid for a connection, throwing relevant exceptions if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid with the specific problem indicated in the message</exception>
        public void Validate()
        {
            HubConnection.ValidateConfiguration(ApplicationKey, UseGibraltarService, CustomerName, Server, Port, UseSsl, ApplicationBaseDirectory);
        }

        #endregion

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (string.IsNullOrEmpty(Server))
            {
                Server = null;
            }
            else
            {
                Server = Server.Trim();
                if (Server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    Server = Server.Substring(8);
                }
                else if (Server.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    Server = Server.Substring(7);
                }

                if (Server.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    Server = Server.Substring(0, Server.Length - 1);
                }
            }

            ApplicationKey = string.IsNullOrEmpty(ApplicationKey) ? null : ApplicationKey.Trim();

            CustomerName = string.IsNullOrEmpty(CustomerName) ? null : CustomerName.Trim();

            Repository = string.IsNullOrEmpty(Repository) ? null : Repository.Trim();

            if (string.IsNullOrEmpty(ApplicationBaseDirectory))
            {
                ApplicationBaseDirectory = null;
            }
            else
            {
                ApplicationBaseDirectory = ApplicationBaseDirectory.Trim();

                if (ApplicationBaseDirectory.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    ApplicationBaseDirectory = ApplicationBaseDirectory.Substring(1);
                }

                if (ApplicationBaseDirectory.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    ApplicationBaseDirectory = ApplicationBaseDirectory.Substring(0, ApplicationBaseDirectory.Length - 1);
                }
            }

            if (Port < 0)
                Port = 0;

            if ((UseGibraltarService && string.IsNullOrEmpty(CustomerName) && string.IsNullOrEmpty(ApplicationKey))
                || (!UseGibraltarService && string.IsNullOrEmpty(Server)))
                Enabled = false; //we can't be enabled because we aren't plausibly configured.
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(ServerElement configuration)
        {
            //copy the configuration
            Enabled = configuration.Enabled;
            AutoSendSessions = configuration.AutoSendSessions;
            AutoSendOnError = configuration.AutoSendOnError;
            SendAllApplications = configuration.SendAllApplications;
            PurgeSentSessions = configuration.PurgeSentSessions;
            UseGibraltarService = configuration.UseGibraltarService;
            ApplicationKey = configuration.ApplicationKey;
            CustomerName = configuration.CustomerName;
            Server = configuration.Server;
            Port = configuration.Port;
            UseSsl = configuration.UseSsl;
            ApplicationBaseDirectory = configuration.ApplicationBaseDirectory;
            Repository = configuration.Repository;
        }

        #endregion
    }
}