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
    /// The application configuration information for storing session data to a file.
    /// </summary>
    public class ExportFileElement : ConfigurationSection
    {
        /// <summary>
        /// True by default, disables storing a session file when false.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// The folder to store log files in unless explicitly overridden at runtime.
        /// </summary>
        /// <remarks>If null or empty the export file will be disabled.</remarks>
        [ConfigurationProperty("folder", DefaultValue = "", IsRequired = false)]
        public string Folder
        {
            get
            {
                return this["folder"].ToString();
            }
            set
            {
                this["folder"] = value;
            }
        }


        /// <summary>
        /// The maximum number of seconds data can be held before it is flushed.
        /// </summary>
        /// <remarks>In addition to the default automatic flush due to the amount of information waiting to be written out 
        /// the messenger will automatically flush to disk based on the number of seconds specified.</remarks>
        [ConfigurationProperty("autoFlushInterval", DefaultValue = 15, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 2147483647)]
        public int AutoFlushInterval
        {
            get
            {
                return (int)this["autoFlushInterval"];
            }
            set
            {
                this["autoFlushInterval"] = value;
            }
        }

        /// <summary>
        /// The maximum number of megabytes in a single log file before a new log file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum size it will be closed and a new file started. 
        /// Due to compression effects and other data storage considerations, final files may end up slightly 
        /// larger on disk or somewhat smaller.  Setting to zero will allow log files to grow to the maximum
        /// size allowed by the file format (2 GB)</remarks>
        [ConfigurationProperty("maxFileSize", DefaultValue = 10, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 2147483647)]
        public int MaxFileSize
        {
            get
            {
                return (int)this["maxFileSize"];
            }
            set
            {
                this["maxFileSize"] = value;
            }
        }


        /// <summary>
        /// The maximum number of minutes in a single log file before a new log file is started.
        /// </summary>
        /// <remarks>When the file reaches the maximum age it will be closed and a new file started.  Setting to zero
        /// will allow the log file to cover an unlimited period of time.</remarks>
        [ConfigurationProperty("maxFileDuration", DefaultValue = 60, IsRequired = false)]
        public int MaxFileDuration
        {
            get
            {
                return (int)this["maxFileDuration"];
            }
            set
            {
                this["maxFileDuration"] = value;
            }
        }


        /// <summary>
        /// When true, session files will be pruned for size or age.
        /// </summary>
        /// <remarks>By default log files older than a specified number of days are automatically
        /// deleted and the oldest log files are removed when the total storage of all log files
        /// exceeds a certain value.  Setting this option to true will disable pruning.</remarks>
        [ConfigurationProperty("enableFilePruning", DefaultValue = true, IsRequired = false)]
        public bool EnableFilePruning
        {
            get
            {
                return (bool)this["enableFilePruning"];
            }
            set
            {
                this["enableFilePruning"] = value;
            }
        }

        /// <summary>
        /// The maximum number of megabytes for all log files in megabytes on the local drive before older files are purged.
        /// </summary>
        /// <remarks><para>When the maximum local disk usage is approached, files are purged by selecting the oldest files first.
        /// This limit may be exceeded temporarily by the maximum log size because the active file will not be purged.
        /// Size is specified in megabytes.</para>
        /// <para>Setting to any integer less than 1 will disable pruning by disk usage.</para></remarks>
        [ConfigurationProperty("maxLocalDiskUsage", DefaultValue = 150, IsRequired = false)]
        public int MaxLocalDiskUsage
        {
            get
            {
                return (int)this["maxLocalDiskUsage"];
            }
            set
            {
                this["maxLocalDiskUsage"] = value;
            }
        }


        /// <summary>
        /// The number of days that log files are retained.
        /// </summary>
        /// <remarks>
        ///   <para>Log files that were collected longer than the retention interval ago will be removed regardless of space constraints.</para>
        ///   <para>Setting to any integer less than 1 will disable pruning by age.</para>
        /// </remarks>
        [ConfigurationProperty("maxLocalFileAge", DefaultValue = 90, IsRequired = false)]
        public int MaxLocalFileAge
        {
            get
            {
                return (int)this["maxLocalFileAge"];
            }
            set
            {
                this["maxLocalFileAge"] = value;
            }
        }

        /// <summary>
        /// The minimum amount of free disk space for logging.
        /// </summary>
        /// <remarks>If the amount of free disk space falls below this value, existing log files will be removed to free space.
        /// If no more log files are available, logging will stop until adequate space is freed.</remarks>
        [ConfigurationProperty("minimumFreeDisk", DefaultValue = 200, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 2147483647)]
        public int MinimumFreeDisk
        {
            get
            {
                return (int)this["minimumFreeDisk"];
            }
            set
            {
                this["minimumFreeDisk"] = value;
            }
        }

        /// <summary>
        /// When true, the messenger will treat all write requests as write-through requests.
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
        /// The maximum number of queued messages waiting to be written to disk
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be written exceeds the
        /// maximum queue length the log writer will switch to a synchronous mode to 
        /// catch up.  This will not cause the client to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
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
    }
}
