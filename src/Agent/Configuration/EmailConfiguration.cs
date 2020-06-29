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

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// Indicates the default method that the agent should use to connect to an email server.
    /// </summary>
    public sealed class EmailConfiguration
    {
        private readonly Monitor.EmailConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the email configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal EmailConfiguration(Monitor.EmailConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The full DNS name of the server to use for exchanging email
        /// </summary>
        public string Server
        {
            get { return m_WrappedConfiguration.Server; }
            set { m_WrappedConfiguration.Server = value; }
        }

        /// <summary>
        /// Optional.  The user to authenticate to the server with.  If provided a password is required.
        /// </summary>
        public string User
        {
            get { return m_WrappedConfiguration.User; }
            set { m_WrappedConfiguration.User = value; }
        }

        /// <summary>
        /// Optional.  The password to authenticate to the server with.  Only used if a user is provided.
        /// </summary>
        public string Password
        {
            get { return m_WrappedConfiguration.Password; }
            set { m_WrappedConfiguration.Password = value; }
        }

        /// <summary>
        /// Optional.  The TCP/IP port to connect to the server on.  If not specified the default (25) will be used.
        /// </summary>
        public int Port
        {
            get { return m_WrappedConfiguration.Port; }
            set { m_WrappedConfiguration.Port = value; }
        }

        /// <summary>
        /// When true any email will be encrypted using SSL.
        /// </summary>
        public bool UseSsl
        {
            get { return m_WrappedConfiguration.UseSsl; }
            set { m_WrappedConfiguration.UseSsl = value; }
        }


        /// <summary>
        /// Optional.  The maximum size an email message can be when submitted to this server.  If not specified then 10MB will be assumed.
        /// </summary>
        public int MaxMessageSize
        {
            get { return m_WrappedConfiguration.MaxMessageSize; }
            set { m_WrappedConfiguration.MaxMessageSize = value; }
        }


        #endregion
    }
}
