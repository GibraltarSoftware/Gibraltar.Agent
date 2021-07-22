
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
using System;
using System.Configuration;
using System.Xml;
using Gibraltar.Agent;

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Configuration information for the trace listener.
    /// </summary>
    public class ListenerConfiguration
    {
        private readonly XmlNode m_GibraltarNode;

        /// <summary>
        /// Initialize from application configuration section
        /// </summary>
        /// <param name="configuration"></param>
        internal ListenerConfiguration(ListenerElement configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Initialize from an xml document
        /// </summary>
        internal ListenerConfiguration(XmlNode gibraltarNode)
        {
            m_GibraltarNode = gibraltarNode;

            //create an element object so we have something to draw defaults from.
            ListenerElement baseline = new ListenerElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("listener");

            //copy the provided configuration
            if (node != null)
            {
                AutoTraceRegistration = AgentConfiguration.ReadValue(node, "autoTraceRegistration", baseline.AutoTraceRegistration);
                CatchUnhandledExceptions = AgentConfiguration.ReadValue(node, "catchUnhandledExceptions", baseline.CatchUnhandledExceptions);
                CatchApplicationExceptions = AgentConfiguration.ReadValue(node, "catchApplicationExceptions", baseline.CatchApplicationExceptions);
                ReportErrorsToUser = AgentConfiguration.ReadValue(node, "reportErrorsToUser", baseline.ReportErrorsToUser);
                EnableConsole = AgentConfiguration.ReadValue(node, "enableConsole", baseline.EnableConsole);
                EnableAssemblyEvents = AgentConfiguration.ReadValue(node, "enableAssemblyEvents", baseline.EnableAssemblyEvents);
                EnableAssemblyLoadFailureEvents = AgentConfiguration.ReadValue(node, "enableAssemblyLoadFailureEvents", baseline.EnableAssemblyLoadFailureEvents);
                EnableDiskPerformance = AgentConfiguration.ReadValue(node, "enableDiskPerformance", baseline.EnableDiskPerformance);
                EnableMemoryPerformance = AgentConfiguration.ReadValue(node, "enableMemoryPerformance", baseline.EnableMemoryPerformance);
                EnableNetworkEvents = AgentConfiguration.ReadValue(node, "enableNetworkEvents", baseline.EnableNetworkEvents);
                EnableNetworkPerformance = AgentConfiguration.ReadValue(node, "enableNetworkPerformance", baseline.EnableNetworkPerformance);
                EnablePowerEvents = AgentConfiguration.ReadValue(node, "enablePowerEvents", baseline.EnablePowerEvents);
                EnableProcessPerformance = AgentConfiguration.ReadValue(node, "enableProcessPerformance", baseline.EnableProcessPerformance);
                EnableSystemPerformance = AgentConfiguration.ReadValue(node, "enableSystemPerformance", baseline.EnableSystemPerformance);
                EnableUserEvents = AgentConfiguration.ReadValue(node, "enableUserEvents", baseline.EnableUserEvents);
                EndSessionOnTraceClose = AgentConfiguration.ReadValue(node, "endSessionOnTraceClose", baseline.EndSessionOnTraceClose);
            }
        }

        #region Public Properties and Methods


        /// <summary>
        /// Configures whether Gibraltar should automatically make sure it is registered as a Trace Listener.
        /// </summary>
        /// <remarks>This is true by default to enable easy drop-in configuration (eg. using the LiveLogViewer
        /// control on a form).  Normally, it should not be necessary to disable this feature even when adding
        /// Gibraltar as a Trace Listener in an app.config or by code.  But this setting can be configured
        /// to false if it is desirable to prevent Gibraltar from receiving Trace events directly, such as
        /// if the application is processing Trace events into the Gibraltar API itself.</remarks>
        public bool AutoTraceRegistration { get; set; }

        /// <summary>
        /// When true, anything written to the console out will be appended to the log.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.</remarks>
        public bool EnableConsole { get; set; }

        /// <summary>
        /// When true, process performance information will be automatically captured for the current process
        /// </summary>
        /// <remarks>This includes basic information on processor and memory utilization for the running process.</remarks>
        public bool EnableProcessPerformance { get; set; }

        /// <summary>
        /// When true, disk performance information will be automatically captured
        /// </summary>
        public bool EnableDiskPerformance { get; set; }

        /// <summary>
        /// When true, extended .NET memory utilization information will be automatically captured
        /// </summary>
        /// <remarks>The extended information is primarily useful for narrowing down memory leaks.  Basic 
        /// memory utilization information (sufficient to identify if a leak is likely) is captured 
        /// as part of the EnableProcessPerformance option.</remarks>
        public bool EnableMemoryPerformance { get; set; }

        /// <summary>
        /// When true, network performance information will be automatically captured
        /// </summary>
        public bool EnableNetworkPerformance { get; set; }

        /// <summary>
        /// When true, system performance information will be automatically captured
        /// </summary>
        public bool EnableSystemPerformance { get; set; }

        /// <summary>
        /// When true, network events (such as reconfiguration and disconnection) will be logged automatically.
        /// </summary>
        public bool EnableNetworkEvents { get; set; }

        /// <summary>
        /// When true, power events (such as going on or coming off battery) will be logged automatically.
        /// </summary>
        public bool EnablePowerEvents { get; set; }

        /// <summary>
        /// When true, user events (such as changing display settings and switching sessions) will be logged automatically.
        /// </summary>
        public bool EnableUserEvents { get; set; }

        /// <summary>
        /// When true, assembly load information will be logged automatically.
        /// </summary>
        public bool EnableAssemblyEvents { get; set; }

        /// <summary>
        /// When true, CLR events related to assembly resolution failures will be logged automatically.
        /// </summary>
        public bool EnableAssemblyLoadFailureEvents { get; set; }

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
        public bool CatchUnhandledExceptions { get; set; }

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
        public bool CatchApplicationExceptions { get; set; }

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
        public bool ReportErrorsToUser { get; set; }

        /// <summary>
        /// When true, the Gibraltar LogListener will end the Gibraltar log session when Trace.Close() is called.
        /// </summary>
        /// <remarks>This setting has no effect if the trace listener is not enabled.  Unless disabled by setting
        /// this configuration value to false, a call to Trace.Close() to shutdown Trace logging will also be
        /// translated into a call to Gibraltar.Agent.Log.EndSession().</remarks>
        public bool EndSessionOnTraceClose { get; set; }

        /// <summary>
        /// When true, the Loupe session summary will include the exact command line and arguments used to start the process.
        /// </summary>
        /// <remarks>
        /// <para>Command line arguments can reveal sensitive information in some cases so these can be suppressed individually without
        /// requiring anonymous mode be enabled.</para>
        /// <para>If false, the session summary will not contain the command line or arguments.</para>
        /// <para>Defaults to True.</para></remarks>
        public bool EnableCommandLine { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            ListenerElement baseline = new ListenerElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("listener");
            AgentConfiguration.WriteValue(newNode, "autoTraceRegistration", AutoTraceRegistration, baseline.AutoTraceRegistration);
            AgentConfiguration.WriteValue(newNode, "catchUnhandledExceptions", CatchUnhandledExceptions, baseline.CatchUnhandledExceptions);
            AgentConfiguration.WriteValue(newNode, "catchApplicationExceptions", CatchApplicationExceptions, baseline.CatchApplicationExceptions);
            AgentConfiguration.WriteValue(newNode, "reportErrorsToUser", ReportErrorsToUser, baseline.ReportErrorsToUser);
            AgentConfiguration.WriteValue(newNode, "enableCommandLine", EnableCommandLine, baseline.EnableCommandLine);
            AgentConfiguration.WriteValue(newNode, "enableConsole", EnableConsole, baseline.EnableConsole);
            AgentConfiguration.WriteValue(newNode, "enableAssemblyEvents", EnableAssemblyEvents, baseline.EnableAssemblyEvents);
            AgentConfiguration.WriteValue(newNode, "enableAssemblyLoadFailureEvents", EnableAssemblyLoadFailureEvents, baseline.EnableAssemblyLoadFailureEvents);
            AgentConfiguration.WriteValue(newNode, "enableDiskPerformance", EnableDiskPerformance, baseline.EnableDiskPerformance);
            AgentConfiguration.WriteValue(newNode, "enableMemoryPerformance", EnableMemoryPerformance, baseline.EnableMemoryPerformance);
            AgentConfiguration.WriteValue(newNode, "enableNetworkEvents", EnableNetworkEvents, baseline.EnableNetworkEvents);
            AgentConfiguration.WriteValue(newNode, "enableNetworkPerformance", EnableNetworkPerformance, baseline.EnableNetworkPerformance);
            AgentConfiguration.WriteValue(newNode, "enablePowerEvents", EnablePowerEvents, baseline.EnablePowerEvents);
            AgentConfiguration.WriteValue(newNode, "enableProcessPerformance", EnableProcessPerformance, baseline.EnableProcessPerformance);
            AgentConfiguration.WriteValue(newNode, "enableSystemPerformance", EnableSystemPerformance, baseline.EnableSystemPerformance);
            AgentConfiguration.WriteValue(newNode, "enableUserEvents", EnableUserEvents, baseline.EnableUserEvents);
            AgentConfiguration.WriteValue(newNode, "endSessionOnTraceClose", EndSessionOnTraceClose, baseline.EndSessionOnTraceClose);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }
        
        #endregion

        #region Internal Properties and Methods

        internal void Sanitize()
        {
        }

        #endregion

        #region Private Properties and Methods

        private void Initialize(ListenerElement configuration)
        {
            //copy the provided configuration
            AutoTraceRegistration = configuration.AutoTraceRegistration;
            CatchUnhandledExceptions = configuration.CatchUnhandledExceptions;
            CatchApplicationExceptions = configuration.CatchApplicationExceptions;
            ReportErrorsToUser = configuration.ReportErrorsToUser;
            EnableCommandLine = configuration.EnableCommandLine;
            EnableConsole = configuration.EnableConsole;
            EnableAssemblyEvents = configuration.EnableAssemblyEvents;
            EnableAssemblyLoadFailureEvents = configuration.EnableAssemblyLoadFailureEvents;
            EnableDiskPerformance = configuration.EnableDiskPerformance;
            EnableMemoryPerformance = configuration.EnableMemoryPerformance;
            EnableNetworkEvents = configuration.EnableNetworkEvents;
            EnableNetworkPerformance = configuration.EnableNetworkPerformance;
            EnablePowerEvents = configuration.EnablePowerEvents;
            EnableProcessPerformance = configuration.EnableProcessPerformance;
            EnableSystemPerformance = configuration.EnableSystemPerformance;
            EnableUserEvents = configuration.EnableUserEvents;
            EndSessionOnTraceClose = configuration.EndSessionOnTraceClose;
        }

        #endregion
    }
}
