
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// A funky helper exception class to grab a stack trace to be attached to a log message.
    /// </summary>
    /// <remarks>A new GibraltarStackInfoException will initialize its message to include a multi-line string
    /// dump of the current call stack upon creation.  Throwing this exception (see GetThrownException()) will
    /// also capture normal Exception stack-trace info, however it appears that throwing an exception just to
    /// catch it will produce a limited stack trace without the full depth desired for call-stack debugging
    /// information.  So this exception type captures it as part of the Message property, and thus does not even
    /// need to be thrown.  It is intended to be attached as a referenced exception in a log message.</remarks>
    [Serializable]
    public class GibraltarStackInfoException : Exception // Not GibraltarException, to avoid the debug-break. ?
    {
        /// <summary>
        /// Initializes a new instance of the GibraltarException class.
        /// </summary>
        /// <remarks>This constructor initializes the Message property of the new instance to a string dump
        /// of the current call stack.</remarks>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public GibraltarStackInfoException()
            : base((new StackTrace(1)).ToString())
        {
            // Just the base constructor with our default stack dump.
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarException class with a specified prefix message.
        /// </summary>
        /// <param name="message">A message string to label this exception before the stack dump.</param>
        /// <remarks>This constructor initializes the Message property of the new instance to a specified
        /// message label followed by a string dump of the current call stack.</remarks>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public GibraltarStackInfoException(string message)
            : base( (( string.IsNullOrEmpty(message) ) ? string.Empty : message + "\r\n") + new StackTrace(1) )
        {
            // Just the base constructor with our default stack dump.
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarException class with a specified prefix message
        /// and an inner exception.
        /// </summary>
        /// <param name="message">A message string to label this exception before the stack dump.</param>
        /// <param name="innerException">Another Exception to reference inside this exception (may be null).</param>
        /// <remarks>This constructor initializes the Message property of the new instance to a specified
        /// message label followed by a string dump of the current call stack.  The innerException can be
        /// used to chain other GibraltarStackInfoException stack dumps to attach the list (outer-most,
        /// most-recent at the top) to a single log message.</remarks>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public GibraltarStackInfoException(string message, Exception innerException)
            : base( (( string.IsNullOrEmpty(message) ) ? string.Empty : message + "\r\n") + new StackTrace(1),
                    innerException )
        {
            // Just the base constructor with our default stack dump.
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarException class with a specified prefix message
        /// and an inner exception.
        /// </summary>
        /// <param name="message">A message string to label this exception before the stack dump.</param>
        /// <param name="innerException">Another Exception to reference inside this exception (may be null).</param>
        /// <param name="skipFrames">The number of stack frames to leave out of the stack dump.</param>
        /// <remarks>This constructor initializes the Message property of the new instance to a specified
        /// message label followed by a string dump of the current call stack.  The optional innerException
        /// can be used to chain other GibraltarStackInfoException stack dumps to attach the list (outer-most,
        /// most-recent at the top) to a single log message.  This constructor allows skipFrames to be specified
        /// to skip over some number of stack frames in the stack dump (0 for default behavior).</remarks>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public GibraltarStackInfoException(string message, Exception innerException, int skipFrames)
            : base( (( string.IsNullOrEmpty(message) ) ? string.Empty : message + "\r\n") +
                    new StackTrace( ((skipFrames < 0) ? 0 : skipFrames) + 1 ),
                    innerException )
        {
            // Just the base constructor with our default stack dump.
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about
        /// the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about
        /// the source or destination.</param>
        /// <remarks>This constructor is called during deserialization to reconstitute the exception object
        /// transmitted over a stream.  For more information, see the base constructor in Exception.</remarks>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        protected GibraltarStackInfoException(System.Runtime.Serialization.SerializationInfo info,
                                              System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            // Just the base constructor, to deserialize the stack dump in the saved message field.
        }

        /// <summary>
        /// Throws and catches a GibraltarStackInfoException to capture the current stack trace.
        /// </summary>
        /// <returns>A GibraltarStackInfoException which has been thrown and caught.</returns>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static GibraltarStackInfoException GetThrownException(string message, Exception innerException)
        {
            try
            {
                throw new GibraltarStackInfoException(message, innerException, 1); // Should be a hard 1?
            }
            catch (GibraltarStackInfoException ex)
            {
                return ex;
            }
        }
    }
}