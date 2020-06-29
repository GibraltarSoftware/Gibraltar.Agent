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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Messaging;
using Gibraltar.Messaging.Export;

namespace Gibraltar.Monitor
{
    /// <summary>
    /// The configuration for the agent.  
    /// </summary>
    public class AgentConfiguration
    {
        private readonly XmlNode m_GibraltarNode;

        private EmailConfiguration m_Email;
        private PublisherConfiguration m_Publisher;
        private ListenerConfiguration m_Listener;
        private FileMessengerConfiguration m_Log;
        private ExportFileMessengerConfiguration m_ExportFile;
        private ViewerMessengerConfiguration m_Viewer;
        private PackagerConfiguration m_Packager;
        private ServerConfiguration m_Server;
        private AutoSendConsentConfiguration m_Consent;
        private NetworkMessengerConfiguration m_Network;
        private Dictionary<string, string> m_Properties;

        /// <summary>
        /// Load the current configuration from the application's configuration data.
        /// </summary>
        public AgentConfiguration()
        {
        }

        /// <summary>
        /// Create a new agent configuration based on the provided XML document.
        /// </summary>
        public AgentConfiguration(XmlNode gibraltarNode)
        {
            if (gibraltarNode == null)
                throw new ArgumentNullException(nameof(gibraltarNode));

            m_GibraltarNode = gibraltarNode;

            object element = Email;
            element = Publisher;
            element = Listener;
            element = SessionFile;
            element = Viewer;
            element = Packager;
            element = Server;
            element = NetworkViewer;
            element = AutoSendConsent;
            element = ExportFile;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Configures consent requirements for automatic background transmission of session data
        /// </summary>
        public AutoSendConsentConfiguration AutoSendConsent
        {
            get
            {
                if (m_Consent == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Consent = new AutoSendConsentConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        AutoSendConsentElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/autoSendConsent") as AutoSendConsentElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Consent = (configuration == null) ? new AutoSendConsentConfiguration(new AutoSendConsentElement()) : new AutoSendConsentConfiguration(configuration);
                    }
                }

                return m_Consent;
            }
        }

        /// <summary>
        /// The listener configuration
        /// </summary>
        public ListenerConfiguration Listener
        {
            get
            {
                if (m_Listener == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Listener = new ListenerConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        ListenerElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/listener") as ListenerElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Listener = (configuration == null) ? new ListenerConfiguration(new ListenerElement()) : new ListenerConfiguration(configuration);
                    }
                }

                return m_Listener;
            }
        }

