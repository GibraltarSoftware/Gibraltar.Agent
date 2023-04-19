using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The configuration of the packager.
    /// </summary>
    public class PackagerElement : LoupeElementBase
    {
        /// <inheritdoc />
        public PackagerElement() : base("LOUPE__PACKAGER__")
        {

        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "hotKey");
            LoadEnvironmentVariable(environmentVars, "allowFile");
            LoadEnvironmentVariable(environmentVars, "allowRemovableMedia");
            LoadEnvironmentVariable(environmentVars, "allowServer");
            LoadEnvironmentVariable(environmentVars, "allowEmail");
            LoadEnvironmentVariable(environmentVars, "fromEmailAddress");
            LoadEnvironmentVariable(environmentVars, "destinationEmailAddress");
            LoadEnvironmentVariable(environmentVars, "productName");
            LoadEnvironmentVariable(environmentVars, "applicationName");
        }

        /// <summary>
        /// The default HotKey configuration string for the packager.
        /// </summary>
        public const string DefaultHotKey = "Ctrl-Alt-F4";

        /// <summary>
        /// The key sequence used to pop up the packager.
        /// </summary>
        [ConfigurationProperty("hotKey", DefaultValue = DefaultHotKey, IsRequired = false)]
        public string HotKey 
        { 
            get => ReadString("hotKey");
            set => this["hotKey"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to save the package to a file.
        /// </summary>
        [ConfigurationProperty("allowFile", DefaultValue = true, IsRequired = false)]
        public bool AllowFile 
        { 
            get => ReadBoolean("allowFile");
            set => this["allowFile"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to save the package directly to the root of a removable media volume
        /// </summary>
        [ConfigurationProperty("allowRemovableMedia", DefaultValue = true, IsRequired = false)]
        public bool AllowRemovableMedia 
        {
            get => ReadBoolean("allowRemovableMedia");
            set => this["allowRemovableMedia"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to send sessions to a session data server
        /// </summary>
        [ConfigurationProperty("allowServer", DefaultValue = true, IsRequired = false)]
        public bool AllowServer 
        { 
            get => ReadBoolean("allowServer");
            set => this["allowServer"] = value;
        }

        /// <summary>
        /// When true the user will be allowed to send the package via email
        /// </summary>
        [ConfigurationProperty("allowEmail", DefaultValue = true, IsRequired = false)]
        public bool AllowEmail 
        { 
            get => ReadBoolean("allowEmail");
            set => this["allowEmail"] = value;
        }

        /// <summary>
        /// The email address to use as the sender&apos;s address
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        [ConfigurationProperty("fromEmailAddress", DefaultValue = "", IsRequired = false)]
        public string FromEmailAddress
        {
            get => ReadString("fromEmailAddress");
            set => this["fromEmailAddress"] = value;
        }

        /// <summary>
        /// The address to send the email to.
        /// </summary>
        /// <remarks>If specified, the user will not be given the option to override it.</remarks>
        [ConfigurationProperty("destinationEmailAddress", DefaultValue = "", IsRequired = false)]
        public string DestinationEmailAddress
        {
            get => ReadString("destinationEmailAddress");
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
        public string ProductName 
        { 
            get => ReadString("productName");
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
            get => ReadString("applicationName");
            set => this["applicationName"] = value;
        }
    }
}
