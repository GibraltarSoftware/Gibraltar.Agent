#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion

#define STRING_APPTYPE

using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The configuration of the publisher.
    /// </summary>
    public class PublisherElement : ConfigurationSection
    {
        #region Public Properties and Methods

        /// <summary>
        /// Optional.  The name of the product for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        [ConfigurationProperty("productName", DefaultValue = "", IsRequired = false)]
        public string ProductName
        {
            get
            {
                return (string)this["productName"];
            }
            set
            {
                this["productName"] = value;
            }
        }

        /// <summary>
        /// Optional.  A description of the application to include with the session information.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// application description from the assemblies that initiate logging.</remarks>
        [ConfigurationProperty("applicationDescription", DefaultValue = "", IsRequired = false)]
        public string ApplicationDescription
        {
            get
            {
                return (string)this["applicationDescription"];
            }
            set
            {
                this["applicationDescription"] = value;
            }
        }

        /// <summary>
        /// Optional.  The name of the application for logging purposes.
        /// </summary>
        /// <remarks>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</remarks>
        [ConfigurationProperty("applicationName", DefaultValue = "", IsRequired = false)]
        public string ApplicationName
        {
            get
            {
                return (string)this["applicationName"];
            }
            set
            {
                this["applicationName"] = value;
            }
        }

        /// <summary>
        /// Optional.  The environment this session is running in.
        /// </summary>
        /// <remarks>Environments are useful for categorizing sessions, for example to 
        /// indicate the hosting environment. If a value is provided it will be 
        /// carried with the session data to upstream servers and clients.  If the 
        /// corresponding entry does not exist it will be automatically created.</remarks>
        [ConfigurationProperty("environmentName", DefaultValue = "", IsRequired = false)]
        public string EnvironmentName
        {
            get
            {
                return (string)this["environmentName"];
            }
            set
            {
                this["environmentName"] = value;
            }
        }

        /// <summary>
        /// Optional.  The promotion level of the session.
        /// </summary>
        /// <remarks>Promotion levels are useful for categorizing sessions, for example to 
        /// indicate whether it was run in development, staging, or production. 
        /// If a value is provided it will be carried with the session data to upstream servers and clients.  
        /// If the corresponding entry does not exist it will be automatically created.</remarks>
        [ConfigurationProperty("promotionLevelName", DefaultValue = "", IsRequired = false)]
        public string PromotionLevelName
        {
            get
            {
                return (string)this["promotionLevelName"];
            }
            set
            {
                this["promotionLevelName"] = value;
            }
        }

        /// <summary>
        /// Optional.  The ApplicationType to treat the application as, overriding the Agent's automatic determination.
        /// </summary>
        /// <remarks>This setting is not generally necessary as the Agent will automatically determine the application
        /// type correctly in most typical windows services, console apps, WinForm applications, and ASP.NET applications.
        /// If the automatic determination is unsuccessful or incorrect with a particular application, the correct type
        /// can be configured with this setting to bypass the automatic determination.  However, setting this incorrectly
        /// for the application could have undesirable effects.</remarks>
        [ConfigurationProperty("applicationType", DefaultValue = "Unknown", IsRequired = false)]
        public string ApplicationType
        {
            get
            {
                return (string)this["applicationType"];
            }
            set
            {
                this["applicationType"] = value;
            }
        }

        /// <summary>
        /// Optional.  The version of the application for logging purposes.
        /// </summary>
        /// <remarks><para>Generally unnecessary for windows services, console apps, and WinForm applications.
        /// Useful for web applications where there is no reasonable way of automatically determining
        /// product name from the assemblies that initiate logging.</para>
        /// <para>If specified, the version must be in the form X.X.X.X such that it can be parsed 
        /// as a version object.  </para></remarks>
        [ConfigurationProperty("applicationVersion", DefaultValue = "", IsRequired = false)]
        public string ApplicationVersion
        {
            get
            {
                return (string)this["applicationVersion"];
            }
            set
            {
                this["applicationVersion"] = value;
            }
        }
        /// <summary>
        /// When true, the publisher will treat all publish requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed to every
        /// configured messenger.</remarks>
        [ConfigurationProperty("forceSynchronous", DefaultValue = false, IsRequired = false)]
        public bool ForceSynchronous
        {
            get
            {
                return (bool)this["forceSynchronous"];
            }
            set
            {
                this["forceSynchronous"] = value;
            }
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be published.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be published exceeds the
        /// maximum queue length the log publisher will switch to a synchronous mode to 
        /// catch up.  This will cause the client to block until each new message is published.</remarks>
        [ConfigurationProperty("maxQueueLength", DefaultValue = 2000, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 50000)]
        public int MaxQueueLength
        {
            get
            {
                return (int)this["maxQueueLength"];
            }
            set
            {
                this["maxQueueLength"] = value;
            }
        }

        /// <summary>
        /// When true, the Agent will record session data without collecting personally-identifying information.
        /// </summary>
        /// <remarks>In anonymous mode the Agent will not collect personally-identifying information such as user name,
        /// user domain name, host name, host domain name, and the application's command line.  Anonymous mode is disabled
        /// by default, and normal operation will collect this information automatically.</remarks>
        [ConfigurationProperty("enableAnonymousMode", DefaultValue = false, IsRequired = false)]
        public bool EnableAnonymousMode
        {
            get
            {
                return (bool)this["enableAnonymousMode"];
            }
            set
            {
                this["enableAnonymousMode"] = value;
            }
        }
        /// <summary>
        /// When true, the Agent will include debug messages in logs. Not intended for production use
        /// </summary>
        /// <remarks><para>Normally the Agent will fail silently and otherwise compensate for problems to ensure
        /// that it does not cause a problem for your application. When you are developing your application 
        /// you can enable this mode to get more detail about why the Agent is behaving as it is and resolve
        /// issues.</para>
        /// <para>In debug mode the agent may throw exceptions to indicate calling errors it normally would 
        /// just silently ignore. Therefore, this option is not recommended for consistent production use.</para></remarks>
        [ConfigurationProperty("enableDebugMode", DefaultValue = false, IsRequired = false)]
        public bool EnableDebugMode
        {
            get
            {
                return (bool)this["enableDebugMode"];
            }
            set
            {
                this["enableDebugMode"] = value;
            }
        }

        #endregion
    }
}
