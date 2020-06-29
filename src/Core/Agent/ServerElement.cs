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

using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// The application configuration information for sending session summaries and data to a Loupe Server
    /// </summary>
    public class ServerElement : ConfigurationSection
    {
        /// <summary>
        /// True by default, disables server communication when false..
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// Indicates whether to automatically send session data to the server in the background.
        /// </summary>
        /// <remarks>Defaults to false, indicating data will only be sent on request via packager.</remarks>
        [ConfigurationProperty("autoSendSessions", DefaultValue = false, IsRequired = false)]
        public bool AutoSendSessions
        {
            get { return (bool)this["autoSendSessions"]; }
            set { this["autoSendSessions"] = value; }
        }

        /// <summary>
        /// Indicates whether to automatically send data to the server when error or critical messages are logged.
        /// </summary>
        /// <remarks>Defaults to true, indicating if the Auto Send Sessions option is also enabled data will be sent
        /// to the server after an error occurs (unless overridden by the MessageAlert event).</remarks>
        [ConfigurationProperty("autoSendOnError", DefaultValue = true, IsRequired = false)]
        public bool AutoSendOnError
        {
            get { return (bool)this["autoSendOnError"]; }
            set { this["autoSendOnError"] = value; }
        }

        /// <summary>
        /// Indicates whether to send data about all applications for this product to the server or just this application (the default)
        /// </summary>
        /// <remarks>Defaults to false, indicating just the current applications data will be sent.  Requires that AutoSendSessions is enabled.</remarks>
        [ConfigurationProperty("sendAllApplications", DefaultValue = false, IsRequired = false)]
        public bool SendAllApplications
        {
            get { return (bool)this["sendAllApplications"]; } 
            set { this["sendAllApplications"] = value; }
        }

        /// <summary>
        /// Indicates whether to remove sessions that have been sent from the local repository once confirmed by the server.
        /// </summary>
        /// <remarks>Defaults to false.  Requires that AutoSendSessions is enabled.</remarks>
        [ConfigurationProperty("purgeSentSessions", DefaultValue = false, IsRequired = false)]
        public bool PurgeSentSessions
        {
            get { return (bool)this["purgeSentSessions"]; }
            set { this["purgeSentSessions"] = value; }
        }

        /// <summary>
        /// The application key to use to communicate with the Loupe Server
        /// </summary>
        /// <remarks>Application keys identify the specific repository and optionally an application environment service
        /// for this session's data to be associated with.  The server administrator can determine by application key
        /// whether to accept the session data or not.</remarks>
        [ConfigurationProperty("applicationKey", DefaultValue = "", IsRequired = false)]
        public string ApplicationKey
        {
            get
            {
                return (string)this["applicationKey"];
            }
            set
            {
                this["applicationKey"] = value;
            }
        }

        /// <summary>
        /// The unique customer name when using the Gibraltar Loupe Service
        /// </summary>
        [ConfigurationProperty("customerName", DefaultValue = "", IsRequired = false)]
        public string CustomerName
        {
            get
            {
                return (string)this["customerName"];
            }
            set
            {
                this["customerName"] = value;
            }
        }

        /// <summary>
        /// Indicates if the Gibraltar Loupe Service should be used instead of a private Loupe Server
        /// </summary>
        /// <remarks>If true then the customer name must be specified.</remarks>
        [ConfigurationProperty("useGibraltarService", DefaultValue = false, IsRequired = false)]
        public bool UseGibraltarService
        {
            get
            {
                return (bool)this["useGibraltarService"];
            }
            set
            {
                this["useGibraltarService"] = value;
            }
        }

        /// <summary>
        /// Indicates if the connection to the Loupe Server should be encrypted with Ssl. 
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server.</remarks>
        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public bool UseSsl
        {
            get
            {
                return (bool)this["useSsl"];
            }
            set
            {
                this["useSsl"] = value;
            }
        }

        /// <summary>
        /// The full DNS name of the server where the Loupe Server is located
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server.</remarks>
        [ConfigurationProperty("server", DefaultValue = "", IsRequired = false)]
        public string Server
        {
            get
            {
                return (string)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }

        /// <summary>
        ///  An optional port number override for the server
        /// </summary>
        /// <remarks>Not required if the port is the traditional port (80 or 443).  Only applies to a private Loupe Server.</remarks>
        [ConfigurationProperty("port", DefaultValue = 0, IsRequired = false)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        /// <summary>
        /// The virtual directory on the host for the private Loupe Server
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server.</remarks>
        [ConfigurationProperty("applicationBaseDirectory", DefaultValue = "", IsRequired = false)]
        public string ApplicationBaseDirectory
        {
            get
            {
                return (string)this["applicationBaseDirectory"];
            }
            set
            {
                this["applicationBaseDirectory"] = value;
            }
        }

        /// <summary>
        /// The specific repository on the server to send the session to
        /// </summary>
        /// <remarks>Only applies to a private Loupe Server running Enterprise Edition.</remarks>
        [ConfigurationProperty("repository", DefaultValue = "", IsRequired = false)]
        public string Repository
        {
            get
            {
                return (string)this["repository"];
            }
            set
            {
                this["repository"] = value;
            }
        }
    }
}
