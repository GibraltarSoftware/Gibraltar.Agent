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
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#endregion File Header

#pragma warning disable 162, 169
namespace Gibraltar
{
    /// <summary>
    /// This static helper class is invoked by all error handling logic within Gibraltar collectors.
    /// </summary>
    /// <remarks>
    /// The intention is that exceptions will only be raised in debug builds.  In release builds
    /// the logging system 
    /// </remarks>
    public static class ErrorNotifier
    {

        /// <summary>
        /// Event that is raised when a Gibraltar library exception occurs
        /// </summary>
        public static event EventHandler<ErrorNotificationEventArgs> Error;

        private static bool s_ReportingError;
        private static Exception s_LastException;
        private static int s_ErrorCount;
        private static int s_MaxErrorCount = 100;
#if DEBUG
        private static ExceptionPolicy s_Policy = ExceptionPolicy.RaiseNoExceptions;
#else
        private static ExceptionPolicy s_Policy = ExceptionPolicy.RaiseNoExceptions;
#endif
        private static bool s_DisableLogging = true;
        private static readonly object s_SyncObject = new object();

        /// <summary>
        /// All messages written by ErrorNotifier to Trace/Console will be prefixed with this string.
        /// This allows for the filtering out these messages by the listeners attached to those sources.
        /// </summary>
        public const string GibraltarErrorPrefix = "LOUPE ERROR:";

        /// <summary>
        /// Holds a reference to the last internal Gibraltar exception raised (if any)
        /// </summary>
        public static Exception LastException { get { return s_LastException; } }

        /// <summary>
        /// Returns a count of the number of internal Gibraltar exceptions raised.
        /// </summary>
        public static int ErrorCount { get { return s_ErrorCount; } }
        
        /// <summary>
        /// Gets or sets the high limit of internal Gibraltar exceptions that 
        /// should trigger disabling of all Gibraltar logging.
        /// </summary>
        public static int MaxErrorCount { get { return s_MaxErrorCount; } set { s_MaxErrorCount = value; } }

        /// <summary>
        /// Gets or sets the policy that determines how internal Gibraltar exceptions will be handled.
        /// </summary>
        public static ExceptionPolicy Policy { get { return s_Policy; } set { s_Policy = value; } }

        /// <summary>
        /// Gets or sets a value indicating that Gibraltar logging should be disabled due to excessive
        /// internal errors.  This flag should be checked by Gibraltar logging components
        /// </summary>
        public static bool DisableLogging { get { return s_DisableLogging; } set { s_DisableLogging = value; } }

        /// <summary>
        /// Raises the Error event with the provided exception
        /// </summary>
        public static void Notify(object sender, Exception exception)
        {
            //NOTE:  We NEVER do any logging or anything; this class is a half- step and we don't want to do anything.
            return;

            s_LastException = exception;
            s_ErrorCount++;

            // During development it is usually best to have all exceptions raised
            // so that any errors in the way Gibraltar is configured or being used
            // can be identified and corrected.
            
            // As with Log4Net, we will allow certain types of exceptions to be passed
            // to the user.  These exception types should be used exclusively for
            // exceptions that indicate that the application called Gibraltar with
            // invalid arguments that the developer needs to fix.
            if ( s_Policy != ExceptionPolicy.RaiseNoExceptions )
            {
                if (exception is ArgumentException || exception is ArgumentNullException)
                {
                    // throw exception;
                    throw new GibraltarException("ErrorNotifier received an argument exception.", exception);
                }
                else if (s_Policy == ExceptionPolicy.RaiseAllExceptions)
                {
                    // throw exception;
                    throw new GibraltarException("ErrorNotifier received an exception.", exception);
                }
            }

            // We are not expecting any exceptions from within Gibraltar.  So, if we get
            // any, it is noteworthy -- let's be sure to report it.
            // We don't want to report all exceptions though because we don't want to
            // risk flooding the log with Gibraltar exceptions.  So, after the first
            // exception, do no special reporting unless/until we see that we're getting
            // so many Gibraltar exceptions that we should conclude that we really
            // FUBAR and should disable logging entirely.
            if (s_ErrorCount == 1)
            {
                LogApplicationError(exception, false);
            }
            else if (s_ErrorCount == s_MaxErrorCount)
            {
                LogApplicationError(exception, true);
            }
            else if (s_ErrorCount > s_MaxErrorCount)
            {
                // We shouldn't be here because logging should be disabled by now.
                // But, if we're here, let's not make things worse.  Just get out
                // and hope for the best.
                return;
            }

            // The logic that follows will raise the Notify event to any/all subscribers

            // assign to a local variable to ensure thread safety
            EventHandler<ErrorNotificationEventArgs> errorNotificationEventHandler = Error;

            // In the (common) case in which nothing is subscribed, no notification is needed
            if (errorNotificationEventHandler == null)
                return;

            // Raise the event asynchronously
            try
            {
                ErrorNotificationEventArgs args = new ErrorNotificationEventArgs(exception);
                errorNotificationEventHandler.Invoke(sender, args);
            }
            catch (Exception ex)
            {
                LogApplicationError( ex, false );
            }
        }

