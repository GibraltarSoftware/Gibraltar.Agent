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
    /// The email communication configuration
    /// </summary>
    public class EmailElement : ConfigurationSection
    {
        #region Public Properties and Methods

        /// <summary>
        /// The full DNS name of the server to use for exchanging email
        /// </summary>
        [ConfigurationProperty("server", DefaultValue = "", IsRequired = false)]
        public string Server
        {
            get { return (string)this["server"]; }
            set { this["server"] = value; }
        }

        /// <summary>
        /// Optional.  The user to authenticate to the server with.  If provided a password is required.
        /// </summary>
        [ConfigurationProperty("user", DefaultValue = "", IsRequired = false)]
        public string User
        {
            get { return (string)this["user"]; }
            set { this["user"] = value; }
        }

        /// <summary>
        /// Optional.  The password to authenticate to the server with.  Only used if a user is provided.
        /// </summary>
        [ConfigurationProperty("password", DefaultValue = "", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        /// <summary>
        /// Optional.  The TCP/IP port to connect to the server on.  If not specified the default (25) will be used.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 0, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 65535)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }

        /// <summary>
        /// When true any email will be encrypted using SSL.
        /// </summary>
        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public bool UseSsl
        {
            get { return (bool)this["useSsl"]; }
            set { this["useSsl"] = value; }
        }

        /// <summary>
        /// Optional.  The maximum size an email message can be when submitted to this server.  If not specified then 10MB will be assumed.
        /// </summary>
        [ConfigurationProperty("maxMessageSize", DefaultValue = 10, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 2048)]
        public int MaxMessageSize
        {
            get { return (int)this["maxMessageSize"]; }
            set { this["maxMessageSizeMb"] = value; }
        }

        /// <summary>
        /// Optional.  A default from email address to use for messages that don't specify an address.
        /// </summary>
        [ConfigurationProperty("fromAddress", DefaultValue = "", IsRequired = false)]
        public string FromAddress
        {
            get { return (string)this["fromAddress"]; }
            set { this["fromAddress"] = value; }
        }

        #endregion
    }
}
