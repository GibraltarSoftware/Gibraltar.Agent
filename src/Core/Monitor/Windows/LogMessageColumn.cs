
namespace Gibraltar.Monitor.Windows
{
    /// <summary>
    /// Columns within the live viewer
    /// </summary>
    public enum LogMessageColumn
    {
        /// <summary>
        /// Unique sequence number of the log message.
        /// </summary>
        Sequence = 0,

        /// <summary>
        /// The LogMessageSeverity of the log message (represented as an icon)
        /// </summary>
        Severity = 1,

        /// <summary>
        /// The date &amp; time of the log message
        /// </summary>
        Timestamp = 2,

        /// <summary>
        /// The text of the log message
        /// </summary>
        Caption = 3,

        /// <summary>
        /// The name of the thread that logged the message
        /// </summary>
        Thread = 4,

        /// <summary>
        /// The class and method name that logged the message.
        /// </summary>
        Method = 5,

        /// <summary>
        /// The file name and line number where the message was generated (for debug builds only)
        /// </summary>
        SourceCodeLocation = 6,

        /// <summary>
        /// The user identity associated with the message
        /// </summary>
        UserName = 7
    }
}
