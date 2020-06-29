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
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Monitor;

namespace Gibraltar.Messaging.Export
{
    /// <summary>
    /// File Messenger Configuration
    /// </summary>
    public class ExportFileMessengerConfiguration : MessengerConfiguration
    {
        private XmlNode m_GibraltarNode;

        /// <summary>
        /// Initialize the file messenger from the application configuration
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        /// <param name="server"></param>
        internal ExportFileMessengerConfiguration(string name, ExportFileElement configuration, ServerConfiguration server)
            : base(name, typeof(CsvFileMessenger).AssemblyQualifiedName)
        {
            Server = server;

            Initialize(configuration);
        }

        /// <summary>
        /// Initialize the file messenger from an XML document
        /// </summary>
        internal ExportFileMessengerConfiguration(string name, XmlNode gibraltarNode, ServerConfiguration server)
            : base(name, typeof(CsvFileMessenger).AssemblyQualifiedName)
        {
            m_GibraltarNode = gibraltarNode;

            Server = server;

            //create an element object so we have ExportFileElement to draw defaults from.
            ExportFileElement baseline = new ExportFileElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("exportFile");

            //copy the provided configuration
            if (node != null)
            {
                Folder = AgentConfiguration.ReadValue(node, "folder", baseline.Folder);
                AutoFlushInterval = AgentConfiguration.ReadValue(node, "autoFlushInterval", baseline.AutoFlushInterval);
                MaxFileSize = AgentConfiguration.ReadValue(node, "maxFileSize", baseline.MaxFileSize);
                EnableFilePruning = AgentConfiguration.ReadValue(node, "enableFilePruning", baseline.EnableFilePruning);
                MaxLocalDiskUsage = AgentConfiguration.ReadValue(node, "maxLocalDiskUsage", baseline.MaxLocalDiskUsage);
                MaxLocalFileAge = AgentConfiguration.ReadValue(node, "maxLocalFileAge", baseline.MaxLocalFileAge);
                MinimumFreeDisk = AgentConfiguration.ReadValue(node, "minimumFreeDisk", baseline.MinimumFreeDisk);
                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                ForceSynchronous = AgentConfiguration.ReadValue(node, "forceSynchronous", baseline.ForceSynchronous);
                MaxQueueLength = AgentConfiguration.ReadValue(node, "maxQueueLength", baseline.MaxQueueLength);
            }

            Sanitize();
        }


        /// <summary>
        /// The maximum number of seconds data can be held before it is flushed.
        /// </summary>
        /// <remarks>In addition to the default automatic flush due to the amount of information waiting to be written out 
        /// the messenger will automatically flush to disk based on the number of seconds specified.</remarks>
        public int AutoFlushInterval { get; set; }

        /// <summary>
        /// The folder to store session files in unless explicitly overridden at runtime.
        /// </summary>
        /// <remarks>If null or empty, files will be stored in a central local application data folder which is the preferred setting.</remarks>
        public string Folder { get; set; }

        /// <summary>
        /// The maximum number of megabytes in a single session file before a new file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum size it will be closed and a new file started. 
        /// Due to compression effects and other data storage considerations, final files may end up slightly 
        /// larger on disk or somewhat smaller.  Setting to zero will allow files to grow to the maximum
        /// size allowed by the file format (2 GB)</remarks>
        public int MaxFileSize { get; set; }

        /// <summary>
        /// The maximum number of minutes in a single session file before a new file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum age it will be closed and a new file started.  Setting to zero
        /// will allow the file to cover an unlimited period of time.</remarks>
        public int MaxFileDuration { get; set; }

        /// <summary>
        /// When true, session files will be pruned for size or age.
        /// </summary>
        /// <remarks>By default session files older than a specified number of days are automatically
        /// deleted and the oldest files are removed when the total storage of all files for the same application
        /// exceeds a certain value.  Setting this option to false will disable pruning.</remarks>
        public bool EnableFilePruning { get; set; }

        /// <summary>
        /// The maximum number of megabytes for all session files in megabytes on the local drive before older files are purged.
        /// </summary>
        /// <remarks>When the maximum local disk usage is approached, files are purged by selecting the oldest files first.
        /// This limit may be exceeded temporarily by the maximum size because the active file will not be purged.
        /// Size is specified in megabytes.</remarks>
        public int MaxLocalDiskUsage { get; set; }

