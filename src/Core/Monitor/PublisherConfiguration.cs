using System;
using System.Configuration;
using System.Xml;
using Gibraltar.Agent;
using Loupe.Extensibility.Data;

namespace Gibraltar.Monitor
{
    /// <summary>
    /// The configuration of the publisher.
    /// </summary>
    public class PublisherConfiguration
    {
        private XmlNode m_GibraltarNode;

        /// <summary>
        /// Initialize the publisher from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal PublisherConfiguration(PublisherElement configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the publisher from an XML document.
        /// </summary>
        internal PublisherConfiguration(XmlNode gibraltarNode)
        {
            m_GibraltarNode = gibraltarNode;

            //create an element object so we have something to draw defaults from.
            PublisherElement baseline = new PublisherElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("publisher");

            //copy the provided configuration
            if (node != null)
            {
                DisableMemoryOptimization = AgentConfiguration.ReadValue(node, "disableMemoryOptimization", baseline.DisableMemoryOptimization);
                EnableAnonymousMode = AgentConfiguration.ReadValue(node, "enableAnonymousMode", baseline.EnableAnonymousMode);
                EnableDebugMode = AgentConfiguration.ReadValue(node, "enableDebugMode", baseline.EnableDebugMode);
                ForceSynchronous = AgentConfiguration.ReadValue(node, "forceSynchronous", baseline.ForceSynchronous);
                MaxQueueLength = AgentConfiguration.ReadValue(node, "maxQueueLength", baseline.MaxQueueLength);
                ProductName = AgentConfiguration.ReadValue(node, "productName", baseline.ProductName);
                ApplicationName = AgentConfiguration.ReadValue(node, "applicationName", baseline.ApplicationName);
                EnvironmentName = AgentConfiguration.ReadValue(node, "environmentName", baseline.EnvironmentName);
                PromotionLevelName = AgentConfiguration.ReadValue(node, "promotionLevelName", baseline.PromotionLevelName);
                string storedAppType = AgentConfiguration.ReadValue(node, "applicationType", baseline.ApplicationType);
                ApplicationDescription = AgentConfiguration.ReadValue(node, "applicationDescription", baseline.ApplicationDescription);
                string storedVersion = AgentConfiguration.ReadValue(node, "applicationVersion", (string)null);

                if (string.IsNullOrEmpty(storedVersion))
                {
                    ApplicationVersion = null;
                }
                else
                {
                    try
                    {
                        ApplicationVersion = new Version(storedVersion);
                    }
                    catch
                    {
                        ApplicationVersion = null;
                    }
                }

                if (string.IsNullOrEmpty(storedAppType))
                {
                    ApplicationType = ApplicationType.Unknown;
                }
                else
                {
                    ApplicationType = ParseApplicationType(storedAppType);
                }
            }

            Sanitize();
        }

        #region Public Properties and Methods

        /// <summary>
        /// Optional.  The name of the product for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        public string ProductName { get; set; }

        /// <summary>
        /// Optional.  A description of the application to include with the session information.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// application description from the assemblies that initiate logging.</remarks>
        public string ApplicationDescription { get; set; }

        /// <summary>
        /// Optional.  The name of the application for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Optional.  The environment this session is running in.
        /// </summary>
        /// <remarks>Environments are useful for categorizing sessions, for example to 
        /// indicate the hosting environment. If a value is provided it will be 
        /// carried with the session data to upstream servers and clients.  If the 
        /// corresponding entry does not exist it will be automatically created.</remarks>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Optional.  The promotion level of the session.
        /// </summary>
        /// <remarks>Promotion levels are useful for categorizing sessions, for example to 
        /// indicate whether it was run in development, staging, or production. 
        /// If a value is provided it will be carried with the session data to upstream servers and clients.  
        /// If the corresponding entry does not exist it will be automatically created.</remarks>
        public string PromotionLevelName { get; set; }

        /// <summary>
        /// Optional.  The ApplicationType to treat the application as, overriding the Agent's automatic determination.
        /// </summary>
        /// <remarks>This setting is not generally necessary as the Agent will automatically determine the application
        /// type correctly in most typical windows services, console apps, WinForm applications, and ASP.NET applications.
        /// If the automatic determination is unsuccessful or incorrect with a particular application, the correct type
        /// can be configured with this setting to bypass the automatic determination.  However, setting this incorrectly
        /// for the application could have undesirable effects.</remarks>
        public ApplicationType ApplicationType { get; set; } // Note: Should it restrict certain detectable mismatches?

        /// <summary>
        /// Optional.  The version of the application for logging purposes.
        /// </summary>
        /// <remarks><para>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</para></remarks>
        public Version ApplicationVersion { get; set; }

        /// <summary>
        /// When true, the publisher will treat all publish requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed to every
        /// configured messenger.</remarks>
        public bool ForceSynchronous { get; set; }

        /// <summary>
        /// The maximum number of queued messages waiting to be published.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be published exceeds the
        /// maximum queue length the log publisher will switch to a synchronous mode to 
        /// catch up.  This will cause the client to block until each new message is published.</remarks>
        public int MaxQueueLength { get; set; }

        /// <summary>
        /// When true, the Agent will record session data without collecting personally-identifying information.
        /// </summary>
        /// <remarks>In anonymous mode the Agent will not collect personally-identifying information such as user name,
        /// user domain name, host name, host domain name, and the application's command line.  Anonymous mode is disabled
        /// by default, and normal operation will collect this information automatically.</remarks>
        public bool EnableAnonymousMode { get; set; }

