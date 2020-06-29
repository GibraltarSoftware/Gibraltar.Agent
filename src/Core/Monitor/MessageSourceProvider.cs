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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// A basic class to determine the source of a log message and act as an IMessageSourceProvider. 
    /// </summary>
    /// <remarks>This class knows how to acquire information about the source of a log message from the current call stack,
    /// and acts as a IMessageSourceProvider to use when handing off a log message to the central Log.
    /// Thus, this object must be created while still within the same call stack as the origination of the log message.
    /// Used internally by our Log.EndFile() method and ExceptionListener (etc).</remarks>
    public class MessageSourceProvider : IMessageSourceProvider
    {
        private string m_MethodName;
        private string m_ClassName;
        private string m_FileName;
        private int m_LineNumber;
        private string m_FormattedStackTrace;
        private int m_SelectedStackFrame;

        /// <summary>
        /// Parameterless constructor for derived classes.
        /// </summary>
        protected MessageSourceProvider()
        {
            m_MethodName = null;
            m_ClassName = null;
            m_FileName = null;
            m_LineNumber = 0;
        }

        /// <summary>
        /// Creates a MessageSourceProvider object to be used as an IMessageSourceProvider.
        /// </summary>
        /// <param name="className">The full name of the class (with namespace) whose method issued the log message.</param>
        /// <param name="methodName">The simple name of the method which issued the log message.</param>
        /// <remarks>This constructor is used only for the convenience of the Log class when it needs to generate
        /// an IMessageSourceProvider for construction of internally-generated packets without going through the
        /// usual direct PublishToLog() mechanism.</remarks>
        public MessageSourceProvider(string className, string methodName)
        {
            m_MethodName = methodName;
            m_ClassName = className;
            m_FileName = null;
            m_LineNumber = 0;
        }

        /// <summary>
        /// Creates a MessageSourceProvider object to be used as an IMessageSourceProvider.
        /// </summary>
        /// <param name="className">The full name of the class (with namespace) whose method issued the log message.</param>
        /// <param name="methodName">The simple name of the method which issued the log message.</param>
        /// <param name="fileName">The name of the file containing the method which issued the log message.</param>
        /// <param name="lineNumber">The line within the file at which the log message was issued.</param>
        /// <remarks>This constructor is used only for the convenience of the Log class when it needs to generate
        /// an IMessageSourceProvider for construction of internally-generated packets without going through the
        /// usual direct PublishToLog() mechanism.</remarks>
        public MessageSourceProvider(string className, string methodName, string fileName, int lineNumber)
        {
            m_MethodName = methodName;
            m_ClassName = className;
            m_FileName = fileName;
            m_LineNumber = lineNumber;
        }

        /// <summary>
        /// Creates a MessageSourceProvider object to be used as an IMessageSourceProvider.
        /// </summary>
        /// <remarks>This constructor is used only for the convenience of the Log class when it needs to generate
        /// an IMessageSourceProvider for construction of internally-generated packets without going through the
        /// usual direct PublishToLog() mechanism.</remarks>
        /// <param name="skipFrames">The number of stack frames to skip over to find the first candidate to be
        /// identified as the source of the log message.</param>
        /// <param name="localOrigin">True if logging a message originating in Gibraltar code.
        /// False if logging a message from the client application.</param>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public MessageSourceProvider(int skipFrames, bool localOrigin)
        {
            const bool trustSkipFrames = true; // Set true to trust skipFrames count and don't skip over Gibraltar libs.

// ReSharper disable ConditionIsAlwaysTrueOrFalse
            m_SelectedStackFrame = CommonCentralLogic.FindMessageSource(skipFrames + 1, localOrigin || trustSkipFrames, null,
                                              out m_ClassName, out m_MethodName, out m_FileName, out m_LineNumber);
// ReSharper restore ConditionIsAlwaysTrueOrFalse

            //FindMessageSource(skipFrames + 1, localOrigin, null);
        }

        /// <summary>
        /// The full adjusted stack trace from the bottom of the stack up through the frame we're attributing this message to
        /// </summary>
        public string StackTrace
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                lock (this)
                {
                    if (string.IsNullOrEmpty(m_FormattedStackTrace))
                    {
                        try
                        {
                            //we are one stack frame higher than FindMessageSource.
                            m_FormattedStackTrace = new StackTrace(m_SelectedStackFrame + 1, true).ToString();
                        }
                        catch
                        {
                            m_FormattedStackTrace = null;
                        }
                    }
                }
                return m_FormattedStackTrace;
            }
        }

        /* This logic moved into CommonFileTools....
        /// <summary>
        /// Extracts needed message source information from the current call stack.
        /// </summary>
        /// <remarks>This is used internally to perform the actual stack frame walk.  Constructors for derived classes
        /// all call this method.  This constructor also allows the caller to specify a log message as being
        /// of local origin, so Gibraltar stack frames will not be automatically skipped over when determining
        /// the originator for internally-issued log messages.</remarks>
        /// <param name="skipFrames">The number of stack frames to skip over to find the first candidate to be
        /// identified as the source of the log message.</param>
        /// <param name="localOrigin">True if logging a message originating in Gibraltar code.
        /// False if logging a message from the client application.</param>
        /// <param name="exception">An exception associated with this log message (or null for none).</param>
        protected void FindMessageSource(int skipFrames, bool localOrigin, Exception exception)
        {
            const bool trustSkipFrames = true; // Set true to trust skipFrames count and don't skip over Gibraltar libs.
#if DEBUG
            // ToDo: These are to play with trying to get method signature info and perhaps even local variable state
            // as future feature enhancements.  But not for Beta 2, so remove them from production code for now.
            // There would be a performance hit to doing these, so they should probably be client-configurable.
            ParameterInfo[] parameters = null;
            MethodBody body = null;
#endif

            try
            {
                StackFrame frame = null;
                StackFrame firstSystem = null;
                StackFrame newFrame;
                MethodBase method;
                string frameModule;

                // We use skipFrames+1 here so that callers can pass in 0 to designate themselves,
                // rather than have to know to start with 1.
                int frameIndex = skipFrames + 1; // Start with the most likely candidate.
                while (true)
                {
                    //careful:  We may be out of frames, in which case we're going to stop, hopefully without an exception.
                    try
                    {
                        newFrame = new StackFrame(frameIndex, true);
                        frameIndex++; // Do this here so any continue statement below in this loop will be okay.

                        method = newFrame.GetMethod(); // This should be safe, constructors can't return null.
                        if (method == null) // But the method we found might be null (if the frame is invalid?)
                        {
                            break; // We're presumably off the end of the stack, bail out of the loop!
                        }
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
                            // And if it's something that called into system libs (eg. Trace), take that regardless.

                            // Notice that we disable some warnings here because this code is deliberately
                            // left in, but may be disabled by trustSkipFrames = true; However, we want this
                            // logic to be easily reenabled by trustSkipFrames = false, hence leaving it in.
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (localOrigin || (firstSystem != null) || trustSkipFrames)
                                break;
                            // ReSharper restore ConditionIsAlwaysTrueOrFalse

#pragma warning disable 162
                            if (frameModule.Equals("Gibraltar.dll") == false &&
                                frameModule.Equals("Gibraltar.Core.dll") == false)
                            {
                                // This is the first frame which is not in our known ecosystem,
                                // so this must be the client code calling us.
                                break; // We found it!  Break out of the loop.
                            }
#pragma warning restore 162
                        }
                    }
                    catch
                    {
                        // Hmmm, we got some sort of failure which we didn't know enough to prevent?
                        // We could comment on that... but we can't do logging here, it gets recursive!
                        // So use our safe breakpoint to alert a debugging user.  This is ignored in production.
                        Log.DebugBreak(); // Stop the debugger here (if it's running, otherwise we won't alert on it).

                        // Well, whatever we found - that's where we are.  We have to give up our search.
                        break;
                    }


                    // Remember, frameIndex was already incremented near the top of the loop
                    if (frameIndex > 200) // Note: We're assuming stacks can never be this deep (without finding our target)
                    {
                        // Maybe we messed up our failure-detection, so to prevent an infinite loop from hanging the application...
                        Log.DebugBreak(); // Stop the debugger here (if it's running).  This shouldn't ever be hit.

                        break; // Okay, it's just not sensible for stack to be so deep, so let's give up.
                    }
                }

                method = (frame == null) ? null : frame.GetMethod(); // Make sure these are in sync!
                if (method == null)
                {
                    frame = firstSystem; // Use that first system frame we found if no later candidate arose.
                    method = (frame == null) ? null : frame.GetMethod();
                }

                // Now that we've selected the best possible frame, we need to make sure we really found one.
                if (method == null)
                {
                    // Ack! We got nothing!  Invalidate all of these which depend on it and are thus meaningless.
                    m_MethodName = null;
                    m_ClassName = null;
                    m_FileName = null;
                    m_LineNumber = 0;
                }
                else
                {
                    // Whew, we found a valid method to attribute this message to.  Get the details safely....
                    try
                    {
                        // MethodBase method = frame.GetMethod();
                        m_ClassName = method.DeclaringType.FullName;
                        m_MethodName = method.Name;
#if DEBUG
                        parameters = method.GetParameters();
                        body = method.GetMethodBody();
#endif
                    }
                    catch
                    {
                        m_MethodName = null;
                        m_ClassName = null;
#if DEBUG
                        parameters = null;
                        body = null;
#endif
                    }

                    try
                    {
                        //now see if we have file information
                        m_FileName = frame.GetFileName();
                        if (string.IsNullOrEmpty(m_FileName) == false)
                        {
                            // m_FileName = Path.GetFileName(m_FileName); // Drops full path... but we want that info!
                            m_LineNumber = frame.GetFileLineNumber();
                        }
                        else
                        {
                            m_LineNumber = 0; // Not meaningful if there's no file name!
                        }
                    }
                    catch
                    {
                        m_FileName = null;
                        m_LineNumber = 0;
                    }
                }
            }
            catch
            {
                // Bleagh!  We got an unexpected failure (not caught and handled by a lower catch block as being expected).
                Log.DebugBreak(); // Stop the debugger here (if it's running, otherwise we won't alert on it).

                m_MethodName = null;
                m_ClassName = null;
                m_FileName = null;
                m_LineNumber = 0;
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
        }
        */

        #region IMessageSourceProvider properties

        /// <summary>
        /// The simple name of the method which issued the log message.
        /// </summary>
        public string MethodName { get { return m_MethodName; } }

        /// <summary>
        /// The full name of the class (with namespace) whose method issued the log message.
        /// </summary>
        public string ClassName { get { return m_ClassName; } }

        /// <summary>
        /// The name of the file containing the method which issued the log message.
        /// </summary>
        public string FileName { get { return m_FileName; } }

        /// <summary>
        /// The line within the file at which the log message was issued.
        /// </summary>
        public int LineNumber { get { return m_LineNumber; } }

        #endregion
    }
}
