

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Loupe.Extensibility.Data;


namespace Gibraltar.Monitor.Net
{
    /// <summary>
    /// Listens for standard and error console output and redirects to the session file.
    /// </summary>
    public class ConsoleListener : TextWriter
    {
        private const string OutCategoryName = "Console.Out";
        private const string ErrorCategoryName = "Console.Error";
        private const string ConsoleLogSystem = "Console";

        private readonly string m_OutputCategory;
        private readonly TextWriter m_OriginalWriter;
        private readonly Encoding m_Encoding;
        private readonly StringBuilder m_Buffer;

        /// <summary>
        /// Create a new instance of the console listener.
        /// </summary>
        /// <param name="outputCategory"></param>
        /// <param name="originalWriter"></param>
        public ConsoleListener(string outputCategory, TextWriter originalWriter)
            : base(CultureInfo.InvariantCulture) // For now, tell inherited TextWriter to use InvariantCulture
        {
            m_OutputCategory = outputCategory;
            m_OriginalWriter = originalWriter;
            m_Encoding = originalWriter.Encoding;
            m_Buffer = new StringBuilder();
        }


        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Text.Encoding"/> in which the output is written.
        /// </summary>
        /// <returns>
        /// The Encoding in which the output is written.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override Encoding Encoding { get { return m_Encoding; } }


        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream. 
        ///                 </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. 
        ///                 </exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><filterpriority>1</filterpriority>
        /// <remarks>
        /// TODO: At some point we want to overhaul this to override at the Write(string) and WriteLine() level instead
        /// of Write(char) and pass the data in a different, stream-optimized format rather than as log messages.
        /// But for now... Wrap as a log message for each line (as we get a newline)....
        /// </remarks>
        public override void Write(char value)
        {
            m_OriginalWriter.Write(value);
            if (value == '\n')
            {
                if (m_Buffer.Length > 0)
                {
#if STACK_DUMP
                        Exception dumpException = new GibraltarStackInfoException(m_OutputCategory + " - Write()", null);
#else
                    const Exception dumpException = null;
#endif
                    SimpleLogMessage logMessage = new SimpleLogMessage(LogMessageSeverity.Verbose, LogWriteMode.Queued,
                                                                       ConsoleLogSystem, m_OutputCategory, 3,
                                                                       dumpException, m_Buffer.ToString());
                    m_Buffer.Length = 0;
                    logMessage.PublishToLog();
                }
            }
            else if (value != '\r')
                m_Buffer.Append(value);
            /* else
                m_buffer.Append("<CR>"); */
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Flush()
        {
            base.Flush(); // Flush our base to push any pending writes to us?
            m_OriginalWriter.Flush(); // Then flush the original writer for whatever we passed on to it.

            //m_buffer.Append("<Flush>");

            // Now should we go ahead and push any partial lines in a log message?
            if (m_Buffer.Length > 0)
            {
#if STACK_DUMP
                    Exception dumpException = new GibraltarStackInfoException(m_OutputCategory + " - Flush()", null);
#else
                const Exception dumpException = null;
#endif
                SimpleLogMessage logMessage = new SimpleLogMessage(LogMessageSeverity.Verbose, LogWriteMode.Queued,
                                                                   ConsoleLogSystem, m_OutputCategory, 1,
                                                                   dumpException, m_Buffer.ToString());
                m_Buffer.Length = 0;
                logMessage.PublishToLog();
            }
        }

        /// <summary>
        /// Tries to find the internal sync object used by the Console class to thread-safe changes to Out and Error.
        /// </summary>
        /// <remarks>This method uses reflection to attempt to dig into the Console class for its InternalSyncObject.</remarks>
        /// <returns>The lock object, or null if it could not be obtained.</returns>
        private static object GetConsoleLockObject()
        {
            object lockObject;

            try
            {
                // Console class uses InternalSyncObject property to get (and initialize, if needed) a lock object
                // which is used to protect writes to the actual Out and Error backing store fields and protects
                // initialization of each of these upon first access.  However, the lock object does NOT protect
                // reads of Out and Error after each is initialized.  This is probably okay, because we're mostly
                // worried about other changes to Out or Error in between our own read and write attempts; by doing
                // our read-and-replace inside a lock on Console's InternalSyncObject, we make sure any other changes
                // occur before our read or after our write, making our replacement operation reasonably atomic.

                const BindingFlags bindingFlags = BindingFlags.GetProperty | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic;
                Type consoleType = typeof(Console);

                lockObject = consoleType.InvokeMember("InternalSyncObject", bindingFlags, null, null, null);
            }
            catch
            {
                lockObject = null;
            }

            return lockObject;
        }

        /// <summary>
        /// Registers new ConsoleIntercepter on Console.Out and Console.Error.
        /// </summary>
        /// <remarks>This attempts to get the Console's InternalSyncObject to protect the operations as atomic,
        /// but will make a best-effort to do them even if the lock object could not be obtained.</remarks>
        public static void RegisterConsoleIntercepter()
        {
            // We need to save the existing Out and Error and set up to write through to them so we don't alter
            // the apparent behavior!  We try to find the Console's lock object to make them atomic replacements.
            object lockObject = GetConsoleLockObject();
            if (lockObject != null)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Information, "Gibraltar.Agent", "Registering ConsoleIntercepter as atomic replacements", "Obtained Console's InternalSyncObject through reflection.");
#endif
                lock (lockObject)
                {
                    Console.SetOut(new ConsoleListener(OutCategoryName, Console.Out));
                    Console.SetError(new ConsoleListener(ErrorCategoryName, Console.Error));
                }
            }
            else
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, "Gibraltar.Agent", "Registering ConsoleIntercepter without lock protection", "Unable to obtain Console's InternalSyncObject through reflection.");
#endif
                Console.SetOut(new ConsoleListener(OutCategoryName, Console.Out));
                Console.SetError(new ConsoleListener(ErrorCategoryName, Console.Error));
            }
        }
    }

}