        /// <summary>
        /// The maximum age in days of a session file before it should be purged.
        /// </summary>
        /// <remarks>Any session file fragment that was closed longer than this number of days in the past will be
        /// automatically purged.  Any value less than 1 will disable age pruning.</remarks>
        public int MaxLocalFileAge { get; set; }

        /// <summary>
        /// The minimum amount of free disk space for logging.
        /// </summary>
        /// <remarks>If the amount of free disk space falls below this value, existing log files will be removed to free space.
        /// If no more log files are available, logging will stop until adequate space is freed.</remarks>
        public int MinimumFreeDisk { get; set; }


        /// <summary>
        /// The server configuration (necessary for the SDS publishing from the File Messenger)
        /// </summary>
        internal ServerConfiguration Server { get; private set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            SessionFileElement baseline = new SessionFileElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("exportFile");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "forceSynchronous", ForceSynchronous, baseline.ForceSynchronous);
            AgentConfiguration.WriteValue(newNode, "maxQueueLength", MaxQueueLength, baseline.MaxQueueLength);

            AgentConfiguration.WriteValue(newNode, "folder", Folder ?? string.Empty, baseline.Folder);
            AgentConfiguration.WriteValue(newNode, "enableFilePruning", EnableFilePruning, baseline.EnableFilePruning);
            AgentConfiguration.WriteValue(newNode, "maxFileSize", MaxFileSize, baseline.MaxFileSize);
            AgentConfiguration.WriteValue(newNode, "maxLocalDiskUsage", MaxLocalDiskUsage, baseline.MaxLocalDiskUsage);
            AgentConfiguration.WriteValue(newNode, "maxLocalFileAge", MaxLocalFileAge, baseline.MaxLocalFileAge);
            AgentConfiguration.WriteValue(newNode, "minimumFreeDisk", MinimumFreeDisk, baseline.MinimumFreeDisk);
            AgentConfiguration.WriteValue(newNode, "autoFlushInterval", AutoFlushInterval, baseline.AutoFlushInterval);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (string.IsNullOrEmpty(Folder))
            {
                Folder = null;
                Enabled = false; //if there is no folder then we can't be enabled.
            }

            if (AutoFlushInterval <= 0)
                AutoFlushInterval = 15;

            if (MaxFileDuration < 1)
                MaxFileDuration = 1576800; //three years, treated as infinite because really - is a process going to run longer than that?

            if (MaxFileSize <= 0)
                MaxFileSize = 1024; //1GB override when set to zero.

            if (MaxLocalDiskUsage <= 0)
            {
                MaxLocalDiskUsage = 0; //we intelligently disable at this point
            }
            else
            {
                //make sure our max file size can fit within our max local disk usage
                if (MaxLocalDiskUsage < MaxFileSize)
                    MaxFileSize = MaxLocalDiskUsage;
            }

            if (MaxLocalFileAge <= 0)
                MaxLocalFileAge = 0; //we intelligently disable at this point

            if (MinimumFreeDisk <= 0)
                MinimumFreeDisk = 50;

            if (MaxQueueLength <= 0)
                MaxQueueLength = 2000;
            else if (MaxQueueLength > 50000)
                MaxQueueLength = 50000;
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(ExportFileElement configuration)
        {
            //copy the configuration from the log element
            Folder = configuration.Folder;
            AutoFlushInterval = configuration.AutoFlushInterval;
            MaxFileSize = configuration.MaxFileSize;
            MaxFileDuration = configuration.MaxFileDuration;
            EnableFilePruning = configuration.EnableFilePruning;
            MaxLocalDiskUsage = configuration.MaxLocalDiskUsage;
            MaxLocalFileAge = configuration.MaxLocalFileAge;
            MinimumFreeDisk = configuration.MinimumFreeDisk;

            //and set our base stuff
            Enabled = configuration.Enabled;
            ForceSynchronous = configuration.ForceSynchronous;
            MaxQueueLength = configuration.MaxQueueLength;
        }

        #endregion
    }
}
