﻿#region File Header and License
// /*
//    EntityFrameworkElement.cs
//    Copyright 2013 Gibraltar Software, Inc.
//    
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// */
#endregion
using System;
using System.Configuration;

namespace Gibraltar.Agent.EntityFramework.Configuration
{
    /// <summary>
    /// Configuration options for the Loupe Agent for Entity Framework
    /// </summary>
    public class EntityFrameworkElement : ConfigurationSection
    {
        /// <summary>
        /// The root log category for this agent
        /// </summary>
        internal const string LogCategory = "Data Access";

        /// <summary>
        /// Determines if any agent functionality should be enabled.  Defaults to true.
        /// </summary>
        /// <remarks>To disable the entire agent set this option to false.  Even if individual
        /// options are enabled they will be ignored if this is set to false.</remarks>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// Determines if the call stack for each operation should be recorded
        /// </summary>
        /// <remarks>This is useful for determining what application code causes each query</remarks>
        [ConfigurationProperty("logCallStack", DefaultValue = false, IsRequired = false)]
        public bool LogCallStack { get { return (bool)this["logCallStack"]; } set { this["logCallStack"] = value; } }

        /// <summary>
        /// Determines if the agent writes a log message for each SQL operation.  Defaults to true.
        /// </summary>
        /// <remarks>Set to false to disable writing log messages for each SQL operation before they are run.
        /// For database-heavy applications this can create a significant volume of log data, but does not
        /// affect overall application performance.</remarks>
        [ConfigurationProperty("logQuery", DefaultValue = true, IsRequired = false)]
        public bool LogQuery { get { return (bool)this["logQuery"]; } set { this["logQuery"] = value; } }

        /// <summary>
        /// The severity used for log messages for the Entity Framework trace message. Defaults to Verbose.
        /// </summary>
        [ConfigurationProperty("queryMessageSeverity", DefaultValue = LogMessageSeverity.Verbose, IsRequired = false)]
        public LogMessageSeverity QueryMessageSeverity { get { return (LogMessageSeverity)this["queryMessageSeverity"]; } set { this["queryMessageSeverity"] = value; } }

        /// <summary>
        /// Determines if a log message is written for exceptions during entity framework operations. Defaults to true.
        /// </summary>
        [ConfigurationProperty("logExceptions", DefaultValue = true, IsRequired = false)]
        public bool LogExceptions { get { return (bool)this["logExceptions"]; } set { this["logExceptions"] = value; } }

        /// <summary>
        /// The severity used for log messages for entity framework operations that throw an exception. Defaults to Error.
        /// </summary>
        [ConfigurationProperty("exceptionSeverity", DefaultValue = LogMessageSeverity.Error, IsRequired = false)]
        public LogMessageSeverity ExceptionSeverity { get { return (LogMessageSeverity)this["exceptionSeverity"]; } set { this["exceptionSeverity"] = value; } }

        /// <summary>
        /// Load the element from the system configuration file, falling back to defaults if it can't be parsed
        /// </summary>
        /// <returns>A new element object</returns>
        internal static EntityFrameworkElement SafeLoad()
        {
            EntityFrameworkElement configuration = null;
            try
            {
                //see if we can get a configuration section
                configuration = ConfigurationManager.GetSection("gibraltar/entityFramework") as EntityFrameworkElement;
            }
            catch (Exception ex)
            {
                Log.Error(ex, LogCategory + ".Agent", "Unable to load the Entity Framework configuration from the config file",
                          "The default configuration will be used which will may create unexpected behavior.  Exception:\r\n{0}", ex.Message);
            }

            return configuration ?? new EntityFrameworkElement();
        }
    }
}