        private static void LogApplicationError(Exception exception, bool disableLogging)
        {
            // Use a lock to ensure single threaded behavior through this routine
            lock(s_SyncObject)
            {
                // Within a single thread, we want to handle the possibility of cascading exceptions.
                // If a thread raises another exception while we're in the process of reporting an
                // exception, something is woefully wrong, so disable logging and get out.
                if (s_ReportingError)
                {
                    s_DisableLogging = true;
                    return;
                }


                try
                {
                    // Set flag to catch the case of a cascading exception
                    s_ReportingError = true;

                    // To start with, let's report this to Gibraltar (hopefully this won't raise another exception)
                    try
                    {
                        //KM:  At the moment, not possible due to circular dependency
                        // Log.Write(LogMessageSeverity.Error, exception, "Logging an application error exception.");
                    }
                    catch (Exception ex)
                    {
                        // We should never get here because Log should be trapping exceptions.
                        // But, just to make sure, we'll catch here too.
                        GC.KeepAlive(ex);
                    }

                    // We don't need try/catch here because that's handled in WriteTraceMessage
                    WriteTraceMessage(exception.Message, exception);
                    if (disableLogging)
                    {
                        WriteTraceMessage("Disabling logging after " + s_MaxErrorCount + " errors.", null);
                        s_DisableLogging = true;
                    }
                }
                finally
                {
                    // We're done reporting this error
                    s_ReportingError = false;
                }
            }
        }

        private static void WriteTraceMessage(string message, Exception exception)
        {
            // Let's prefix all messages we post.  This will make them easier to find
            // and will also allow for filtering out those messages in Gibraltar
            // listeners attached to the Console/Trace streams.
            message = GibraltarErrorPrefix + message;

            // First, let's report the error to the Windows Application log
            try
            {
                string machineName = Environment.MachineName;
                EventLog eventLog = new EventLog("Application", machineName);
                eventLog.Source = "Gibraltar";
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                // We don't want to allow any exceptions to prevent the rest of the processing
                GC.KeepAlive(ex);
            }

            // Log to the Console in the event that there is some other logging going on
            // in addition to Gibraltar
            try
            {
                Console.Error.WriteLine(message);
            }
            catch (Exception ex)
            {
                // We don't want to allow any exceptions to prevent the rest of the processing
                GC.KeepAlive(ex);
            }

            // Log to Trace as well, just to do everything safe we can to let the outside
            // world know that Gibraltar is having trouble
            try
            {
                Trace.TraceError(message, exception);
            }
            catch (Exception ex)
            {
                // We don't want to allow any exceptions to prevent the rest of the processing
                GC.KeepAlive(ex);
            }
        }

        #region Safe string format expansion with embedded error reporting

        /// <summary>
        /// Safely attempt to expand a format string with supplied arguments.
        /// </summary>
        /// <remarks>If the normal call to string.Format() fails, this method does its best to create a string
        /// (intended as a log message) error message containing the original format string and a representation
        /// of the args supplied, to attempt to preserve meaningful information despite the user's mistake.</remarks>
        /// <param name="formatProvider">An IFormatProvider (such as a CultureInfo) to use, where applicable.
        /// (may be null, indicating the current culture)</param>
        /// <param name="format">The desired format string, as used by string.Format().</param>
        /// <param name="args">An array of args, as used by string.Format() after the format string.</param>
        /// <returns>The formatted string, or an error string containing best-effort information.</returns>
        public static string SafeFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                // No arguments were supplied, so the "format" string is returned without any expansion.
                // Providing null or empty is also legal in this case, and we'll treat them both as empty.
                return format ?? string.Empty; // Protect against a null, always return a valid string.
            }

