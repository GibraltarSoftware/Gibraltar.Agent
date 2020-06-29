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

#if MESSENGER_HOTKEY
using System;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Monitor.Windows.Internal;
#endif
using System.Collections.Generic;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Internal;
using Gibraltar.Monitor.Windows;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Messaging
{
    /// <summary>
    /// A Messenger to integrate Gibraltar Trace Monitor functionality into Gibraltar proper.
    /// </summary>
    internal class MonitorMessenger : MessengerBase
    {
        public const int DefaultBufferSize = 1000; // Big enough?
        private static int s_BufferSize = DefaultBufferSize;
        private static readonly List<LiveLogViewer> s_ActiveViewers = new List<LiveLogViewer>(); // LOCKED BY ViewerLock
        private static readonly Queue<ILogMessage> s_Buffer = new Queue<ILogMessage>(); // LOCKED BY ViewerLock
        private static readonly object s_ViewerLock = new object(); // Lock for ActiveViewers and Buffer.  No pulse (?).

        #region Viewer Management

        /// <summary>
        /// Register a viewer to begin receiving log messages from the Gibraltar central log.
        /// </summary>
        /// <remarks>If the buffer of previous messages is needed--to include messages prior to the viewer's
        /// creation--the GetMessageBuffer() method should be used instead, which can atomically register a
        /// viewer in a single transaction to prevent drops and duplication of messages.</remarks>
        /// <param name="viewer">The LiveLogViewer to register (a null argument is ignored without error).</param>
        internal static void Register(LiveLogViewer viewer)
        {
            if (viewer == null)
                return;

            lock (s_ViewerLock)
            {
                s_ActiveViewers.Add(viewer);
            }
        }

        /// <summary>
        /// Unregister a viewer, to stop receiving log messages from the Gibraltar central log.
        /// </summary>
        /// <remarks>Registered active viewers should call this method as an early part of their Dispose(true) logic.
        /// Until this method returns, viewers must be able to accept receipt of log messages to their queue.</remarks>
        /// <param name="viewer">The LiveLogViewer to unregister (a null argument is ignored without error).</param>
        internal static void Unregister(LiveLogViewer viewer)
        {
            if (viewer == null)
                return;

            lock (s_ViewerLock)
            {
                s_ActiveViewers.Remove(viewer); // This should not barf if it isn't found in the list for some reason.
            }
        }

        /// <summary>
        /// Get the current buffer of recent log messages, and optionally register an active viewer as an atomic transaction.
        /// </summary>
        /// <remarks><para>When registering an active viewer, the registration is lock-protected and guaranteed to
        /// take place after any dispatch of messages already in the buffer, and before any new dispatch of messages
        /// not yet in the buffer, to prevent duplicates and dropped messages.</para>
        /// <para>Because the lock must release upon return from this method--allowing new messages to be
        /// immediately sent to all registered viewers--a calling viewer which is registering itself (upon creation)
        /// should make sure to process the returned buffer BEFORE going through anything added to its queue.
        /// This will avoid the need to lock and delay dispatch of messages to other viewers while processing
        /// a backlog of messages from the buffer.</para>
        /// <para>Also see the Register() method, if the buffer of previous messages is not needed.</para></remarks>
        /// <param name="registerViewer">The LiveLogViewer to register, or null to get the buffer without registering.</param>
        /// <returns>A LogMessage array snapshot of the buffer.</returns>
        public static ILogMessage[] GetMessageBuffer(LiveLogViewer registerViewer)
        {
            lock (s_ViewerLock)
            {
                ILogMessage[] bufferCopy = s_Buffer.ToArray();
                if (registerViewer != null)
                    s_ActiveViewers.Add(registerViewer);
                return bufferCopy;
            }
        }

        private static void QueueMessageToViewers(LogMessagePacket newMessage)
        {
            // Make sure this is actually a message, not null.
            if (newMessage == null)
            {
                Log.DebugBreak(); // This shouldn't happen, and we'd like to know if it is, so stop here if debugging.

                return; // Otherwise, just return; we don't want to throw exceptions.
            }

            LiveLogViewer[] viewers;
            lock (s_ViewerLock)
            {
                // Any new viewers added after we release the lock need to see this log message in the buffer,
                // and not get it in their queue.
                
                viewers = s_ActiveViewers.ToArray(); // Snapshot the active viewers so it doesn't change on us...
                if (s_BufferSize > 0) // ToDo: This check shouldn't be needed if we only exist in a WinForms app.
                    s_Buffer.Enqueue(newMessage); // Add it to the buffer while we're still in the lock.

                while (s_Buffer.Count > s_BufferSize)
                    s_Buffer.Dequeue(); // Discard older excess.

                // ...and we can release the lock?  
            }

            foreach (LiveLogViewer viewer in viewers)
                viewer.QueueViewerMessage(newMessage);
        }

        #endregion

        #region MessengerBase Override Methods

        protected override void OnWrite(IMessengerPacket packet, bool writeThrough, ref MaintenanceModeRequest maintenanceRequested)
        {
            // We have no concept of writeThrough blocking in live viewers, so just ignore the setting.

            LogMessagePacket logMessage = packet as LogMessagePacket;
            if (logMessage != null)
                QueueMessageToViewers(logMessage); // All log message packets simply get queued to all active viewers.
            else
            {
//                ThreadInfoPacket threadInfoPacket = packet as ThreadInfoPacket;
//                if (threadInfoPacket != null)
//                    RecordThreadInfo(threadInfoPacket);
            }
            // Other packet types (eg. metrics) are not handled by live viewers, only LogMessagePacket (and ThreadInfoPacket).
        }

        /// <summary>
        /// Implements custom initialize functionality for this Messenger class.
        /// </summary>
        /// <remarks>This method will be called exactly once before any call to OnFlush or OnWrite is made.  
        /// Code in this method is protected by a Thread Lock.
        /// This method is called with the Message Dispatch thread exclusively.</remarks>
        protected override void OnInitialize(MessengerConfiguration configuration)
        {
            ViewerMessengerConfiguration viewerConfiguration = configuration as ViewerMessengerConfiguration;
            if (viewerConfiguration == null)
                return; // Hmmm, something is wrong.  Let's just bail, for now.

            lock (s_ViewerLock) // Is this really needed?  Can't really hurt to be safe....
            {
                s_BufferSize = viewerConfiguration.MaxMessages; // Get Viewer's configured MaxMessages.
            }
        }

        /// <summary>
        /// Signal the Messenger to close down.
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            Listener.OnManagerExit(); // Tell the Agent Manager UI to exit.
        }

        /// <summary>
        /// Signal the Messenger that the logging session is ending (typically on application exit).
        /// </summary>
        protected override void OnExit()
        {
            base.OnExit();

            Listener.OnManagerExit(); // Tell the Agent Manager UI to exit.
        }

        protected override void OnCommand(MessagingCommand command, object state, bool writeThrough, ref MaintenanceModeRequest maintenanceRequested)
        {
            base.OnCommand(command, state, writeThrough, ref maintenanceRequested);

            if (command == MessagingCommand.ShowLiveView)
            {
                Listener.ShowViewerForm();
            }
        }

        #endregion
    }
}
