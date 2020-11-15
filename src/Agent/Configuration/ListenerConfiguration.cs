

namespace Gibraltar.Agent.Configuration
{
    /// <summary>
    /// Configuration information for the trace listener.
    /// </summary>
    public sealed class ListenerConfiguration
    {
        private readonly Monitor.ListenerConfiguration m_WrappedConfiguration;

        /// <summary>
        /// Initialize from application configuration section
        /// </summary>
        /// <param name="configuration"></param>
        internal ListenerConfiguration(Monitor.ListenerConfiguration configuration)
        {
            m_WrappedConfiguration = configuration;
        }

        /// <summary>
        /// Configures whether Loupe should automatically make sure it is registered as a Trace Listener.
        /// </summary>
        /// <remarks>This is true by default to enable easy drop-in configuration (e.g. using the LiveLogViewer
        /// control on a form).  Normally, it should not be necessary to disable this feature even when adding
        /// Loupe as a Trace Listener in an app.config or by code.  But this setting can be configured
        /// to false if it is desirable to prevent Loupe from receiving Trace events directly, such as
        /// if the application is processing Trace events into the Loupe API itself.</remarks>
        public bool AutoTraceRegistration
        {
            get { return m_WrappedConfiguration.AutoTraceRegistration; }
            set { m_WrappedConfiguration.AutoTraceRegistration = value; }
        }

        /// <summary>
        /// When true, anything written to the console out will be appended to the log.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.</remarks>
        public bool EnableConsole
        {
            get { return m_WrappedConfiguration.EnableConsole; }
            set { m_WrappedConfiguration.EnableConsole = value; }
        }

        /// <summary>
        /// When true, process performance information will be automatically captured for the current process
        /// </summary>
        /// <remarks>This includes basic information on processor and memory utilization for the running process.</remarks>
        public bool EnableProcessPerformance
        {
            get { return m_WrappedConfiguration.EnableProcessPerformance; }
            set { m_WrappedConfiguration.EnableProcessPerformance = value; }
        }

        /// <summary>
        /// When true, disk performance information will be automatically captured
        /// </summary>
        public bool EnableDiskPerformance
        {
            get { return m_WrappedConfiguration.EnableDiskPerformance; }
            set { m_WrappedConfiguration.EnableDiskPerformance = value; }
        }

        /// <summary>
        /// When true, extended .NET memory utilization information will be automatically captured
        /// </summary>
        /// <remarks>The extended information is primarily useful for narrowing down memory leaks.  Basic 
        /// memory utilization information (sufficient to identify if a leak is likely) is captured 
        /// as part of the EnableProcessPerformance option.</remarks>
        public bool EnableMemoryPerformance
        {
            get { return m_WrappedConfiguration.EnableMemoryPerformance; }
            set { m_WrappedConfiguration.EnableMemoryPerformance = value; }
        }

        /// <summary>
        /// When true, network performance information will be automatically captured
        /// </summary>
        public bool EnableNetworkPerformance
        {
            get { return m_WrappedConfiguration.EnableNetworkPerformance; }
            set { m_WrappedConfiguration.EnableNetworkPerformance = value; }
        }

        /// <summary>
        /// When true, system performance information will be automatically captured
        /// </summary>
        public bool EnableSystemPerformance
        {
            get { return m_WrappedConfiguration.EnableSystemPerformance; }
            set { m_WrappedConfiguration.EnableSystemPerformance = value; }
        }

        /// <summary>
        /// When true, network events (such as reconfiguration and disconnection) will be logged automatically.
        /// </summary>
        public bool EnableNetworkEvents
        {
            get { return m_WrappedConfiguration.EnableNetworkEvents; }
            set { m_WrappedConfiguration.EnableNetworkEvents = value; }
        }

        /// <summary>
        /// When true, power events (such as going on or coming off battery) will be logged automatically.
        /// </summary>
        public bool EnablePowerEvents
        {
            get { return m_WrappedConfiguration.EnablePowerEvents; }
            set { m_WrappedConfiguration.EnablePowerEvents = value; }
        }

        /// <summary>
        /// When true, user events (such as changing display settings and switching sessions) will be logged automatically.
        /// </summary>
        public bool EnableUserEvents
        {
            get { return m_WrappedConfiguration.EnableUserEvents; }
            set { m_WrappedConfiguration.EnableUserEvents = value; }
        }

        /// <summary>
        /// When true, assembly load information will be logged automatically.
        /// </summary>
        public bool EnableAssemblyEvents
        {
            get { return m_WrappedConfiguration.EnableAssemblyEvents; }
            set { m_WrappedConfiguration.EnableAssemblyEvents = value; }
        }

