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
 *    Copyright � 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Diagnostics;
using System.Text;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Net;

#endregion File Header

namespace Gibraltar.Agent
{
    /// <summary>
    /// Implements a TraceListener to forward Trace and Debug messages from
    /// System.Diagnostics.Trace to the Loupe Agent.
    /// </summary>
    /// <remarks>
    /// 	<para>This class is not normally used directly but instead is registered in the
    ///     App.Config file according to the example above.</para>
    /// 	<para>
    ///         For more information on how to record log messages with the Loupe Agent,
    ///         see the <see cref="Log">Log Class</see>.
    ///     </para>
    /// 	<para>For more information on logging using Trace, see <a href="Logging_Trace.html">Developer's Reference - Logging - Using with Trace</a>.</para>
    /// </remarks>
    /// <seealso cref="!:Logging_Trace.html" cat="Developer's Reference">Logging - Using with Trace</seealso>
    /// <example>
    /// 	<code lang="XML" title="Loupe Trace Listener Registration" description="The easiest way to add Loupe to an application is to register it in the App.Config XML file. Each application has an XML configuration file that is used to hold options that can be changed without recompiling the application. These options apply to all users of the application.">
    /// 		<![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <configuration>
    /// <!-- You may already have a <system.diagnostics> section in your configuration file,
    ///    possibly also including the <trace> and <listeners> tags.  If this is the case,
    ///    you only need to add the line that starts <add name="gibraltar" ... /> to the file -->
    ///  <system.diagnostics>
    ///    <trace>
    ///      <listeners>
    ///        <add name="gibraltar" type="Gibraltar.Agent.LogListener, Gibraltar.Agent"/>
    ///      </listeners>
    ///    </trace>
    ///  </system.diagnostics>
    /// </configuration>]]>
    /// 	</code>
    /// </example>
    public sealed class LogListener : TraceListener
    {
        private bool m_ListenerActive; //indicates if we're the active listener or not.

        /// <summary>
        /// Create a new instance of the log listener.
        /// </summary>
        /// <remarks>The log listener should be managed by the Listener class instead of being directly
        /// managed by trace - do not add to the Trace Listener through its normal registration.</remarks>
        public LogListener()
        {
            //we have to directly mark silent mode because we don't want to cause initialization during our constructor.
            Monitor.Log.SilentMode = true;

            //register this object with the singleton manager so we know whether we should be active or not.
            SingletonArbitrator<TraceListener>.ActiveObjectChanged += LogListener_ActiveObjectChanged;
            SingletonArbitrator<TraceListener>.Register(this);
        }

        #region Public Properties and Methods

        /// <summary>
        /// Indicates if the trace listener is thread safe or not
        /// </summary>
        /// <remarks>The Loupe Log Listener is thread safe.</remarks>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (m_ListenerActive == false)
                return;

            TracePublisher.TraceEvent(TraceOutputOptions, IndentLevel, IndentSize, eventCache, source, eventType, id);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (m_ListenerActive == false)
                return;

            TracePublisher.TraceEvent(TraceOutputOptions,  IndentLevel, IndentSize, eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, object[] args)
        {
            if (m_ListenerActive == false)
                return;

            TracePublisher.TraceEvent(TraceOutputOptions, IndentLevel, IndentSize, eventCache, source, eventType, id, format, args);
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void Write(string message)
        {
            if (m_ListenerActive == false)
                return;

            bool needIndent = NeedIndent;
            TracePublisher.Write(message, ref needIndent, IndentLevel, IndentSize);
            NeedIndent = needIndent;
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void WriteLine(string message)
        {
            if (m_ListenerActive == false)
                return;

            bool needIndent = NeedIndent;
            TracePublisher.WriteLine(message, ref needIndent, IndentLevel, IndentSize);
            NeedIndent = needIndent;
        }

        /// <summary>
        /// Flush the information to disk.
        /// </summary>
        public override void Flush()
        {
            if (m_ListenerActive == false)
                return;

            TracePublisher.Flush();
        }

        /// <summary>
        /// Close the listener because Trace is shutting down.
        /// </summary>
        public override void Close()
        {
            if (m_ListenerActive)
            {
                TracePublisher.Close();
            }

            //now that we're being closed we need to unregister ourselves.
            SingletonArbitrator<TraceListener>.Unregister(this);
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// When overridden in a derived class, records a thread-specific indentation.
        /// </summary>
        protected override void WriteIndent()
        {
            if (m_ListenerActive == false)
                return;

            TracePublisher.WriteIndent(IndentLevel, IndentSize);
            NeedIndent = false;
        }

        #endregion

        #region Event Handlers

        void LogListener_ActiveObjectChanged(object sender, EventArgs e)
        {
            //see if we are or aren't active.
            m_ListenerActive = ReferenceEquals(SingletonArbitrator<TraceListener>.ActiveObject, this);
        }

        #endregion
    }
}