        /// <summary>
        /// The email connection configuration
        /// </summary>
        public EmailConfiguration Email
        {
            get
            {
                if (m_Email == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Email = new EmailConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        EmailElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/email") as EmailElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Email = (configuration == null) ? new EmailConfiguration(new EmailElement()) : new EmailConfiguration(configuration);
                    }
                }

                return m_Email;
            }
        }

        /// <summary>
        /// The log configuration
        /// </summary>
        public ExportFileMessengerConfiguration ExportFile
        {
            get
            {
                if (m_ExportFile == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_ExportFile = new ExportFileMessengerConfiguration("exportFile", m_GibraltarNode, Server);
                    }
                    else
                    {
                        ExportFileElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/exportFile") as ExportFileElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_ExportFile = (configuration == null) ? new ExportFileMessengerConfiguration("exportFile", new ExportFileElement(), Server) 
                            : new ExportFileMessengerConfiguration("exportFile", configuration, Server);
                    }
                }

                return m_ExportFile;
            }
        }


        /// <summary>
        /// The log configuration
        /// </summary>
        public FileMessengerConfiguration SessionFile
        {
            get
            {
                if (m_Log == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Log = new FileMessengerConfiguration("log", m_GibraltarNode, Server);
                    }
                    else
                    {
                        SessionFileElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/sessionFile") as SessionFileElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Log = (configuration == null) ? new FileMessengerConfiguration("log", new SessionFileElement(), Server) : new FileMessengerConfiguration("log", configuration, Server);
                    }
                }

                return m_Log;
            }
        }

        /// <summary>
        /// Configures consent requirements for automatic background transmission of session data
        /// </summary>
        public NetworkMessengerConfiguration NetworkViewer
        {
            get
            {
                if (m_Network == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Network = new NetworkMessengerConfiguration("networkViewer", m_GibraltarNode);
                    }
                    else
                    {
                        NetworkViewerElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/networkViewer") as NetworkViewerElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Network = (configuration == null) ? new NetworkMessengerConfiguration("networkViewer", new NetworkViewerElement())
                            : new NetworkMessengerConfiguration("networkViewer", configuration);
                    }
                }

                return m_Network;
            }
        }

        /// <summary>
        /// The packager configuration
        /// </summary>
        public PackagerConfiguration Packager
        {
            get
            {
                if (m_Packager == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Packager = new PackagerConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        PackagerElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/packager") as PackagerElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Packager = (configuration == null) ? new PackagerConfiguration(new PackagerElement()) : new PackagerConfiguration(configuration);
                    }
                }

                return m_Packager;
            }
        }

        /// <summary>
        /// The publisher configuration
        /// </summary>
        public PublisherConfiguration Publisher
        {
            get
            {
                if (m_Publisher == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Publisher = new PublisherConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        PublisherElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/publisher") as PublisherElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Publisher = (configuration == null) ? new PublisherConfiguration(new PublisherElement()) : new PublisherConfiguration(configuration);
                    }
                }

                return m_Publisher;
            }
        }

        /// <summary>
        /// The server configuration
        /// </summary>
        public ServerConfiguration Server
        {
            get
            {
                if (m_Server == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Server = new ServerConfiguration(m_GibraltarNode);
                    }
                    else
                    {
                        ServerElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/server") as ServerElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Server = (configuration == null) ? new ServerConfiguration(new ServerElement()) : new ServerConfiguration(configuration);
                    }
                }

                return m_Server;
            }
        }

        /// <summary>
        /// The viewer configuration
        /// </summary>
        public ViewerMessengerConfiguration Viewer
        {
            get
            {
                if (m_Viewer == null)
                {
                    if (m_GibraltarNode != null)
                    {
                        m_Viewer = new ViewerMessengerConfiguration("viewer", m_GibraltarNode);
                    }
                    else
                    {
                        ViewerElement configuration = null;
                        try
                        {
                            //see if we can get a configuration section
                            configuration = ConfigurationManager.GetSection("gibraltar/viewer") as ViewerElement;
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }

                        m_Viewer = (configuration == null) ? new ViewerMessengerConfiguration("viewer", new ViewerElement()) : new ViewerMessengerConfiguration("viewer", configuration);
                    }
                }

                return m_Viewer;
            }
        }

        /// <summary>
        /// Application defined properties
        /// </summary>
        public Dictionary<string, string> Properties
        {
            get
            {
                if (m_Properties == null)
                {
                    m_Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (m_GibraltarNode != null)
                    {
                        System.Diagnostics.Debug.Print("Property rehydration not yet supported!");
                    }
                    else
                    {
                        try
                        {
                            //see if we can get a configuration section
                            NameValueCollection properties = (NameValueCollection)ConfigurationManager.GetSection("gibraltar/properties");

                            if (properties != null)
                            {
                                for (int curPropertyIndex = 0; curPropertyIndex < properties.Count; curPropertyIndex++)
                                {
                                    try
                                    {
                                        m_Properties.Add(properties.GetKey(curPropertyIndex), properties[curPropertyIndex]);
                                    }
                                        // ReSharper disable EmptyGeneralCatchClause
                                    catch
                                        // ReSharper restore EmptyGeneralCatchClause
                                    {
                                        //most likely they had a dupe somehow - we're going to put in any that we can.
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
                            Log.DebugBreak();
                        }
                    }
                }

                return m_Properties;
            }
        }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //clear everything that's under this node...
            gibraltarNode.RemoveAll();

            //ask each property that exists to save itself.
            if (Email != null)
                Email.Save(gibraltarNode);

            if (Listener != null)
                Listener.Save(gibraltarNode);

            if (Packager != null)
                Packager.Save(gibraltarNode);

            if (Publisher != null)
                Publisher.Save(gibraltarNode);

            if (SessionFile != null)
                SessionFile.Save(gibraltarNode);

            if (ExportFile != null)
                ExportFile.Save(gibraltarNode);

            if (Viewer != null)
                Viewer.Save(gibraltarNode);

            if (Server != null)
                Server.Save(gibraltarNode);

            if (AutoSendConsent != null)
                AutoSendConsent.Save(gibraltarNode);

            if (NetworkViewer != null)
                NetworkViewer.Save(gibraltarNode);

            if (m_Properties != null)
                System.Diagnostics.Debug.Print("Saving Properties to XML not implemented yet!");               

        }

        #endregion

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            //we want to force everyone to load and sanitize so we know it's completed.
            Email.Sanitize();
            Listener.Sanitize();
            Packager.Sanitize();
            Publisher.Sanitize();
            SessionFile.Sanitize();
            ExportFile.Sanitize();
            Viewer.Sanitize();
            Server.Sanitize();
        }

        internal static T ReadValue<T>(XmlNode node, string name, T defaultValue)
        {
            T returnVal = defaultValue;

            XmlNode targetItem = node.Attributes.GetNamedItem(name);
            if (targetItem != null)
            {
                returnVal = (T)Convert.ChangeType(targetItem.Value, typeof(T));
            }

            return returnVal;
        }

        /// <summary>
        /// Add an attribute with the specified name to the provided node if different than the default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node to add the attribute to</param>
        /// <param name="name">The name of the attribute</param>
        /// <param name="value">The value to write out</param>
        /// <param name="defaultValue">The default value.  No attribute will be written if value = defaultValue</param>
        internal static void WriteValue<T>(XmlNode node, string name, T value, T defaultValue)
        {
            if (EqualityComparer<T>.Default.Equals(value, defaultValue))
                return;

            var isBool = (typeof(T) == typeof(bool));

            //add an attribute to the node with the provided name and set its value
            XmlAttribute newAttrib = node.OwnerDocument.CreateAttribute(name);
            newAttrib.Value = (value.Equals(null) 
                ? string.Empty 
                : isBool 
                    ? value.ToString().ToLowerInvariant()
                    : value.ToString());

            node.Attributes.Append(newAttrib);
        }

        #endregion

        #region Private Properties and Methods

        #endregion
    }
}