            string resultString;
            Exception formattingException = null;

            // If format is null, we want to get the exception from string.Format(), but we don't want to pass in
            // an empty format string (which won't fail but will drop all of their arguments).
            // So this is not the usual IsNullOrEmpty() check, it's null-or-not-empty that we want here.
            if (format == null || format.Length > 0)
            {
                try
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    resultString = string.Format(formatProvider, format, args);
                    // ReSharper restore AssignNullToNotNullAttribute
                }
                catch (Exception ex)
                {
                    // Catch all exceptions.
                    formattingException = ex;
                    resultString = null; // Signal a failure, so we can exit the catch block for further error handling.
                }
            }
            else
            {
                // They supplied arguments with an empty or null format string, so they won't get any info!
                // We'll treat this as an error case, so they get the data from the args in our error handling.
                resultString = null;
            }

            if (resultString == null)
            {
                // There was some formatting error, so we want to format an error string with all the useful info we can.

                StringBuilder supportBuilder = new StringBuilder(format ?? string.Empty); // For support people.
                StringBuilder devBuilder = new StringBuilder(); // For developers.
                string formatString = ReverseEscapes(format);
                
                // Add a blank line after the format string for support.  We need a second line break if there wasn't one already.
                if (string.IsNullOrEmpty(format))
                {
                    // ToDo: Decide if we actually want the extra one in this case.  It seems unnecessary.
                    supportBuilder.Append("\r\n"); // There wasn't one already, so add the first linebreak...
                }
                else
                {
                    char lastChar = format[format.Length - 1];
                    if (lastChar != '\n' && lastChar != '\r')
                        supportBuilder.Append("\r\n"); // Make sure this case ends with some kind of a linebreak...
                }
                // The second line break will come at the start of the first Value entry.

                if (formattingException != null)
                {
                    devBuilder.AppendFormat("\r\n\r\n\r\nError expanding message format with {0} args supplied:\r\nException = ", args.Length);
                    devBuilder.Append(SafeToString(formatProvider, formattingException, false));

                    // Use formatString here rather than format because it has the quotes around it and handles the null case.
                    devBuilder.AppendFormat(formatProvider, "\r\nFormat string = {0}", formatString);
                }

                // Now loop over the args provided.  We need to add each entry to supportBuilder and devBuilder.

                for (int i = 0; i < args.Length; i++)
                {
                    object argI = args[i];

                    supportBuilder.AppendFormat(formatProvider, "\r\nValue #{0}: {1}", i,
                                                SafeToString(formatProvider, argI, false));
                    // Only doing devBuilder if we have an actual formatting Exception.  Empty format case doesn't bother.
                    if (formattingException != null)
                    {
                        if (argI == null)
                        {
                            // We can't call GetType() from a null, can we?  I think any original cast type for the null is lost
                            // by this point, so we can't report a type for it (other than "object"), so just report it as a null.
                            devBuilder.AppendFormat(formatProvider, "\r\nargs[{0}] {1}", i,
                                                    SafeToString(formatProvider, argI, true));
                        }
                        else
                        {
                            string typeName = argI.GetType().FullName;
                            devBuilder.AppendFormat(formatProvider, "\r\nargs[{0}] ({1}) = {2}", i, typeName,
                                                    SafeToString(formatProvider, argI, true));
                        }
                    }
                }

                supportBuilder.Append(devBuilder); // Append the devBuilder section
                resultString = supportBuilder.ToString();
            }

            return resultString;
        }

        private static readonly char[] ResolvedEscapes = new[] { '\r', '\n', '\t', '\"', '\\', };
        private static readonly string[] LiteralEscapes = new[] { "\\r", "\\n", "\\t", "\\\"", "\\\\", };
        private static readonly Dictionary<char,string> EscapeMap = InitEscapeMap();

        /// <summary>
        /// Initializes the EscapeMap dictionary.
        /// </summary>
        private static Dictionary<char,string> InitEscapeMap()
        {
            // Allocate and initialize our mapping of special resolved-escape characters to corresponding string literals.
            int size = ResolvedEscapes.Length;
            Dictionary<char,string> escapeMap = new Dictionary<char,string>(size);

            for (int i=0; i < size; i++)
            {
                escapeMap[ResolvedEscapes[i]] = LiteralEscapes[i];
            }
            return escapeMap;
        }

        /// <summary>
        /// Expand (some) special characters back to how they appear in string literals in source code.
        /// </summary>
        /// <remarks>This currently does nothing but return the original string.</remarks>
        /// <param name="format">The string (e.g. a format string) to convert back to its literal appearance.</param>
        /// <returns>A string with embedded backslash escape codes to be displayed as in source code.</returns>
        private static string ReverseEscapes(string format)
        {
            if (format == null)
                return "(null)";

            StringBuilder builder = new StringBuilder("\"");
            int currentIndex = 0;

            while (currentIndex < format.Length)
            {
                string escapeString = null;
                int nextEscapeIndex = format.IndexOfAny(ResolvedEscapes, currentIndex);

                if (nextEscapeIndex < 0)
                {
                    // There aren't any more.  We just need to copy the rest of the string.
                    nextEscapeIndex = format.Length; // Pretend it's just past the end, so the math below works.
                    // Leave escapeString as null, so we won't append anything for it below.
                }
                else
                {
                    // We found one of our ResolvedEscapes.  Which one?
                    char escapeChar = format[nextEscapeIndex];
                    if (EscapeMap.TryGetValue(escapeChar, out escapeString) == false)
                    {
                        // It wasn't found in the map!  Someone screwed up our mapping configuration, so we have to punt.
                        escapeString = new string(escapeChar, 1); // Copy the original char (1 time).
                    }
                }

                int length = nextEscapeIndex - currentIndex; // How long is the substring up to the next escape char?

                if (length >= 0)
                    builder.Append(format, currentIndex, length); // Copy the string up to this point.

                if (string.IsNullOrEmpty(escapeString) == false)
                    builder.Append(escapeString); // Replace the char with the corresponding string.

                currentIndex = nextEscapeIndex + 1;
            }

            builder.Append("\"");
            return builder.ToString();
        }

        /// <summary>
        /// Try to expand an object to a string, handling exceptions which might occur.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider (such as a CultureInfo).  (may be null to indicate the
        /// current culture)</param>
        /// <param name="forDisplay">The object for display into a string.</param>
        /// <param name="reverseEscapes">Whether to convert null and strings back to appearance as in code.</param>
        /// <returns>The best effort at representing the given object as a string.</returns>
        private static string SafeToString(IFormatProvider formatProvider, object forDisplay, bool reverseEscapes)
        {
            StringBuilder builder = new StringBuilder();

            Exception displayException = forDisplay as Exception;
            Exception expansionException = null;
            try
            {
                if (reverseEscapes && (forDisplay == null || forDisplay.GetType() == typeof (string)))
                {
                    builder.Append(ReverseEscapes((string) forDisplay)); // Special handling of strings and nulls requested.
                }
                else if (displayException == null)
                {
                    // forDisplay was not an exception, so do a generic format.
                    builder.AppendFormat(formatProvider, "{0}", forDisplay); // Try to format the object by formatProvider.
                }
                else
                {
                    // forDisplay was an exception type, use a helpful two-line format.
                    builder.AppendFormat(formatProvider, "{0}\r\nException Message ",
                                         displayException.GetType().FullName);
                    // This is separate so that the text is set up in case Message throws an exception here.
                    builder.AppendFormat(formatProvider, "= {0}", displayException.Message);
                }
            }
            catch (Exception ex)
            {
                // Catch all exceptions.
                expansionException = ex;
            }

            if (expansionException != null)
            {
                try
                {
                    builder.AppendFormat(formatProvider, "<<<{0} error converting to string>>> : ",
                                         expansionException.GetType().FullName);
                    builder.Append(expansionException.Message);
                }
                catch
                {
                    // An exception accessing the exception?  Wow.  That should not be possible.  Well, just punt.
                    builder.Append("<<<Error accessing exception message>>>");
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}