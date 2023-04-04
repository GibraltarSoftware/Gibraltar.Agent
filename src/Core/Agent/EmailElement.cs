using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The email communication configuration
    /// </summary>
    public class EmailElement : LoupeElementBase
    {
        /// <inheritdoc />
        public EmailElement() 
            : base("LOUPE__EMAIL__")
        {
        }

        /// <summary>
        /// The full DNS name of the server to use for exchanging email
        /// </summary>
        [ConfigurationProperty("server", DefaultValue = "", IsRequired = false)]
        public string Server
        {
            get => ReadString("server");
            set => this["server"] = value;
        }

        /// <summary>
        /// Optional.  The user to authenticate to the server with.  If provided a password is required.
        /// </summary>
        [ConfigurationProperty("user", DefaultValue = "", IsRequired = false)]
        public string User
        {
            get => ReadString("user");
            set => this["user"] = value;
        }

        /// <summary>
        /// Optional.  The password to authenticate to the server with.  Only used if a user is provided.
        /// </summary>
        [ConfigurationProperty("password", DefaultValue = "", IsRequired = false)]
        public string Password
        {
            get => ReadString("password");
            set => this["password"] = value;
        }

        /// <summary>
        /// Optional.  The TCP/IP port to connect to the server on.  If not specified the default (25) will be used.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 0, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 65535)]
        public int Port
        {
            get => ReadInt("port");
            set => this["port"] = value;
        }

        /// <summary>
        /// When true any email will be encrypted using SSL.
        /// </summary>
        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public bool UseSsl
        {
            get => ReadBoolean("useSsl");
            set => this["useSsl"] = value;
        }

        /// <summary>
        /// Optional.  The maximum size an email message can be when submitted to this server.  If not specified then 10MB will be assumed.
        /// </summary>
        [ConfigurationProperty("maxMessageSize", DefaultValue = 10, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 2048)]
        public int MaxMessageSize
        {
            get => ReadInt("maxMessageSize");
            set => this["maxMessageSizeMb"] = value;
        }

        /// <summary>
        /// Optional.  A default from email address to use for messages that don't specify an address.
        /// </summary>
        [ConfigurationProperty("fromAddress", DefaultValue = "", IsRequired = false)]
        public string FromAddress
        {
            get => ReadString("fromAddress");
            set => this["fromAddress"] = value;
        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "server");
            LoadEnvironmentVariable(environmentVars, "user");
            LoadEnvironmentVariable(environmentVars, "password");
            LoadEnvironmentVariable(environmentVars, "port");
            LoadEnvironmentVariable(environmentVars, "useSsl");
            LoadEnvironmentVariable(environmentVars, "maxMessageSize");
            LoadEnvironmentVariable(environmentVars, "fromAddress");
        }
    }
}
