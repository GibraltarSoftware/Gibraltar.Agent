using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The configuration of the packager.
    /// </summary>
    public class PackagerElement : ConfigurationSection
    {
        #region Public Properties and Methods

        /// <summary>
        /// The default HotKey configuration string for the packager.
        /// </summary>
        public const string DefaultHotKey = "Ctrl-Alt-F4";

        /// <summary>
        /// The key sequence used to pop up the packager.
        /// </summary>
        [ConfigurationProperty("hotKey", DefaultValue = DefaultHotKey, IsRequired = false)]
        public string HotKey { get => (string)this["hotKey"];
            set => this["hotKey"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to save the package to a file.
        /// </summary>
        [ConfigurationProperty("allowFile", DefaultValue = true, IsRequired = false)]
        public bool AllowFile { get => (bool)this["allowFile"];
            set => this["allowFile"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to save the package directly to the root of a removable media volume
        /// </summary>
        [ConfigurationProperty("allowRemovableMedia", DefaultValue = true, IsRequired = false)]
        public bool AllowRemovableMedia {
            get => (bool)this["allowRemovableMedia"];
            set => this["allowRemovableMedia"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to send sessions to a session data server
        /// </summary>
        [ConfigurationProperty("allowServer", DefaultValue = true, IsRequired = false)]
        public bool AllowServer { get => (bool)this["allowServer"];
            set => this["allowServer"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to send the package via email
        /// </summary>
        [ConfigurationProperty("allowEmail", DefaultValue = true, IsRequired = false)]
        public bool AllowEmail { get => (bool)this["allowEmail"];
            set => this["allowEmail"] = value;
        }

        /// <summary>
        /// The email address to use as the sender&apos;s address
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        [ConfigurationProperty("fromEmailAddress", DefaultValue = "", IsRequired = false)]
        public string FromEmailAddress
        {
            get => (string)this["fromEmailAddress"];
            set => this["fromEmailAddress"] = value;
        }

        /// <summary>
        /// The address to send the email to.
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        [ConfigurationProperty("destinationEmailAddress", DefaultValue = "", IsRequired = false)]
        public string DestinationEmailAddress
        {
            get => (string)this["destinationEmailAddress"];
            set => this["destinationEmailAddress"] = value;
        }

        /// <summary>
        /// The product name to use instead of the current application.
        /// </summary>
        /// <remarks>Primarily used in the Packager.exe.config file to specify the end-user product and application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Gibraltar for the product.
        /// <para>To limit the package to one application within a product specify the applicationName as well
        /// as the productName.  Specifying just the product name will cause the package to contain all applications
        /// for the specified product.</para></remarks>
        [ConfigurationProperty("productName", DefaultValue = "", IsRequired = false)]
        public string ProductName { get => (string)this["productName"];
            set => this["productName"] = value;
        }

        /// <summary>
        /// The application name to use instead of the current application.
        /// </summary>
        /// <remarks><para>Primarily used in the Packager.exe.config file to specify the end-user application
        /// you want to package information for instead of the current application.  If specified, the name
        /// must exactly match the name shown in Gibraltar for the application.</para>
        /// <para>Application name is ignored if product name is not also specified.</para></remarks>
        [ConfigurationProperty("applicationName", DefaultValue = "", IsRequired = false)]
        public string ApplicationName
        {
            get => (string)this["applicationName"];
            set => this["applicationName"] = value;
        }

        #endregion
    }
}
