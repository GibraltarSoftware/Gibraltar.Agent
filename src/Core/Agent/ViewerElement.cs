using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Windows;
using Loupe.Extensibility.Data;

namespace Gibraltar.Agent
{
    /// <summary>
    /// Configuration information for Gibraltar Trace Monitor.
    /// </summary>
    public class ViewerElement : LoupeElementBase
    {
        /// <inheritdoc />
        public ViewerElement() : base("LOUPE__VIEWER__")
        {

        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "enabled");
            LoadEnvironmentVariable(environmentVars, "hotKey");
            LoadEnvironmentVariable(environmentVars, "maxMessages");
            LoadEnvironmentVariable(environmentVars, "showVerboseMessages");
            LoadEnvironmentVariable(environmentVars, "formTitleText");
            LoadEnvironmentVariable(environmentVars, "defaultFilterLevel");
            LoadEnvironmentVariable(environmentVars, "enableIndependentSeverityFilters");
            LoadEnvironmentVariable(environmentVars, "showDetailsButton");
            LoadEnvironmentVariable(environmentVars, "showDetailsInGrid");
            LoadEnvironmentVariable(environmentVars, "showDetailsInTooltips");
            LoadEnvironmentVariable(environmentVars, "showMessageCounters");
            LoadEnvironmentVariable(environmentVars, "runButtonText");
            LoadEnvironmentVariable(environmentVars, "runButtonTextVisible");
            LoadEnvironmentVariable(environmentVars, "pauseButtonText");
            LoadEnvironmentVariable(environmentVars, "pauseButtonTextVisible");
            LoadEnvironmentVariable(environmentVars, "resetSearchButtonText");
            LoadEnvironmentVariable(environmentVars, "resetSearchButtonTextVisible");
            LoadEnvironmentVariable(environmentVars, "clearMessagesButtonText");
            LoadEnvironmentVariable(environmentVars, "clearMessagesButtonTextVisible");
            LoadEnvironmentVariable(environmentVars, "showToolBar");
            LoadEnvironmentVariable(environmentVars, "enableMultiSelection");
            LoadEnvironmentVariable(environmentVars, "forceSynchronous");
            LoadEnvironmentVariable(environmentVars, "maxQueueLength");
        }

        /// <summary>
        /// True by default, disables the live viewer when false.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled 
        { 
            get => ReadBoolean("enabled");
            set => this["enabled"] = value;
        }

        /// <summary>
        /// The default HotKey configuration string for the live viewer.
        /// </summary>
        public const string DefaultHotKey = "Ctrl-Alt-F5";

        /// <summary>
        /// The key sequence used to pop up the live viewer.
        /// </summary>
        [ConfigurationProperty("hotKey", DefaultValue = DefaultHotKey, IsRequired = false)]
        public string HotKey 
        { 
            get => ReadString("hotKey");
            set => this["hotKey"] = value;
        }

        /// <summary>
        /// Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.
        /// </summary>
        [ConfigurationProperty("maxMessages", DefaultValue = LiveLogViewer.DefaultMaxMessages, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 500000)]
        public int MaxMessages 
        { 
            get => ReadInt("maxMessages");
            set => this["maxMessages"] = value;
        }

        /// <summary>
        /// Suppresses the collection and display of verbose messages
        /// </summary>
        [ConfigurationProperty("showVerboseMessages", DefaultValue = true, IsRequired = false)]
        public bool ShowVerboseMessages 
        { 
            get => ReadBoolean("showVerboseMessages");
            set => this["showVerboseMessages"] = value;
        }

        /// <summary>
        /// Caption for the live viewer form.
        /// </summary>
        [ConfigurationProperty("formTitleText", DefaultValue = "Loupe Live Log", IsRequired = false)]
        public string FormTitleText 
        { 
            get => ReadString("formTitleText");
            set => this["formTitleText"] = value;
        }

        /// <summary>
        /// Specifies the default value for the filter.  If not set, no messages will be filtered.
        /// </summary>
        [ConfigurationProperty("defaultFilterLevel", DefaultValue = LogMessageSeverity.Verbose, IsRequired = false)]
        public LogMessageSeverity DefaultFilterLevel 
        { 
            get => (LogMessageSeverity)ReadInt("defaultFilterLevel");
            set => this["defaultFilterLevel"] = value;
        }

        /// <summary>
        /// Causes each of the message severity filter buttons to operate independently
        /// </summary>
        [ConfigurationProperty("enableIndependentSeverityFilters", DefaultValue = false, IsRequired = false)]
        public bool EnableIndependentSeverityFilters 
        { 
            get => ReadBoolean("enableIndependentSeverityFilters");
            set => this["enableIndependentSeverityFilters"] = value;
        }

        /// <summary>
        /// Specifies whether the Show Details button should be visible in the toolbar
        /// </summary>
        [ConfigurationProperty("showDetailsButton", DefaultValue = true, IsRequired = false)]
        public bool ShowDetailsButton 
        { 
            get => ReadBoolean("showDetailsButton");
            set => this["showDetailsButton"] = value;
        }

