using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Gibraltar.Agent
{
    /// <summary>
    /// Configuration information for the trace listener.
    /// </summary>
    public class ListenerElement : LoupeElementBase
    {
        /// <inheritdoc />
        public ListenerElement() : base("LOUPE__LISTENER__")
        {

        }

        /// <inheritdoc />
        protected override void OnLoadEnvironmentVars(IDictionary<string, string> environmentVars)
        {
            LoadEnvironmentVariable(environmentVars, "autoTraceRegistration");
            LoadEnvironmentVariable(environmentVars, "enableCommandLine");
            LoadEnvironmentVariable(environmentVars, "enableConsole");
            LoadEnvironmentVariable(environmentVars, "enableProcessPerformance");
            LoadEnvironmentVariable(environmentVars, "enableDiskPerformance");
            LoadEnvironmentVariable(environmentVars, "enableMemoryPerformance");
            LoadEnvironmentVariable(environmentVars, "enableNetworkPerformance");
            LoadEnvironmentVariable(environmentVars, "enableSystemPerformance");
            LoadEnvironmentVariable(environmentVars, "enableNetworkEvents");
            LoadEnvironmentVariable(environmentVars, "enablePowerEvents");
            LoadEnvironmentVariable(environmentVars, "enableUserEvents");
            LoadEnvironmentVariable(environmentVars, "enableAssemblyEvents");
            LoadEnvironmentVariable(environmentVars, "enableAssemblyLoadFailureEvents");
            LoadEnvironmentVariable(environmentVars, "catchUnhandledExceptions");
            LoadEnvironmentVariable(environmentVars, "catchApplicationExceptions");
            LoadEnvironmentVariable(environmentVars, "reportErrorsToUser");
            LoadEnvironmentVariable(environmentVars, "endSessionOnTraceClose");
        }

        /// <summary>
        /// Configures whether Gibraltar should automatically make sure it is registered as a Trace Listener.
        /// </summary>
        /// <remarks>This is true by default to enable easy drop-in configuration (eg. using the LiveLogViewer
        /// control on a form).  Normally, it should not be necessary to disable this feature even when adding
        /// Gibraltar as a Trace Listener in an app.config or by code.  But this setting can be configured
        /// to false if it is desirable to prevent Gibraltar from receiving Trace events directly, such as
        /// if the application is processing Trace events into the Gibraltar API itself.</remarks>
        [ConfigurationProperty("autoTraceRegistration", DefaultValue = true, IsRequired = false)]
        public bool AutoTraceRegistration
        {
            get => ReadBoolean("autoTraceRegistration");
            set => this["autoTraceRegistration"] = value;
        }

        /// <summary>
        /// When true, the Loupe session summary will include the exact command line and arguments used to start the process.
        /// </summary>
        /// <remarks>
        /// <para>Command line arguments can reveal sensitive information in some cases so these can be suppressed individually without
        /// requiring anonymous mode be enabled.</para>
        /// <para>If false, the session summary will not container the command line or arguments.</para>
        /// <para>Defaults to True.</para></remarks>
        [ConfigurationProperty("enableCommandLine", DefaultValue = true, IsRequired = false)]
        public bool EnableCommandLine
        {
            get => ReadBoolean("enableCommandLine");
            set => this["enableCommandLine"] = value;
        }


        /// <summary>
        /// When true, anything written to the console out will be appended to the log.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.</remarks>
        [ConfigurationProperty("enableConsole", DefaultValue = true, IsRequired = false)]
        public bool EnableConsole
        {
            get => ReadBoolean("enableConsole");
            set => this["enableConsole"] = value;
        }

        /// <summary>
        /// When true, process performance information will be automatically captured for the current process
        /// </summary>
        [ConfigurationProperty("enableProcessPerformance", DefaultValue = true, IsRequired = false)]
        public bool EnableProcessPerformance
        {
            get => ReadBoolean("enableProcessPerformance");
            set => this["enableProcessPerformance"] = value;
        }

        /// <summary>
        /// When true, disk performance information will be automatically captured
        /// </summary>
        [ConfigurationProperty("enableDiskPerformance", DefaultValue = true, IsRequired = false)]
        public bool EnableDiskPerformance
        {
            get => ReadBoolean("enableDiskPerformance");
            set => this["enableDiskPerformance"] = value;
        }

        /// <summary>
        /// When true, extended .NET memory utilization information will be automatically captured
        /// </summary>
        [ConfigurationProperty("enableMemoryPerformance", DefaultValue = false, IsRequired = false)]
        public bool EnableMemoryPerformance
        {
            get => ReadBoolean("enableMemoryPerformance");
            set => this["enableMemoryPerformance"] = value;
        }

        /// <summary>
        /// When true, network performance information will be automatically captured
        /// </summary>
        [ConfigurationProperty("enableNetworkPerformance", DefaultValue = true, IsRequired = false)]
        public bool EnableNetworkPerformance
        {
            get => ReadBoolean("enableNetworkPerformance");
            set => this["enableNetworkPerformance"] = value;
        }

        /// <summary>
        /// When true, system performance information will be automatically captured
        /// </summary>
        [ConfigurationProperty("enableSystemPerformance", DefaultValue = true, IsRequired = false)]
        public bool EnableSystemPerformance
        {
            get => ReadBoolean("enableSystemPerformance");
            set => this["enableSystemPerformance"] = value;
        }

        /// <summary>
        /// When true, network events (such as reconfiguration and disconnection) will be logged automatically.
        /// </summary>
        [ConfigurationProperty("enableNetworkEvents", DefaultValue = true, IsRequired = false)]
        public bool EnableNetworkEvents
        {
            get => ReadBoolean("enableNetworkEvents");
            set => this["enableNetworkEvents"] = value;
        }

        /// <summary>
        /// When true, power events (such as going on or coming off battery) will be logged automatically.
        /// </summary>
        [ConfigurationProperty("enablePowerEvents", DefaultValue = true, IsRequired = false)]
        public bool EnablePowerEvents
        {
            get => ReadBoolean("enablePowerEvents");
            set => this["enablePowerEvents"] = value;
        }

        /// <summary>
        /// When true, user events (such as changing display settings and switching sessions) will be logged automatically.
        /// </summary>
        [ConfigurationProperty("enableUserEvents", DefaultValue = true, IsRequired = false)]
        public bool EnableUserEvents
        {
            get => ReadBoolean("enableUserEvents");
            set => this["enableUserEvents"] = value;
        }

        /// <summary>
        /// When true, assembly load information will be logged automatically.
        /// </summary>
        [ConfigurationProperty("enableAssemblyEvents", DefaultValue = true, IsRequired = false)]
        public bool EnableAssemblyEvents
        {
            get => ReadBoolean("enableAssemblyEvents");
            set => this["enableAssemblyEvents"] = value;
        }

        /// <summary>
        /// When true, CLR events related to assembly resolution failures will be logged automatically.
        /// </summary>
        [ConfigurationProperty("enableAssemblyLoadFailureEvents", DefaultValue = false, IsRequired = false)]
        public bool EnableAssemblyLoadFailureEvents
        {
            get => ReadBoolean("enableAssemblyLoadFailureEvents");
            set => this["enableAssemblyLoadFailureEvents"] = value;
        }

        /// <summary>
        /// When true, any unhandled exceptions (which are always fatal) will be recorded by the Agent before the application exits.
        /// </summary>
        /// <remarks><para>If disabled (false), the Agent will not listen for
        /// <see CREF="AppDomain.UnhandledException">UnhandledException</see> events.  Your own
        /// <see CREF="AppDomain.UnhandledExceptionEventHandler">UnhandledExceptionEventHandler</see> may still send
        /// exceptions to the Agent explicitly by calling <see CREF="Gibraltar.Agent.Log.RecordException">Log.RecordException</see>
        /// or <see CREF="Gibraltar.Agent.Log.ReportException">Log.ReportException</see>.</para>
        /// <para>If enabled, the Agent's handler may be called before your own UnhandledException handler or after it
        /// depending on the timing of registering for the event.  If ReportErrorsToUser is also enabled, the Agent's event
        /// handler will block, preventing further event handlers from being called, until the alerting dialog window is
        /// closed by the user.</para></remarks>
        [ConfigurationProperty("catchUnhandledExceptions", DefaultValue = true, IsRequired = false)]
        public bool CatchUnhandledExceptions
        {
            get => ReadBoolean("catchUnhandledExceptions");
            set => this["catchUnhandledExceptions"] = value;
        }

        /// <summary>
        /// When true, uncaught exceptions in a Windows Forms application thread will be handled by the Agent.
        /// </summary>
        /// <remarks><para>In a Windows Forms application, the <see CREF="System.Windows.Forms.Application.Run">Application.Run</see>
        /// message loop can be configured to <see CREF="System.Windows.Forms.UnhandledExceptionMode.CatchException">CatchException</see>
        /// and issue them to the <see CREF="System.Windows.Forms.Application.ThreadException">Application.ThreadException</see>
        /// event handler, rather than to <see CREF="System.Windows.Forms.UnhandledExceptionMode.ThrowException">ThrowException</see>
        /// on up to become fatal <see CREF="AppDomain.UnhandledException">UnhandledException</see> events.</para>
        /// <para>If the Gibraltar Agent is configured to catch application exceptions (the default setting), the Agent will
        /// attempt to set CatchException as the setting for all application UI threads which are left in the default
        /// <see CREF="System.Windows.Forms.UnhandledExceptionMode.Automatic">Automatic</see> mode.  This can only work if
        /// the Agent is able to do so before any Forms are created, so it is recommended that a log message be issued through
        /// <see CREF="Gibraltar.Agent.Log.TraceInformation(string, object[])">Log.TraceInformation</see> (if Gibraltar Agent
        /// is linked to your application statically by reference to Gibraltar.Agent.dll) or through
        /// <see CREF="System.Diagnostics.Trace.TraceInformation(string, object[])">Trace.TraceInformation</see> (if
        /// Gibraltar Agent is linked to your application dynamically by adding Gibraltar as a TraceListener in your
        /// app.config) early in your Program.Main method to cause Gibraltar Agent to initialize itself at that point.</para>
        /// <para>If the Agent is configured not to catch application exceptions (or if it is not a Windows application)
        /// then it will not alter the default UnhandledExceptionMode and the Agent will not listen for
        /// <see CREF="System.Windows.Forms.Application.ThreadException">Application.ThreadException</see> events.  Unlike
        /// most events in .NET, these events are only sent to a single subscriber.  The most recent handler registered on
        /// that UI thread overwrites any previous handler, so even if the Agent is configured to catch application
        /// exceptions the Agent's handler can be overridden by registering your own handler for the event after initializing
        /// Gibraltar and before creating your application's first Form.</para></remarks>
        [ConfigurationProperty("catchApplicationExceptions", DefaultValue = true, IsRequired = false)]
        public bool CatchApplicationExceptions
        {
            get => ReadBoolean("catchApplicationExceptions");
            set => this["catchApplicationExceptions"] = value;
        }

        /// <summary>
        /// When true, exceptions handled automatically by the Agent will be reported to the user in an alert dialog (in Windows applications only).
        /// </summary>
        /// <remarks><para>This setting is enabled by default for Windows applications and provides the Agent's error-alert
        /// dialog to notify the user and present options.  It is ignored in applications where opening a form is not
        /// possible, such as console, service, or web applications.  The error-alert dialog can also be accessed by calling
        /// <see CREF="Gibraltar.Agent.Log.ReportException">Log.ReportException</see> to explicitly report exceptions
        /// caught by your own code.</para>
        /// <para>Explicit calls to ReportException will report those errors to the user (in Windows applications)
        /// even if this setting is configured to false.  This setting does not affect explicit calls to
        /// <see CREF="Gibraltar.Agent.Log.RecordException">Log.RecordException</see>, which will only record the
        /// exception to the session log without reporting it to the user.</para></remarks>
        [ConfigurationProperty("reportErrorsToUser", DefaultValue = true, IsRequired = false)]
        public bool ReportErrorsToUser
        {
            get => ReadBoolean("reportErrorsToUser");
            set => this["reportErrorsToUser"] = value;
        }

        /// <summary>
        /// When true, the Gibraltar LogListener will end the Gibraltar log session when Trace.Close() is called.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.  Unless disabled by setting
        /// this configuration value to false, a call to Trace.Close() to shutdown Trace logging will also be
        /// translated into a call to Gibraltar.Agent.Log.EndSession().</remarks>
        [ConfigurationProperty("endSessionOnTraceClose", DefaultValue = true, IsRequired = false)]
        public bool EndSessionOnTraceClose
        {
            get => ReadBoolean("endSessionOnTraceClose");
            set => this["endSessionOnTraceClose"] = value;
        }

    }
}
