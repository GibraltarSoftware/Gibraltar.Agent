using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gibraltar.Agent
{
    /// <summary>
    /// Extends configuration section with common configuration overrides 
    /// </summary>
    public abstract class LoupeElementBase : ConfigurationSection
    {
        private readonly string _environmentVariablePrefix;

        private Dictionary<string, string> _environmentVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected LoupeElementBase(string environmentVariablePrefix)
        {
            _environmentVariablePrefix = environmentVariablePrefix;
            try
            {
                var environmentVars = Environment.GetEnvironmentVariables();

                if (environmentVars != null)
                {
                    OnLoadEnvironmentVars(environmentVars);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Invoked to load all of the environment variables that match the configuration elements for this section.
        /// </summary>
        /// <param name="environmentVars"></param>
        protected abstract void OnLoadEnvironmentVars(IDictionary environmentVars);

        /// <summary>
        /// Load the specified environment variable if present for deferred use as configuration.
        /// </summary>
        /// <param name="environmentVars">The collection of all environment variables</param>
        /// <param name="environmentVariableName">The full name (not case sensitive) of the environment variable to read</param>
        /// <param name="configPropertyName">The name of the configuration property to map the variable to.</param>
        protected void LoadEnvironmentVariable(IDictionary environmentVars, string configPropertyName)
        {
            string environmentVariableName = _environmentVariablePrefix + configPropertyName;
            var value = environmentVars[environmentVariableName];
            if (value != null)
            {
                _environmentVars.Add(configPropertyName, value as string);
            }
        }

        protected bool ReadBoolean(string configPropertyName)
        {
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

        protected int ReadInt(string configPropertyName)
        {
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

        protected string ReadString(string configPropertyName)
        {
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
