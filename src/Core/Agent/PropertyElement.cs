using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// An application defined name/value pair to be persisted with the session.
    /// </summary>
    public class PropertyElement : ConfigurationSection
    {
        /// <summary>
        /// The unique name of the property to be persisted
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get => (string)this["name"];
            set => this["name"] = value;
        }

        /// <summary>
        /// The value to be persisted.
        /// </summary>
        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public string Value
        {
            get => (string)this["value"];
            set => this["value"] = value;
        }
    }
}