        /// <summary>
        /// Specifies whether the grid includes developer details about threads and calling method
        /// </summary>
        [ConfigurationProperty("showDetailsInGrid", DefaultValue = true, IsRequired = false)]
        public bool ShowDetailsInGrid 
        { 
            get => ReadBoolean("showDetailsInGrid");
            set => this["showDetailsInGrid"] = value;
        }

        /// <summary>
        /// Specifies whether tooltips include developer details about threads and calling method
        /// </summary>
        [ConfigurationProperty("showDetailsInTooltips", DefaultValue = false, IsRequired = false)]
        public bool ShowDetailsInTooltips 
        { 
            get => ReadBoolean("showDetailsInTooltips");
            set => this["showDetailsInTooltips"] = value;
        }

        /// <summary>
        /// Specifies whether the severity filter buttons should display message counts next to the icon
        /// </summary>
        [ConfigurationProperty("showMessageCounters", DefaultValue = true, IsRequired = false)]
        public bool ShowMessageCounters 
        { 
            get => ReadBoolean("showMessageCounters");
            set => this["showMessageCounters"] = value;
        }

        /// <summary>
        /// Caption text for Run button
        /// </summary>
        [ConfigurationProperty("runButtonText", DefaultValue = "Click to Auto Refresh", IsRequired = false)]
        public string RunButtonText 
        { 
            get => ReadString("runButtonText");
            set => this["runButtonText"] = value;
        }

        /// <summary>
        /// Specifies whether the Run button should display caption text next to the icon
        /// </summary>
        [ConfigurationProperty("runButtonTextVisible", DefaultValue = false, IsRequired = false)]
        public bool RunButtonTextVisible 
        { 
            get => ReadBoolean("runButtonTextVisible");
            set => this["runButtonTextVisible"] = value;
        }

        /// <summary>
        /// Caption text for Pause button
        /// </summary>
        [ConfigurationProperty("pauseButtonText", DefaultValue = "Pause", IsRequired = false)]
        public string PauseButtonText 
        { 
            get => ReadString("pauseButtonText");
            set => this["pauseButtonText"] = value;
        }

        /// <summary>
        /// Specifies whether the Pause button should display caption text next to the icon
        /// </summary>
        [ConfigurationProperty("pauseButtonTextVisible", DefaultValue = false, IsRequired = false)]
        public bool PauseButtonTextVisible 
        { 
            get => ReadBoolean("pauseButtonTextVisible");
            set => this["pauseButtonTextVisible"] = value;
        }

        /// <summary>
        /// Caption text for Reset Search button
        /// </summary>
        [ConfigurationProperty("resetSearchButtonText", DefaultValue = "Reset", IsRequired = false)]
        public string ResetSearchButtonText 
        { 
            get => ReadString("resetSearchButtonText");
            set => this["resetSearchButtonText"] = value;
        }

        /// <summary>
        /// Specifies whether the Reset Search button should display caption text next to the icon
        /// </summary>
        [ConfigurationProperty("resetSearchButtonTextVisible", DefaultValue = false, IsRequired = false)]
        public bool ResetSearchButtonTextVisible 
        { 
            get => ReadBoolean("resetSearchButtonTextVisible");
            set => this["resetSearchButtonTextVisible"] = value;
        }

        /// <summary>
        /// Caption text for Clear Messages button
        /// </summary>
        [ConfigurationProperty("clearMessagesButtonText", DefaultValue = "Clear", IsRequired = false)]
        public string ClearMessagesButtonText 
        { 
            get => ReadString("clearMessagesButtonText");
            set => this["clearMessagesButtonText"] = value;
        }

        /// <summary>
        /// Specifies whether the Clear Messages button should display caption text next to the icon
        /// </summary>
        [ConfigurationProperty("clearMessagesButtonTextVisible", DefaultValue = false, IsRequired = false)]
        public bool ClearMessagesButtonTextVisible 
        { 
            get => ReadBoolean("clearMessagesButtonTextVisible");
            set => this["clearMessagesButtonTextVisible"] = value;
        }

        /// <summary>
        /// Shows or hides the built-in toolbar
        /// </summary>
        [ConfigurationProperty("showToolBar", DefaultValue = true, IsRequired = false)]
        public bool ShowToolBar 
        { 
            get => ReadBoolean("showToolBar");
            set => this["showToolBar"] = value;
        }

        /// <summary>
        /// Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy.
        /// </summary>
        [ConfigurationProperty("enableMultiSelection", DefaultValue = true, IsRequired = false)]
        public bool EnableMultiSelection 
        { 
            get => ReadBoolean("enableMultiSelection");
            set => this["enableMultiSelection"] = value;
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
            get => ReadBoolean("forceSynchronous");
            set => this["forceSynchronous"] = value;
        }

        /// <summary>
        /// The maximum number of queued messages waiting to be dispatched to viewers.
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be written exceeds the
        /// maximum queue length the log writer will switch to a synchronous mode to 
        /// catch up.  This will not cause the client to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        [ConfigurationProperty("maxQueueLength", DefaultValue = 2000, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 50000)]
        public int MaxQueueLength 
        { 
            get => ReadInt("maxQueueLength");
            set => this["maxQueueLength"] = value;
        }
    }
}
