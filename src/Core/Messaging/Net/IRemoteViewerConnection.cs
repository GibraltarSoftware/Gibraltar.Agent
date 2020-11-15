
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/
using System;
using System.ComponentModel;
using Gibraltar.Data;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// Provides an interface to send network packets to the remote computer
    /// </summary>
    public interface IRemoteViewerConnection : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised each time a new log message is available.
        /// </summary>
        event MessageAvailableEventHandler MessageAvailable;

        /// <summary>
        /// Indicates whether a session had errors during rehydration and has lost some packets.
        /// </summary>
        bool HasCorruptData { get; }

        /// <summary>
        /// Indicates how many packets were lost due to errors in rehydration.
        /// </summary>
        int PacketsLostCount { get; }

        /// <summary>
        /// Indicates if the remote viewer is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The session id
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Attempt to connect the live session data stream
        /// </summary>
        void Connect();

        /// <summary>
        /// Sends a request to the remote agent to package and submit its data
        /// </summary>
        /// <param name="criteria"></param>
        void SendToServer(SessionCriteria criteria);

        /// <summary>
        /// Load the set of log messages still in the connection buffer
        /// </summary>
        /// <returns></returns>
        ILogMessage[] GetMessageBuffer();
    }

    /// <summary>
    /// The event arguments for the RemoteCommandWriter MessageAvailable event
    /// </summary>
    public class MessageAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new log message event
        /// </summary>
        /// <param name="message"></param>
        public MessageAvailableEventArgs(ILogMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// The message that is now available.
        /// </summary>
        public ILogMessage Message { get; private set; }
    }

    /// <summary>
    /// The delegate for the RemoteCommandWriter MessageAvailable event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MessageAvailableEventHandler(object sender, MessageAvailableEventArgs e);
}
