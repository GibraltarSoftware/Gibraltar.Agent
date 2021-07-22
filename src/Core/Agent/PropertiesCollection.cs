
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System.Configuration;

#endregion File Header

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
        public PropertyElement this[int idx]
        {
            get
            {
                return (PropertyElement)BaseGet(idx);
            }
        }
    }
}
