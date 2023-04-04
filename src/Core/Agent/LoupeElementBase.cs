using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// Extends configuration section with common configuration overrides 
    /// </summary>
    public abstract class LoupeElementBase : ConfigurationSection
    {
        private volatile bool _environmentVariablesLoaded = false;

        private readonly string _environmentVariablePrefix;

        private readonly Dictionary<string, string> _environmentVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected LoupeElementBase(string environmentVariablePrefix)
        {
            _environmentVariablePrefix = environmentVariablePrefix;
        }

        /// <summary>
        /// Load up our set of environment variables if they aren't already.
        /// </summary>
        private void EnsureEnvironmentVariablesLoaded()
        {
            if (_environmentVariablesLoaded == false)
            {
                try
                {
                    _environmentVariablesLoaded = true; //we only give this one shot.
                    var environmentVars = CommonCentralLogic.LoadEnvironmentVariables(); //this is a common cache.

                    OnLoadEnvironmentVars(environmentVars);
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Invoked to load all of the environment variables that match the configuration elements for this section.
        /// </summary>
        /// <param name="environmentVars"></param>
        protected abstract void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars);

        /// <summary>
        /// Load the specified environment variable if present for deferred use as configuration.
        /// </summary>
        /// <param name="environmentVars">The collection of all environment variables</param>
        /// <param name="configPropertyName">The name of the configuration property to map the variable to.</param>
        protected void LoadEnvironmentVariable(IDictionary<string, string> environmentVars, string configPropertyName)
        {
            var environmentVariableName = _environmentVariablePrefix + configPropertyName;
            if (environmentVars.TryGetValue(environmentVariableName, out var value))
            {
                _environmentVars.Add(configPropertyName, value);
            }
        }

        /// <summary>
        /// Read a configuration property
        /// </summary>
        /// <param name="configPropertyName">The (typically short) name of the configuration property</param>
        /// <returns>The value from a matching environment variable if it is set, otherwise the configuration value (and default)</returns>
        protected bool ReadBoolean(string configPropertyName)
        {
            EnsureEnvironmentVariablesLoaded();

            //we default to using the environment variable if it's present.
            if (_environmentVars?.TryGetValue(configPropertyName, out var rawValue) == true)
            {
                if (string.IsNullOrEmpty(rawValue) == false)
                {
                    if (bool.TryParse(rawValue, out var value))
                    {
                        return value;
                    }
                }
            }

            return (bool)this[configPropertyName];
        }

        /// <summary>
        /// Read a configuration property
        /// </summary>
        /// <param name="configPropertyName">The (typically short) name of the configuration property</param>
        /// <returns>The value from a matching environment variable if it is set, otherwise the configuration value (and default)</returns>
        protected int ReadInt(string configPropertyName)
        {
            EnsureEnvironmentVariablesLoaded();

            //we default to using the environment variable if it's present.
            if (_environmentVars?.TryGetValue(configPropertyName, out var rawValue) == true)
            {
                if (string.IsNullOrEmpty(rawValue) == false)
                {
                    if (int.TryParse(rawValue, out var value))
                    {
                        return value;
                    }
                }
            }

            return (int)this[configPropertyName];
        }

        /// <summary>
        /// Read a configuration property
        /// </summary>
        /// <param name="configPropertyName">The (typically short) name of the configuration property</param>
        /// <returns>The value from a matching environment variable if it is set, otherwise the configuration value (and default)</returns>
        protected string ReadString(string configPropertyName)
        {
            EnsureEnvironmentVariablesLoaded();

            //we default to using the environment variable if it's present.
            if (_environmentVars?.TryGetValue(configPropertyName, out var rawValue) == true)
            {
                if (string.IsNullOrEmpty(rawValue) == false)
                {
                    return rawValue;
                }
            }

            return (string)this[configPropertyName];
        }
    }
}
