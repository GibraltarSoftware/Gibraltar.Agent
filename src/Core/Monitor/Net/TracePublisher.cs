
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
using System.Collections;
using System.Diagnostics;
using System.Text;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Monitor.Net
{
    /// <summary>
    /// The single central class that all Gibraltar trace listeners use to gather data from Trace into Gibraltar.  
    /// </summary>
    public static class TracePublisher
    {
        [ThreadStatic]
        private static StringBuilder t_buffer; // Each thread has its own... (Must be initialized by each thread!)
        [ThreadStatic]
        private static int t_IndentSize; // set (per-thread) on the first write after a line has ended.
        [ThreadStatic]
        private static int t_IndentLevel; // set (per-thread) on the first write after a line has ended.
        [ThreadStatic]
        private static bool t_IndentSaved; // set (per-thread) when we save the Indent info.
        [ThreadStatic]
        private static string t_IndentSizeCache;
        [ThreadStatic]
        private static string t_IndentLevelCache;

        private static ListenerConfiguration m_Configuration;

        // The indent stuff needs to be saved so we can pass it along when we wrap up the message, but it's intended
        // effect is defined when the line *starts*.  TODO: How to handle manually-set NeedIndent and inline \n ???
        private const string LogSystem = "Trace";
        private const string LogListenerCategory = "Trace";

#if STACK_DUMP
        [ThreadStatic] private static Exception t_debugException;
#endif

        /// <summary>
        /// Initialize the one trace publisher
        /// </summary>
        /// <remarks>This method ensures that the common logging class (Monitor.Log) has been initialized.</remarks>
        static TracePublisher()
        {
            //note in this case we really wil try to force the issue and initialize.
            Log.IsLoggingActive(true); //essential - we need to suppress the trace listener registration
            m_Configuration = Log.Configuration.Listener;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Apply the provided configuration to the listener.
        /// </summary>
        /// <param name="configuration"></param>
        public static void Initialize(ListenerConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="options">Output options specified by the caller.</param>
        /// <param name="indentLevel">The number of levels to indent</param>
        /// <param name="indentSize">The number of characters to indent</param>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public static void TraceEvent(TraceOptions options, int indentLevel, int indentSize, TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            string indent = ComputeIndentString(indentLevel, indentSize);

            SimpleLogMessage logMessage = new SimpleLogMessage((LogMessageSeverity)eventType, LogSystem,
                                                               LogListenerCategory, 2, indent + "Event {0}", id);
            logMessage.PublishToLog();
        }


        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="options">Output options specified by the caller.</param>
        /// <param name="indentLevel">The number of levels to indent</param>
        /// <param name="indentSize">The number of characters to indent</param>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public static void TraceEvent(TraceOptions options, int indentLevel, int indentSize, TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            string indent = ComputeIndentString(indentLevel, indentSize);

            SimpleLogMessage logMessage = (id != 0)
                                              ? new SimpleLogMessage((LogMessageSeverity)eventType, LogSystem, LogListenerCategory, 2,
                                                  indent + "Event {0}: {1}", id, message)
                                              : new SimpleLogMessage((LogMessageSeverity)eventType, LogSystem, LogListenerCategory, 2,
                                                  indent + (message ?? string.Empty));
            
            logMessage.PublishToLog();
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="options">Output options specified by the caller.</param>
        /// <param name="indentLevel">The number of levels to indent</param>
        /// <param name="indentSize">The number of characters to indent</param>
        /// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public static void TraceEvent(TraceOptions options, int indentLevel, int indentSize, TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, object[] args)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            string indent = ComputeIndentString(indentLevel, indentSize);

            SimpleLogMessage logMessage = (id != 0)
                                              ? new SimpleLogMessage((LogMessageSeverity)eventType, LogSystem, LogListenerCategory, 2, 
                                                  string.Format("{2}Event {0}: {1}", id, format, indent), args)
                                              : new SimpleLogMessage((LogMessageSeverity)eventType, LogSystem, LogListenerCategory, 2,
                                                  indent + (format ?? string.Empty), args);
            logMessage.PublishToLog();
        }

        /// <summary>
        /// Writes the specified message to the output stream for the current thread.
        /// </summary>
        /// <param name="message">A message to write. </param>
        /// <param name="indentLevel">The number of levels to indent</param>
        /// <param name="indentSize">The number of characters to indent</param>
        /// <param name="needIndent">Indicates if an indent is needed.</param>
        /// <filterpriority>2</filterpriority>
        public static void Write(string message, ref bool needIndent, int indentLevel, int indentSize)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            WriteMessage(message, false, ref needIndent, indentLevel, indentSize);
        }

        /// <summary>
        /// Writes the specified message to the output stream for the current thread and completes the current buffer.
        /// </summary>
        /// <param name="indentLevel">The number of levels to indent</param>
        /// <param name="indentSize">The number of characters to indent</param>
        /// <param name="needIndent">Indicates if an indent is needed.</param>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public static void WriteLine(string message, ref bool needIndent, int indentLevel, int indentSize)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            WriteMessage(message, true, ref needIndent, indentLevel, indentSize);
        }

        /// <summary>
        /// Flush the information to disk.
        /// </summary>
        public static void Flush()
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            try
            {
                if (t_buffer == null) // If this is our first access by this thread...
                {
                    t_buffer = new StringBuilder(); // ...then initialize this thread's buffer.
                }
#if STACK_DUMP
                t_debugException = new GibraltarStackInfoException("Trace - Flush()", t_debugException);
#endif
                if (t_buffer.Length > 0)
                {
                    bool needIndent = false;
                    WriteLine(null, ref needIndent, 0, 0);
                }
            }
            catch (Exception exception)
            {
                // Don't let a log message cause problems
                GC.KeepAlive(exception);
            }
        }

        /// <summary>
        /// Close the listener because Trace is shutting down.
        /// </summary>
        public static void Close()
        {
            //we are NOT checking is logging active here because End Session is a control flow thing, and we don't want to either spin us up or get into a race condition.

            //if we're the active listener we need to worry about the end session option, if it hasn't been called already.
            if ((m_Configuration != null) && (m_Configuration.EndSessionOnTraceClose) && (Log.IsSessionEnding == false))
            {
                Log.EndSession(SessionStatus.Normal, 4, "System.Diagnostics.Trace has been closed.");
            }
        }

        /// <summary>
        /// Called to write out an indention on the current thread's string buffer
        /// </summary>
        public static void WriteIndent(int indentLevel, int indentSize)
        {
            if (Log.IsLoggingActive(true) == false)
                return;

            if (t_buffer == null) // If this is our first access by this thread...
            {
                t_buffer = new StringBuilder(); // ...then initialize this thread's buffer.
            }

            t_buffer.Append(ComputeIndentString(indentLevel, indentSize));

            if (t_IndentSaved == false)
            {
                // Only save these once per aggregated log message, to lock in their effect.
                t_IndentSize = indentSize;
                t_IndentLevel = indentLevel;
                t_IndentSaved = true;
#if STACK_DUMP
                t_debugException = new GibraltarStackInfoException("Trace - WriteIndent(): "+t_IndentSize+" * "+t_IndentLevel, t_debugException);
#endif
            }
#if STACK_DUMP
            else
            {
                t_debugException = new GibraltarStackInfoException("Trace - WriteIndent(): Ignored", t_debugException);
            }
#endif
        }

        #endregion

        #region Private Properties and Methods

        private static string ComputeIndentString(int indentLevel, int indentSize)
        {
            if (indentSize <= 0 || indentLevel <= 0)
                return string.Empty; // Shortcut bail if size or level is 0 (or illegal negative).

            string powerString;
            string sizeString;
            string levelString;

            if (t_IndentSizeCache != null && t_IndentSizeCache.Length == indentSize)
            {
                sizeString = t_IndentSizeCache; // Use the cached string, already the right size.
            }
            else
            {
                // We have to compute it.
                t_IndentLevelCache = null; // Size changed.  Invalidate the indent level cache string.
                powerString = " ";
                sizeString = string.Empty;
                for (int size = indentSize; size > 1; size >>= 1)
                {
                    if ((size & 1) != 0)
                        sizeString += powerString;

                    powerString += powerString; // Double it for next higher bit.
                }
                sizeString += powerString; // There has to be a final 1 bit at the top, we weeded out 0.

                t_IndentSizeCache = sizeString; // Store this for reuse efficiency.
            }

            if (t_IndentLevelCache != null && t_IndentLevelCache.Length == (indentLevel * indentSize))
            {
                levelString = t_IndentLevelCache; // Use the cached string, already the right level & size.
            }
            else
            {
                powerString = sizeString;
                levelString = string.Empty;
                for (int level = indentLevel; level > 1; level >>= 1)
                {
                    if ((level & 1) != 0)
                        levelString += powerString;

                    powerString += powerString; // Double it for next higher bit.
                }
                levelString += powerString; // There has to be a final 1 bit at the top, we weeded out 0.

                t_IndentLevelCache = levelString; // Store this for reuse efficiency.
            }

            return levelString;
        }

