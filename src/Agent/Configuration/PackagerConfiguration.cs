

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// The configuration of the packager.
    /// </summary>
    public sealed class PackagerConfiguration
    {
        private readonly Monitor.PackagerConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the packager configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal PackagerConfiguration(Monitor.PackagerConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The default HotKey configuration string for the packager.
        /// </summary>
        public const string DefaultHotKey = PackagerElement.DefaultHotKey;

        /// <summary>
        /// The key sequence used to pop up the packager.
        /// </summary>
        public string HotKey
        {
            get { return m_WrappedConfiguration.HotKey; }
            set { m_WrappedConfiguration.HotKey = value; }
        }

        /// <summary>
        /// When true the user will be allowed to save the package to a file.
        /// </summary>
        public bool AllowFile
        {
            get { return m_WrappedConfiguration.AllowFile; }
            set { m_WrappedConfiguration.AllowFile = value; }
        }

        /// <summary>
        /// When true the user will be allowed to save the package directly to the root of a removable media volume
        /// </summary>
        public bool AllowRemovableMedia
        {
            get { return m_WrappedConfiguration.AllowRemovableMedia; }
            set { m_WrappedConfiguration.AllowRemovableMedia = value; }
        }

        /// <summary>
        /// When true the user will be allowed to send the package via email
        /// </summary>
        public bool AllowEmail
        {
            get { return m_WrappedConfiguration.AllowEmail; }
            set { m_WrappedConfiguration.AllowEmail = value; }
        }

        /// <summary>
        /// When true the user will be allowed to send sessions to a session data server
        /// </summary>
        public bool AllowServer
        {
            get { return m_WrappedConfiguration.AllowServer; }
            set { m_WrappedConfiguration.AllowServer = value; }
        }

        /// <summary>
        /// The email address to use as the sender&apos;s address
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        public string FromEmailAddress
        {
            get { return m_WrappedConfiguration.FromEmailAddress; }
            set { m_WrappedConfiguration.FromEmailAddress = value; }
        }

        /// <summary>
        /// The address to send the email to.
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        public string DestinationEmailAddress
        {
            get { return m_WrappedConfiguration.DestinationEmailAddress; }
            set { m_WrappedConfiguration.DestinationEmailAddress = value; }
        }

        /// <summary>
        /// The product name to use instead of the current application.
        /// </summary>
        /// <remarks>Primarily used in the Packager.exe.config file to specify the end-user product and application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Loupe for the product.
        /// <para>To limit the package to one application within a product specify the applicationName as well
        /// as the productName.  Specifying just the product name will cause the package to contain all applications
        /// for the specified product.</para></remarks>
        public string ProductName
        {
            get { return m_WrappedConfiguration.ProductName; }
            set { m_WrappedConfiguration.ProductName = value; }
        }

        /// <summary>
        /// The application name to use instead of the current application.
        /// </summary>
        /// <remarks><para>Primarily used in the Packager.exe.config file to specify the end-user application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Loupe for the application.</para>
        /// <para>Application name is ignored if product name is not also specified.</para></remarks>
        public string ApplicationName
        {
            get { return m_WrappedConfiguration.ApplicationName; }
            set { m_WrappedConfiguration.ApplicationName = value; }
        }

        #endregion
    }
}
