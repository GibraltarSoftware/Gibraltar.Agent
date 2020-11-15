
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Gibraltar
{
    /// <summary>
    /// A static class to hold central logic for common file and OS operations needed by various projects.
    /// </summary>
    public static class CommonCentralLogic
    {
        private static bool g_MonoRuntime = CheckForMono(); // Are we running in Mono or full .NET CLR?
        private static bool s_SilentMode = false;
        volatile private static bool s_BreakPointEnable = false; // Can be changed in the debugger

        // Basic log implementation.
        volatile private static bool g_SessionEnding; // Session end triggered. False until set to true.
        volatile private static bool g_SessionEnded; // Session end completed. False until set to true.

        /// <summary>
        /// Indicates if the process is running under the Mono runtime or the full .NET CLR.
        /// </summary>
        public static bool IsMonoRuntime { get { return g_MonoRuntime; } }

        /// <summary>
        /// Indicates if the logging system should be running in silent mode (for example when running in the agent).
        /// </summary>
        public static bool SilentMode
        {
            get { return s_SilentMode; }
            set { s_SilentMode = value; }
        }

        /// <summary>
        /// A temporary flag to tell us whether to invoke a Debugger.Break() when Log.DebugBreak() is called.
        /// </summary>
        /// <remarks>True enables breakpointing, false disables.  This should probably be replaced with an enum
        /// to support multiple modes, assuming the basic usage works out.</remarks>
        public static bool BreakPointEnable
        {
            get { return s_BreakPointEnable; }
            set { s_BreakPointEnable = value; }
        }

        /// <summary>
        /// Reports whether EndSession() has been called to formally end the session.
        /// </summary>
        public static bool IsSessionEnding { get { return g_SessionEnding; } }

        /// <summary>
        /// Reports whether EndSession() has completed flushing the end-session command to the log.
        /// </summary>
        public static bool IsSessionEnded { get { return g_SessionEnded; } }

        /// <summary>
        /// Sets the SessionEnding flag to true.  (Can't be reversed once set.)
        /// </summary>
        public static void DeclareSessionIsEnding()
        {
            g_SessionEnding = true;
        }

        /// <summary>
        /// Sets the SessionHasEnded flag to true.  (Can't be reversed once set.)
        /// </summary>
        public static void DeclareSessionHasEnded()
        {
            g_SessionEnding = true; // This must also be set to true before it can be ended.
            g_SessionEnded = true;
        }

        /// <summary>
        /// Automatically stop debugger like a breakpoint, if enabled.
        /// </summary>
        /// <remarks>This will check the state of Log.BreakPointEnable and whether a debugger is attached,
        /// and will breakpoint only if both are true.  This should probably be extended to handle additional
        /// configuration options using an enum, assuming the basic usage works out.  This method is conditional
        /// upon a DEBUG build and will be safely ignored in release builds, so it is not necessary to wrap calls
        /// to this method in #if DEBUG (acts much like Debug class methods).</remarks>
        [Conditional("DEBUG")]
        public static void DebugBreak()
        {
            if (s_BreakPointEnable && Debugger.IsAttached)
            {
                Debugger.Break(); // Stop here only when debugging
                // ...then Shift-F11 to step out to where it is getting called...
            }
        }

        /// <summary>
        /// Calculate the right URL to navigate to the specified web page on the public Gibraltar web site.
        /// </summary>
        /// <param name="pageFileName">The simple page file name (including ".aspx" extension).</param>
        /// <param name="product">The name of the requesting app's product group.</param>
        /// <param name="application">The name of the requesting application.</param>
        /// <param name="version">The Version of the requesting application.</param>
        /// <returns>The fully qualified URL</returns>
        public static string CalculateWebUrl(string pageFileName, string product, string application, Version version)
        {
            //format up the full URL
            string fullUrl = string.Format("http://www.gibraltarsoftware.com/external/{0}?productName={1}&applicationName={2}&majorVer={3}&minorVer={4}&build={5}&revisionVer={6}",
                pageFileName, product, application, version.Major, version.Minor, version.Build, version.Revision);

            return fullUrl;
        }

        /// <summary>
        /// Attempt to navigate to the specified web page on the public Gibraltar web site.
        /// </summary>
        /// <param name="pageFileName">The simple page file name (including ".aspx" extension).</param>
        /// <param name="product">The name of the requesting app's product group.</param>
        /// <param name="application">The name of the requesting application.</param>
        /// <param name="version">The Version of the requesting application.</param>
        public static void DisplayWebPage(string pageFileName, string product, string application, Version version)
        {
            string fullUrl = CalculateWebUrl(pageFileName, product, application, version);
            try
            {
                Process.Start(fullUrl);
            }
            catch(Exception)
            {
                MessageBox.Show("Sorry, we couldn't directly open the web page on your system.  If you'd like, open a browser and go to:\r\n" + fullUrl, "Unable to open web browser", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Check whether we are running in a Mono runtime environment rather than a normal .NET CLR.
        /// </summary>
        /// <returns>True if running in Mono.  False if .NET CLR.</returns>
        private static bool CheckForMono()
        {
            Type monoRuntime = Type.GetType("Mono.Runtime"); // Detect if we're running under Mono runtime.
            bool isMonoRuntime = (monoRuntime != null); // We'll cache the result so we don't have to waste time checking again.

            return isMonoRuntime;
        }

        /// <summary>
        /// Extracts needed message source information from the current call stack.
        /// </summary>
        /// <remarks>This is used internally to perform the actual stack frame walk.  Constructors for derived classes
        /// all call this method.  This constructor also allows the caller to specify a log message as being
        /// of local origin, so Gibraltar stack frames will not be automatically skipped over when determining
        /// the originator for internally-issued log messages.</remarks>
        /// <param name="skipFrames">The number of stack frames to skip over to find the first candidate to be
        /// identified as the source of the log message.  (Should generally use 0 if exception parameter is not null.)</param>
        /// <param name="trustSkipFrames">True if logging a message originating in Gibraltar code (or to just trust skipFrames).
        /// False if logging a message from the client application and Gibraltar frames should be explicitly skipped over.</param>
        /// <param name="exception">An exception declared as the source of this log message (or null for normal call stack source).</param>
        /// <param name="className">The class name of the identified source (usually available).</param>
        /// <param name="methodName">The method name of the identified source (usually available).</param>
        /// <param name="fileName">The file name of the identified source (if available).</param>
        /// <param name="lineNumber">The line number of the identified source (if available).</param>
        /// <returns>The index of the stack frame chosen</returns>
        public static int FindMessageSource(int skipFrames, bool trustSkipFrames, Exception exception, out string className,
                                             out string methodName, out string fileName, out int lineNumber)
        {
#if DEBUG
            // ToDo: These are to play with trying to get method signature info and perhaps even local variable state
            // as future feature enhancements.  But not for Beta 2, so remove them from production code for now.
            // There would be a performance hit to doing these, so they should probably be client-configurable.
            ParameterInfo[] parameters = null;
            MethodBody body = null;
#endif
            int selectedFrame = -1;
            try
            {
                // We use skipFrames+1 here so that callers can pass in 0 to designate themselves rather than have to know to start with 1.
                // But for an exception stack trace, we didn't get added to the stack, so don't add anything in that case.
                StackTrace stackTrace = (exception == null) ? new StackTrace(skipFrames + 1, true) 
                    : new StackTrace(exception, skipFrames, true);
                StackFrame frame = null;
                StackFrame firstSystem = null;
                StackFrame newFrame;
                MethodBase method = null;
                string frameModule;

                int frameIndex = 0; // we already accounted for skip frames in getting the stack trace
                while (true)
                {
                    // Careful:  We may be out of frames, in which case we're going to stop, hopefully without an exception.
                    try
                    {
                        newFrame = stackTrace.GetFrame(frameIndex);
                        frameIndex++; // Do this here so any continue statement below in this loop will be okay.
                        if (newFrame == null) // Not sure if this check is actually needed, but it doesn't hurt.
                            break; // We're presumably off the end of the stack, bail out of the loop!

                        method = newFrame.GetMethod();
                        if (method == null) // The method we found might be null (if the frame is invalid?).
                            break; // We're presumably off the end of the stack, bail out of the loop!

                        frameModule = method.Module.Name;

                        if (frameModule.Equals("System.dll") || frameModule.Equals("mscorlib.dll"))
                        {
                            // Ahhh, a frame in the system libs... Next non-system frame will be our pick!
                            if (firstSystem == null) // ...unless we find no better candidate, so remember the first one.
                            {
                                firstSystem = newFrame;
                            }

                        }
                        else
                        {
                            frame = newFrame; // New one is valid, and not system, so update our chosen frame to use it.
                            // We already got its corresponding method, above, to validate the module.

                            // Okay, it's not in the system libs, so it might be a good candidate,
                            // but do we need to filter out Gibraltar or is this a deliberate local invocation?
                            // And if it's something that called into system libs (e.g. Trace), take that regardless.

                            if (trustSkipFrames || (firstSystem != null))
                                break;

                            if (frameModule.Equals("Gibraltar.Agent.dll") == false &&
                                frameModule.Equals("Gibraltar.Core.dll") == false)
                            {
                                // This is the first frame which is not in our known ecosystem,
                                // so this must be the client code calling us.
                                break; // We found it!  Break out of the loop.
                            }
                        }
                    }
                    catch
                    {
                        // Hmmm, we got some sort of failure which we didn't know enough to prevent?
                        // We could comment on that... but we can't do logging here, it gets recursive!
                        // So use our safe breakpoint to alert a debugging user.  This is ignored in production.
                        DebugBreak(); // Stop the debugger here (if it's running, otherwise we won't alert on it).

                        // Well, whatever we found - that's where we are.  We have to give up our search.
                        break;
                    }

                    method = null; // Invalidate it for the next loop.

                    // Remember, frameIndex was already incremented near the top of the loop
                    if (frameIndex > 200) // Note: We're assuming stacks can never be this deep (without finding our target)
                    {
                        // Maybe we messed up our failure-detection, so to prevent an infinite loop from hanging the application...
                        DebugBreak(); // Stop the debugger here (if it's running).  This shouldn't ever be hit.

                        break; // Okay, it's just not sensible for stack to be so deep, so let's give up.
                    }
                }

                if (frame == null || method == null)
                {
                    frame = stackTrace.GetFrame(0); // If we went off the end, go back to the first frame (after skipFrames).
                    selectedFrame = 0;
                }
                else
                {
                    selectedFrame = frameIndex;
                }

                method = (frame == null) ? null : frame.GetMethod(); // Make sure these are in sync!
                if (method == null)
                {
                    frame = firstSystem; // Use that first system frame we found if no later candidate arose.
                    method = (frame == null) ? null : frame.GetMethod();
                }

                // Now that we've selected the best possible frame, we need to make sure we really found one.
                if (method != null)
                {
                    // Whew, we found a valid method to attribute this message to.  Get the details safely....
                    className = method.DeclaringType == null ? null : method.DeclaringType.FullName;
                    methodName = method.Name;
#if DEBUG
                    parameters = method.GetParameters();
                    body = method.GetMethodBody();
#endif

                    try
                    {
                        //now see if we have file information
                        fileName = frame.GetFileName();
                        if (string.IsNullOrEmpty(fileName) == false)
                        {
                            // m_FileName = Path.GetFileName(m_FileName); // Drops full path... but we want that info!
                            lineNumber = frame.GetFileLineNumber();
                        }
                        else
                            lineNumber = 0; // Not meaningful if there's no file name!
                    }
                    catch
                    {
                        fileName = null;
                        lineNumber = 0;
                    }
                }
                else
                {
                    // Ack! We got nothing!  Invalidate all of these which depend on it and are thus meaningless.
                    methodName = null;
                    className = null;
                    fileName = null;
                    lineNumber = 0;
                }
            }
            catch
            {
                // Bleagh!  We got an unexpected failure (not caught and handled by a lower catch block as being expected).
                DebugBreak(); // Stop the debugger here (if it's running, otherwise we won't alert on it).

                methodName = null;
                className = null;
                fileName = null;
                lineNumber = 0;
#if DEBUG
                parameters = null;
                body = null;
#endif
            }

#if DEBUG
            if (!ReferenceEquals(body, null) && !ReferenceEquals(parameters, null))
            {
                // Hey, wow, this stuff could be handy!
                GC.KeepAlive(body);
                GC.KeepAlive(parameters);
            }
#endif
            return selectedFrame;
        }
    }
}