/*KM:  This turns out to be a bad way to do this; the call stack isn't being pre-calculated so we
 * end up inside the call stack and then we do double-work to figure it out again when we go into simple message.
 * If we want to support these options we should do it in SimpleMessage.
        private static string ComputeTraceOptions(TraceOptions options, TraceEventCache cache)
        {
            if (options == TraceOptions.None)
                return string.Empty; //bail fast and return constant that is appendable safely

            //we only respect a few options, and they change on every call.
            StringBuilder outputBuffer = new StringBuilder(1024);

            if (((options & TraceOptions.LogicalOperationStack) != 0)
                && (cache.LogicalOperationStack != null))
            {
                lock(cache.LogicalOperationStack.SyncRoot) //logical operation stack is not thread safe.
                {
                    object[] operations = cache.LogicalOperationStack.ToArray();
                    if ((operations != null) && (operations.Length > 0))
                    {
                        outputBuffer.AppendLine("\r\nLogical Operation Stack:\r\n");
                        foreach (object operation in operations)
                        {
                            string stringOperation = operation as string;
                            if (stringOperation != null) //it is a string
                            {
                                outputBuffer.AppendLine(stringOperation);
                            }
                            else
                            {
                                outputBuffer.AppendLine(operation.ToString());
                            }
                        }
                    }
                }
            }

            if ((options & TraceOptions.Callstack) != 0)
            {
                string callStack = cache.Callstack;
                if (string.IsNullOrEmpty(callStack) == false)
                {
                    outputBuffer.AppendLine("\r\nCall Stack:\r\n" + callStack + "\r\n");
                }
            }

            return outputBuffer.ToString();         
        }
*/

        private static void WriteMessage(string message, bool endLine, ref bool needIndent, int indentLevel, int indentSize)
        {
            try
            {
                if (t_buffer == null) // If this is our first access by this thread...
                {
                    t_buffer = new StringBuilder(); // ...then initialize this thread's buffer.
                }

                if (needIndent)
                {
                    WriteIndent(indentLevel, indentSize);
                }

                if (string.IsNullOrEmpty(message) == false)
                {
                    t_buffer.Append(message);
                }

                if (endLine)
                {
                    //t_buffer.Append("<Line>");
#if STACK_DUMP
                    t_debugException = new GibraltarStackInfoException("Trace - WriteLine()", t_debugException);
                    Exception dumpException = t_debugException;
#else
                    const Exception dumpException = null;
#endif

                    // Then pull the contents of the buffer as a complete line into a log message.
                    SimpleLogMessage logMessage = new SimpleLogMessage(LogMessageSeverity.Verbose, LogWriteMode.Queued,
                                                                       LogSystem, LogListenerCategory, 3, dumpException,
                                                                       t_buffer.ToString());
#if STACK_DUMP
                    t_debugException = null;
#endif

                    t_buffer.Length = 0; // Reset the buffer to empty again (do it here just in case publishing crashes).
                    t_IndentSaved = false; // Mark our indent info as no longer valid.
                    t_IndentLevel = 0; // Reset to 0's here so we don't have to check t_IndentSaved before using them above.
                    t_IndentSize = 0; // Should we leave this one, or reset it here, too?
                    needIndent = true;
                    logMessage.PublishToLog(); // And send the completed log message to the Log.
                }
#if STACK_DUMP
                else
                {
                    t_debugException = new GibraltarStackInfoException("Trace - Write()", t_debugException);
                }
#endif
            }
            catch (Exception)
            {
                // We generally just want to suppress all exceptions, but if we're actively debugging...

                Log.DebugBreak(); // Use our debugging breakpoint.  This is ignored in production.
            }
        }

        #endregion

    }
}
