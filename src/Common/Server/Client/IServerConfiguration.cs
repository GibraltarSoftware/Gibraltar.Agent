

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// The configuration for the agent to connect with a session data server
    /// </summary>
    public interface IServerConfiguration
    {
        /// <summary>
        /// True by default, disables server communication when false..
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether to automatically send session data to the server in the background.
        /// </summary>
        /// <remarks>Defaults to false, indicating data will only be sent on request via packager.</remarks>
        bool AutoSendSessions { get; set; }

        /// <summary>
        /// Indicates whether to automatically send data to the server when error or critical messages are logged.
        /// </summary>
        /// <remarks>Defaults to true, indicating if the Auto Send Sessions option is also enabled data will be sent
        /// to the server after an error occurs (unless overridden by the MessageAlert event).</remarks>
        bool AutoSendOnError { get; set; }
       
        /// <summary>
        /// Indicates whether to send data about all applications for this product to the server or just this application (the default)
        /// </summary>
        /// <remarks>Defaults to false, indicating just the current applications data will be sent</remarks>
        bool SendAllApplications { get; set; }

        /// <summary>
        /// Indicates whether to remove sessions that have been sent from the local repository once confirmed by the server.
        /// </summary>
        bool PurgeSentSessions { get; set; }

        /// <summary>
        /// The application key to use to communicate with the Loupe Server
        /// </summary>
        /// <remarks>Application keys identify the specific repository and optionally an application environment service
        /// for this session's data to be associated with.  The server administrator can determine by application key
        /// whether to accept the session data or not.</remarks>
        string ApplicationKey { get; set; }

        /// <summary>
        /// The unique customer name when using the Loupe Service
        /// </summary>
        string CustomerName { get; set; }

        /// <summary>
        /// Indicates if the Loupe Service should be used instead of a private server
        /// </summary>
        /// <remarks>If true then the customer name must be specified.</remarks>
        bool UseGibraltarService { get; set; }

        /// <summary>
        /// Indicates if the connection should be encrypted with Ssl. 
        /// </summary>
        /// <remarks>Only applies to a private server.</remarks>
        bool UseSsl { get; set; }

        /// <summary>
        /// The full DNS name of the server where the service is located
        /// </summary>
        /// <remarks>Only applies to a private Hub.</remarks>
        string Server { get; set; }

        /// <summary>
        ///  An optional port number override for the server
        /// </summary>
        /// <remarks>Not required if the port is the traditional port (80 or 443).  Only applies to a private server.</remarks>
        int Port { get; set; }

        /// <summary>
        /// The virtual directory on the host for the private service
        /// </summary>
        /// <remarks>Only applies to a private server.</remarks>
        string ApplicationBaseDirectory { get; set; }

        /// <summary>
        /// The specific repository on the server to send the session to
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server running Enterprise Edition.</remarks>
        string Repository { get; set; }
    }
}