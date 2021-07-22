

using System;
using System.ComponentModel;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Event arguments for the CanConnectCompleted event.
    /// </summary>
    public class HubConnectionCanConnectEventArgs: AsyncCompletedEventArgs
    {
        internal HubConnectionCanConnectEventArgs(Exception error, bool cancelled, HubStatus? status, string message, bool isValid)
            :base(error, cancelled, null)
        {
            Status = status;
            Message = message;
            IsValid = isValid;
        }

        /// <summary>
        /// The status of the connection
        /// </summary>
        public HubStatus? Status { get; private set; }

        /// <summary>
        /// A descriptive message of the connection status
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Indicates if the connection was successful
        /// </summary>
        public bool IsValid { get; private set; }
    }

    /// <summary>
    /// Delegate for the CanConnectCompleted event
    /// </summary>
    /// <param name="state"></param>
    /// <param name="e"></param>
    public delegate void HubConnectionCanConnectEventHandler(object state, HubConnectionCanConnectEventArgs e);
}
