

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// The top level configuration class for all Agent configuration. Supplied during a
    /// Log.Initializing event.
    /// </summary>
    /// <remarks>
    ///     This object is created by the agent and supplied to user code during the <see cref="Gibraltar.Agent.Log.Initializing">Log.Initializing</see> event to allow for
    ///     configuration to be determined in code at runtime. This configuration is applied
    ///     over whatever has been configured in the application configuration file.
    /// </remarks>
    /// <example>
    /// 	<code lang="CS" title="Programmatic Configuration" description="You can supply some or all of your configuration information during the Log.Initializing event. In this example, the Loupe Server configuration is being done at runtime during this event.">
    /// 		<![CDATA[
    /// /// <summary>
    /// /// The primary program entry point.
    /// /// </summary>
    /// static class Program
    /// {
    ///     /// <summary>
    ///     /// The main entry point for the application.
    ///     /// </summary>
    ///     [STAThread]
    ///     public static void Main()
    ///     {
    ///         Log.Initializing += Log_Initializing;
    ///  
    ///         Application.EnableVisualStyles();
    ///         Application.SetCompatibleTextRenderingDefault(false);
    ///         Thread.CurrentThread.Name = "User Interface Main";  //set the thread name before our first call that logs on this thread.
    ///  
    ///         Log.StartSession("Starting Gibraltar Analyst");
    ///  
    ///         //here you actual start up your application
    ///  
    ///         //and if we got to this point, we done good and can mark the session as being not crashed :)
    ///         Log.EndSession("Exiting Gibraltar Analyst");
    ///     }
    ///  
    ///     static void Log_Initializing(object sender, LogInitializingEventArgs e)
    ///     {
    ///         //and configure Loupe Server Connection
    ///         ServerConfiguration server = e.Configuration.Server;
    ///         server.UseGibraltarService = true;
    ///         server.CustomerName = "Gibraltar Software";
    ///         server.AutoSendSessions = true;
    ///         server.SendAllApplications = true;
    ///         server.PurgeSentSessions = true;
    ///     }
    /// }]]>
    /// 	</code>
    /// </example>
    public sealed class AgentConfiguration
    {
        private const string GibraltarNode = "gibraltar";
        private const string XPathGibraltar = "//" + GibraltarNode;

        private readonly Monitor.AgentConfiguration m_WrappedAgentConfiguration;

        private AutoSendConsentConfiguration m_AutoSendConsent;
        private EmailConfiguration m_Email;
        private PublisherConfiguration m_Publisher;
        private ListenerConfiguration m_Listener;
        private SessionFileConfiguration m_SessionFile;
        private ExportFileConfiguration m_ExportFile;
        private ServerConfiguration m_Server;
        private ViewerConfiguration m_Viewer;
        private PackagerConfiguration m_Packager;
        private NetworkViewerConfiguration m_Network;

        /// <summary>
        /// Create a new agent configuration, starting with the application's configuration file data.
        /// </summary>
        public AgentConfiguration()
        {
            m_WrappedAgentConfiguration = new Monitor.AgentConfiguration();
        }

        /// <summary>
        /// Create a new agent configuration, starting with the specified file instead of the application's configuration file data.
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="FileNotFoundException">The configuration file couldn't be found</exception>
        /// <exception cref="InvalidDataException">The configuration file is in the wrong format</exception>
        public AgentConfiguration(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            var configDocument = new XmlDocument();

            if (File.Exists(fileName))
            {
                //try to load the configuration
                configDocument.Load(fileName);
            }
            else
            {
                throw new FileNotFoundException("The configuration file couldn't be found", fileName);
            }

            //now navigate down to the Gibraltar section.
            var gibraltarNode = configDocument.SelectSingleNode(XPathGibraltar);

            if (gibraltarNode == null)
            {
                throw new InvalidDataException("No gibraltar configuration section could be found in the provided XML file.");
            }

            m_WrappedAgentConfiguration = new Monitor.AgentConfiguration(gibraltarNode);
        }

        /// <summary>
        /// Load the current configuration from the application's configuration data.
        /// </summary>
        internal AgentConfiguration(Monitor.AgentConfiguration wrappedConfiguration)
        {
            m_WrappedAgentConfiguration = wrappedConfiguration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Configures consent requirements for automatic background transmission of session data
        /// </summary>
        public AutoSendConsentConfiguration AutoSendConsent
        {
            get
            {
                if (m_AutoSendConsent == null)
                {
                    //create a new wrapped object.
                    m_AutoSendConsent = new AutoSendConsentConfiguration(m_WrappedAgentConfiguration.AutoSendConsent);
                }

                return m_AutoSendConsent;
            }
        }

        /// <summary>The email server connection configuration.</summary>
        public EmailConfiguration Email
        {
            get
            {
                if (m_Email == null)
                {
                    //create a new wrapped object.
                    m_Email = new EmailConfiguration(m_WrappedAgentConfiguration.Email);
                }

                return m_Email;
            }
        }

        /// <summary>
        /// The listener configuration
        /// </summary>
        public ListenerConfiguration Listener
        {
            get
            {
                if (m_Listener == null)
                {
                    //create a new wrapped object.
                    m_Listener = new ListenerConfiguration(m_WrappedAgentConfiguration.Listener);
                }

                return m_Listener;
            }
        }

        /// <summary>The session data file configuration</summary>
        public SessionFileConfiguration SessionFile
        {
            get
            {
                if (m_SessionFile == null)
                {
                    //create a new wrapped object
                    m_SessionFile = new SessionFileConfiguration(m_WrappedAgentConfiguration.SessionFile);
                }

                return m_SessionFile;
            }
        }

        /// <summary>The session data file configuration</summary>
        public ExportFileConfiguration ExportFile
        {
            get
            {
                if (m_ExportFile == null)
                {
                    //create a new wrapped object
                    m_ExportFile = new ExportFileConfiguration(m_WrappedAgentConfiguration.ExportFile);
                }

                return m_ExportFile;
            }
        }

        /// <summary>
        /// The packager configuration
        /// </summary>
        public PackagerConfiguration Packager
        {
            get
            {
                if (m_Packager == null)
                {
                    //create a new wrapped object
                    m_Packager = new PackagerConfiguration(m_WrappedAgentConfiguration.Packager);
                }

                return m_Packager;
            }
        }

        /// <summary>
        /// The publisher configuration
        /// </summary>
        public PublisherConfiguration Publisher
        {
            get
            {
                if (m_Publisher == null)
                {
                    //create a new wrapped object
                    m_Publisher = new PublisherConfiguration(m_WrappedAgentConfiguration.Publisher);
                }

                return m_Publisher;
            }
        }

        /// <summary>
        /// The central server configuration
        /// </summary>
        public ServerConfiguration Server
        {
            get
            {
                if (m_Server == null)
                {
                    //create a new wrapped object
                    m_Server = new ServerConfiguration(m_WrappedAgentConfiguration.Server);
                }

                return m_Server;
            }
        }

        /// <summary>
        /// The viewer configuration
        /// </summary>
        public ViewerConfiguration Viewer
        {
            get
            {
                if (m_Viewer == null)
                {
                    //create a new wrapped object
                    m_Viewer = new ViewerConfiguration(m_WrappedAgentConfiguration.Viewer);
                }

                return m_Viewer;
            }
        }

        /// <summary>
        /// Configures consent requirements for automatic background transmission of session data
        /// </summary>
        public NetworkViewerConfiguration NetworkViewer
        {
            get
            {
                if (m_Network == null)
                {
                    //create a new wrapped object
                    m_Network = new NetworkViewerConfiguration(m_WrappedAgentConfiguration.NetworkViewer);
                }

                return m_Network;
            }
        }

        /// <summary>
        /// Application defined properties
        /// </summary>
        public Dictionary<string, string> Properties
        {
            get
            {
                return m_WrappedAgentConfiguration.Properties;
            }
        }

        /// <summary>
        /// Save the current configuration to the specified file, overwriting it if it already exists
        /// </summary>
        /// <param name="fileName">The name of the file to write to</param>
        public void Save(string fileName)
        {
            //open an XML document for the target file.
            var configDocument = new XmlDocument();

            //we'll need to create a new XML document.
            //if we didn't find an existing configuration, make an empty one.
            XmlDeclaration xmlDeclaration = configDocument.CreateXmlDeclaration("1.0", "utf-8", null);

            // Create the root element
            XmlElement rootNode = configDocument.CreateElement(GibraltarNode);
            configDocument.InsertBefore(xmlDeclaration, configDocument.DocumentElement);
            configDocument.AppendChild(rootNode);

            //now navigate down to the Gibraltar section.
            var gibraltarNode = configDocument.SelectSingleNode(XPathGibraltar);
            m_WrappedAgentConfiguration.Save(gibraltarNode);

            File.Delete(fileName);
            configDocument.Save(fileName);
        }

        /// <summary>
        /// The Agent configuration usable by internal assemblies.
        /// </summary>
        internal Monitor.AgentConfiguration WrappedObject { get { return m_WrappedAgentConfiguration; } }

        #endregion
    }
}
