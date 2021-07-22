
namespace Gibraltar.Messaging
{
    /// <summary>
    /// Standard configuration class for messengers.
    /// </summary>
    /// <remarks>Individual messengers may extend this class with additional configuration information.</remarks>
    public class MessengerConfiguration
    {
        private string m_Name;
        private string m_MessengerType;

        /// <summary>
        /// Create a new messenger configuration object with the provided unique name and specified messenger type.
        /// </summary>
        /// <param name="name">A unique name for this messenger.</param>
        /// <param name="messengerType">The .NET type name of the messenger object.</param>
        public MessengerConfiguration(string name, string messengerType)
        {
            m_Name = name;
            m_MessengerType = messengerType;
        }

        /// <summary>
        /// A unique name for this messenger in the active messengers collection
        /// </summary>
        public string Name { get { return m_Name; } }

        /// <summary>
        /// The type name of the class that implements the messenger.
        /// </summary>
        public string MessengerType { get { return m_MessengerType; } }

        /// <summary>
        /// When true, the messenger will treat all write requests as write-through requests.
        /// </summary>
        /// <remarks>This overrides the write through request flag for all published requests, acting
        /// as if they are set true.  This will slow down logging and change the degree of parallelism of 
        /// multithreaded applications since each log message will block until it is committed to every
        /// configured messenger.</remarks>
        public bool ForceSynchronous { get; set; }

        /// <summary>
        /// The maximum number of queued messages waiting to be processed by the messenger
        /// </summary>
        /// <remarks>Once the total number of messages waiting to be processed exceeds the
        /// maximum queue length the messenger will switch to a synchronous mode to 
        /// catch up.  This will not cause the client to experience synchronous logging
        /// behavior unless the publisher queue is also filled.</remarks>
        public int MaxQueueLength { get; set; }

        /// <summary>
        /// When false, the messenger is disabled even if otherwise configured.
        /// </summary>
        /// <remarks>This allows for explicit disable/enable without removing the existing configuration
        /// or worrying about the default configuration.</remarks>
        public bool Enabled { get; set; }
    }
}
