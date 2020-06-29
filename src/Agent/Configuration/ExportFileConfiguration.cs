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
    /// File Messenger Configuration
    /// </summary>
    public sealed class ExportFileConfiguration
    {
        private readonly Messaging.Export.ExportFileMessengerConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize the session file configuration from the application configuration
        /// </summary>
        /// <param name="configuration"></param>
        internal ExportFileConfiguration(Messaging.Export.ExportFileMessengerConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        /// <summary>
        /// The maximum number of seconds data can be held before it is flushed.
        /// </summary>
        /// <remarks>In addition to the default automatic flush due to the amount of information waiting to be written out 
        /// the messenger will automatically flush to disk based on the number of seconds specified.</remarks>
        public int AutoFlushInterval
        {
            get => m_WrappedConfiguration.AutoFlushInterval;
            set => m_WrappedConfiguration.AutoFlushInterval = value;
        }

        /// <summary>
        /// The folder to store session files in unless explicitly overridden at runtime.
        /// </summary>
        /// <remarks>If null or empty the export file will be disabled.</remarks>
        public string Folder
        {
            get => m_WrappedConfiguration.Folder;
            set => m_WrappedConfiguration.Folder = value;
        }

        /// <summary>
        /// The maximum number of megabytes in a single session file before a new file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum size it will be closed and a new file started. 
        /// Due to compression effects and other data storage considerations, final files may end up slightly 
        /// larger on disk or somewhat smaller.  Setting to zero will allow files to grow to the maximum
        /// size allowed by the file format (2 GB)</remarks>
        public int MaxFileSize
        {
            get => m_WrappedConfiguration.MaxFileSize;
            set => m_WrappedConfiguration.MaxFileSize = value;
        }

        /// <summary>
        /// The maximum number of minutes in a single session file before a new file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum age it will be closed and a new file started.  Setting to zero
        /// will allow the file to cover an unlimited period of time.</remarks>
        public int MaxFileDuration
        {
            get => m_WrappedConfiguration.MaxFileDuration;
            set => m_WrappedConfiguration.MaxFileDuration = value;
        }

        /// <summary>
        /// When true, session files will be pruned for size or age.
        /// </summary>
        /// <remarks>By default session files older than a specified number of days are automatically
        /// deleted and the oldest files are removed when the total storage of all files for the same application
        /// exceeds a certain value.  Setting this option to false will disable pruning.</remarks>
        public bool EnableFilePruning
        {
            get => m_WrappedConfiguration.EnableFilePruning;
            set => m_WrappedConfiguration.EnableFilePruning = value;
        }

        /// <summary>
        /// The maximum number of megabytes for all log files in megabytes on the local drive before older files are purged.
        /// </summary>
        /// <remarks><para>When the maximum local disk usage is approached, files are purged by selecting the oldest files first.
        /// This limit may be exceeded temporarily by the maximum log size because the active file will not be purged.
        /// Size is specified in megabytes.</para>
        /// <para>Setting to any integer less than 1 will disable pruning by disk usage.</para></remarks>
        public int MaxLocalDiskUsage
        {
            get => m_WrappedConfiguration.MaxLocalDiskUsage;
            set => m_WrappedConfiguration.MaxLocalDiskUsage = value;
        }

        /// <summary>
        /// The number of days that log files are retained.
        /// </summary>
        /// <remarks>
        ///   <para>Log files that were collected longer than the retention interval ago will be removed regardless of space constraints.</para>
        ///   <para>Setting to any integer less than 1 will disable pruning by age.</para>
        /// </remarks>
        public int MaxLocalFileAge
        {
            get => m_WrappedConfiguration.MaxLocalFileAge;
            set => m_WrappedConfiguration.MaxLocalFileAge = value;
        }

        /// <summary>
        /// The minimum amount of free disk space for logging.
        /// </summary>
        /// <remarks>If the amount of free disk space falls below this value, existing log files will be removed to free space.
        /// If no more log files are available, logging will stop until adequate space is freed.</remarks>
        public int MinimumFreeDisk
        {
            get => m_WrappedConfiguration.MinimumFreeDisk;
            set => m_WrappedConfiguration.MinimumFreeDisk = value;
        }

        /// <summary>
        /// When true, the session file will treat all write requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed.</remarks>
        public bool ForceSynchronous
        {
            get => m_WrappedConfiguration.ForceSynchronous;
            set => m_WrappedConfiguration.ForceSynchronous = value;
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be processed by the session file
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be processed exceeds the
        /// maximum queue length the session file will switch to a synchronous mode to 
        /// catch up.  This will not cause the application to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        public int MaxQueueLength
        {
            get => m_WrappedConfiguration.MaxQueueLength;
            set => m_WrappedConfiguration.MaxQueueLength = value;
        }

        /// <summary>
        /// When false, the session file is disabled even if otherwise configured.
        /// </summary>
        /// <remarks>This allows for explicit disable/enable without removing the existing configuration
        /// or worrying about the default configuration.</remarks>
        public bool Enabled
        {
            get => m_WrappedConfiguration.Enabled;
            set => m_WrappedConfiguration.Enabled = value;
        }
    }
}
