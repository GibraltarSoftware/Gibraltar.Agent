using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The set of properties to be persisted with the session
    /// </summary>
    [ConfigurationCollection( typeof(PropertyElement))]
    public class PropertiesCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new PropertyElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PropertyElement)element).Name;
        }

        /// <summary>
        /// Retrieve a property by index from the underlying collection
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public PropertyElement this[int idx] => (PropertyElement)BaseGet(idx);
    }
}