        /// <summary>
        /// When true, the Agent will include debug messages in logs. Not intended for production use
        /// </summary>
        /// <remarks><para>Normally the Agent will fail silently and otherwise compensate for problems to ensure
        /// that it does not cause a problem for your application. When you are developing your application 
        /// you can enable this mode to get more detail about why th Agent is behaving as it is and resolve
        /// issues.</para>
        /// <para>In debug mode the agent may throw exceptions to indicate calling errors it normally would 
        /// just silently ignore. Therefore, this option is not recommended for consistent production use.</para></remarks>
        public bool EnableDebugMode { get; set; }

        /// <summary>
        /// When true, the Agent will not do string compression and other optimizations to minimize memory.
        /// </summary>
        /// <remarks><para>The Agent normally works to minimize memory use in production scenarios, such as
        /// using a string cache to avoid keeping duplicate strings in memory and other steps.  These steps
        /// rely on .NET Garbage Collector features; if there are GC issues in the process they can be
        /// confusing to debug and understand using conventional profilers while these optimizations are being
        /// used.</para>
        /// <para>Setting this option to true will disable these optimizations which will increase the memory used by 
        /// the agent (particularly for log message buffering) but makes a simpler picture for memory profiling.</para></remarks>
        public bool DisableMemoryOptimization { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            PublisherElement baseline = new PublisherElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("publisher");
            AgentConfiguration.WriteValue(newNode, "productName", ProductName ?? string.Empty, baseline.ProductName);
            AgentConfiguration.WriteValue(newNode, "applicationName", ApplicationName ?? string.Empty, baseline.ApplicationName);
            AgentConfiguration.WriteValue(newNode, "environmentName", EnvironmentName ?? string.Empty, baseline.EnvironmentName);
            AgentConfiguration.WriteValue(newNode, "promotionLevelName", PromotionLevelName ?? string.Empty, baseline.PromotionLevelName);
            AgentConfiguration.WriteValue(newNode, "applicationType", ApplicationType.ToString(), baseline.ApplicationType);
            AgentConfiguration.WriteValue(newNode, "applicationDescription", ApplicationDescription ?? string.Empty, baseline.ApplicationDescription);
            AgentConfiguration.WriteValue(newNode, "forceSynchronous", ForceSynchronous, baseline.ForceSynchronous);
            AgentConfiguration.WriteValue(newNode, "maxQueueLength", MaxQueueLength, baseline.MaxQueueLength);
            AgentConfiguration.WriteValue(newNode, "enableAnonymousMode", EnableAnonymousMode, baseline.EnableAnonymousMode);
            AgentConfiguration.WriteValue(newNode, "enableDebugMode", EnableDebugMode, baseline.EnableDebugMode);
            AgentConfiguration.WriteValue(newNode, "disableMemoryOptimization", DisableMemoryOptimization, baseline.DisableMemoryOptimization);

            if (ApplicationVersion != null)
            {
                AgentConfiguration.WriteValue(newNode, "applicationVersion", ApplicationVersion, new Version(0,0));
            }

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        #endregion

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (MaxQueueLength <= 0)
                MaxQueueLength = 2000;
            else if (MaxQueueLength > 50000)
                MaxQueueLength = 2000;

            if (string.IsNullOrEmpty(ProductName))
                ProductName = null;

            if (string.IsNullOrEmpty(ApplicationDescription))
                ApplicationDescription = null;

            if (string.IsNullOrEmpty(ApplicationName))
                ApplicationName = null;

        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(PublisherElement configuration)
        {
            //copy the configuration
            DisableMemoryOptimization = configuration.DisableMemoryOptimization;
            EnableAnonymousMode = configuration.EnableAnonymousMode;
            EnableDebugMode = configuration.EnableDebugMode;
            ForceSynchronous = configuration.ForceSynchronous;
            MaxQueueLength = configuration.MaxQueueLength;
            ProductName = configuration.ProductName;
            ApplicationName = configuration.ApplicationName;
            EnvironmentName = configuration.EnvironmentName;
            PromotionLevelName = configuration.PromotionLevelName;
            ApplicationDescription = configuration.ApplicationDescription;

            string storedVersion = configuration.ApplicationVersion;
            if (string.IsNullOrEmpty(storedVersion))
            {
                ApplicationVersion = null;
            }
            else
            {
                try
                {
                    ApplicationVersion = new Version(storedVersion);
                }
                catch
                {
                    ApplicationVersion = null;
                }
            }

            string storedAppType = configuration.ApplicationType;
            ApplicationType = ParseApplicationType(storedAppType);
        }

        private static ApplicationType ParseApplicationType(string appTypeString)
        {
            ApplicationType typeVal = ApplicationType.Unknown;
            if (string.IsNullOrEmpty(appTypeString) == false)
            {
                switch (appTypeString.ToUpper())
                {
                    case "WINDOWS":
                        //case "WINFORM":
                        //case "WINFORMS":
                        //case "WPF": // ToDo: Should this be a separate enum val?
                        typeVal = ApplicationType.Windows;
                        break;
                    case "ASPNET":
                        //case "ASP":
                        //case "WEB":
                        typeVal = ApplicationType.AspNet;
                        break;
                    case "SERVICE":
                        typeVal = ApplicationType.Service;
                        break;
                    case "CONSOLE":
                        typeVal = ApplicationType.Console;
                        break;
                    case "UNKNOWN":
                        //case "DEFAULT":
                        //case "AUTO":
                        //case "DETECT":
                        typeVal = ApplicationType.Unknown;
                        break;
                }
            }
            return typeVal;
        }

        #endregion
    }
}
