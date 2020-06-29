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
using System.Diagnostics;
using System.Text;

namespace Gibraltar.Monitor.Net
{
    /// <summary>
    /// Provides support for .NET trace log listening for automatically capturing trace and console messages from the .NET runtime
    /// and recording them into the current log for this process (handled by the Log class).  
    /// </summary>
    /// <remarks>
    /// See the .NET System.Diagnostics.Trace class for more information on how to write trace messages.
    /// </remarks>
    public class LogListener : TraceListener
    {
        private bool m_ListenerActive; //indicates if we're the active listener or not.

        /// <summary>
        /// Create a new instance of the log listener.
        /// </summary>
        /// <remarks>The log listener should be managed by the Listener class instead of being directly
        /// managed by trace - do not add to the Trace Listener through its normal registration.</remarks>
        public LogListener()
        {
            //register this object with the singleton manager so we know whether we should be active or not.
            SingletonArbitrator<TraceListener>.ActiveObjectChanged += LogListener_ActiveObjectChanged;
            SingletonArbitrator<TraceListener>.Register(this);
        }

        #region Public Properties and Methods

        /// <summary>
        /// Apply the provided configuration to the listener.
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(ListenerConfiguration configuration)
        {
            //update the config of the trace publisher.
            TracePublisher.Initialize(configuration);

            //and see if we need to add ourselves as a trace listener - we do this only if 
            //auto registration is enabled AND there isn't one already.
            if (configuration.AutoTraceRegistration) 
            {
                //Guarantee that we're a registered trace listener.
                bool listenerRegistered = false;
                foreach (TraceListener listener in Trace.Listeners)
                {
                    if (listener is LogListener)
                    {
                        //this is one of us - is it us?  if so we don't worry about it.  If not, we need to track
                        //it to unregister it in a moment.
                        if (ReferenceEquals(listener, this))
                        {
                            listenerRegistered = true;
                        }
                    }
                }

                //now we need to swap items:  we register us first since double events is better than missing events.
                if (listenerRegistered == false)
                    Trace.Listeners.Add(this);
            }
        }

        /// <summary>
        /// Indicates if the trace listener is thread safe or not
        /// </summary>
        /// <remarks>The Gibraltar Log Listener is thread safe.</remarks>
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

            TracePublisher.TraceEvent(TraceOutputOptions, IndentLevel, IndentSize, eventCache, source, eventType, id, message);
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