        /// <summary>
        /// When true, CLR events related to assembly resolution failures will be logged automatically.
        /// </summary>
        public bool EnableAssemblyLoadFailureEvents
        {
            get { return m_WrappedConfiguration.EnableAssemblyLoadFailureEvents; }
            set { m_WrappedConfiguration.EnableAssemblyLoadFailureEvents = value; }
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
        public bool CatchUnhandledExceptions
        {
            get { return m_WrappedConfiguration.CatchUnhandledExceptions; }
            set { m_WrappedConfiguration.CatchUnhandledExceptions = value; }
        }

        /// <summary>
        /// When true, uncaught exceptions in a Windows Forms application thread will be handled by the Agent.
        /// </summary>
        /// <remarks><para>In a Windows Forms application, the <see CREF="System.Windows.Forms.Application.Run">Application.Run</see>
        /// message loop can be configured to <see CREF="System.Windows.Forms.UnhandledExceptionMode.CatchException">CatchException</see>
        /// and issue them to the <see CREF="System.Windows.Forms.Application.ThreadException">Application.ThreadException</see>
        /// event handler, rather than to <see CREF="System.Windows.Forms.UnhandledExceptionMode.ThrowException">ThrowException</see>
        /// on up to become fatal <see CREF="AppDomain.UnhandledException">UnhandledException</see> events.</para>
        /// <para>If the Loupe Agent is configured to catch application exceptions (the default setting), the Agent will
        /// attempt to set CatchException as the setting for all application UI threads which are left in the default
        /// <see CREF="System.Windows.Forms.UnhandledExceptionMode.Automatic">Automatic</see> mode.  This can only work if
        /// the Agent is able to do so before any Forms are created, so it is recommended that a log message be issued through
        /// <see CREF="Gibraltar.Agent.Log.TraceInformation(string, object[])">Log.TraceInformation</see> (if Gibraltar Agent
        /// is linked to your application statically by reference to Gibraltar.Agent.dll) or through
        /// <see CREF="System.Diagnostics.Trace.TraceInformation(string, object[])">Trace.TraceInformation</see> (if
        /// the Loupe Agent is linked to your application dynamically by adding Loupe as a TraceListener in your
        /// app.config) early in your Program.Main method to cause the Loupe Agent to initialize itself at that point.</para>
        /// <para>If the Agent is configured not to catch application exceptions (or if it is not a Windows application)
        /// then it will not alter the default UnhandledExcetionMode and the Agent will not listen for
        /// <see CREF="System.Windows.Forms.Application.ThreadException">Application.ThreadException</see> events.  Unlike
        /// most events in .NET, these events are only sent to a single subscriber.  The most recent handler registered on
        /// that UI thread overwrites any previous handler, so even if the Agent is configured to catch application
        /// exceptions the Agent's handler can be overridden by registering your own handler for the event after initializing
        /// Loupe and before creating your application's first Form.</para></remarks>
        public bool CatchApplicationExceptions
        {
            get { return m_WrappedConfiguration.CatchApplicationExceptions; }
            set { m_WrappedConfiguration.CatchApplicationExceptions = value; }
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
        public bool ReportErrorsToUser
        {
            get { return m_WrappedConfiguration.ReportErrorsToUser; }
            set { m_WrappedConfiguration.ReportErrorsToUser = value; }
        }

        /// <summary>
        /// When true, the Loupe LogListener will end the Loupe log session when Trace.Close() is called.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.  Unless disabled by setting
        /// this configuration value to false, a call to Trace.Close() to shutdown Trace logging will also be
        /// translated into a call to Gibraltar.Agent.Log.EndSession().</remarks>
        public bool EndSessionOnTraceClose
        {
            get { return m_WrappedConfiguration.EndSessionOnTraceClose; }
            set { m_WrappedConfiguration.EndSessionOnTraceClose = value; }
        }

        /// <summary>
        /// When true, the Loupe session summary will include the exact command line and arguments used to start the process.
        /// </summary>
        /// <remarks>
        /// <para>Command line arguments can reveal sensitive information in some cases so these can be suppressed individually without
        /// requiring anonymous mode be enabled.</para>
        /// <para>If false, the session summary will not containe the command line or arguments.</para>
        /// <para>Defaults to True.</para></remarks>
        public bool EnableCommandLine
        {
            get { return m_WrappedConfiguration.EnableCommandLine; }
            set { m_WrappedConfiguration.EnableCommandLine = value; }
        }
    }
}
