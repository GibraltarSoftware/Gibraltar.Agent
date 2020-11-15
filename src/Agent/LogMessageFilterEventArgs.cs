
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
 *    Copyright © 2008-2010 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/

using System;
using Gibraltar.Agent.Data;
using Gibraltar.Agent.Internal;

#endregion File Header

namespace Gibraltar.Agent
{
    /// <summary>
    /// EventArgs for Notification events.
    /// </summary>
    public class LogMessageFilterEventArgs : EventArgs
    {
        private readonly Monitor.MessageFilterEventArgs m_Event; // TODO: Change this type.
        private readonly ILogMessage m_Message;

        internal LogMessageFilterEventArgs(Monitor.MessageFilterEventArgs eventArgs)
        {
            m_Event = eventArgs;
            m_Message = ConvertMessage(m_Event.Message);
        }

        #region Public Properties and Methods

        /// <summary>
        /// A new log message received for possible display by the (LiveLogViewer) sender of this event.
        /// </summary>
        public ILogMessage Message { get { return m_Message; } }

        /// <summary>
        /// Cancel (block) this message from being displayed to users by the (LiveLogViewer) sender of this event.
        /// </summary>
        public bool Cancel { get { return m_Event.Cancel; } set { m_Event.Cancel = value; } }

        #endregion

        #region Private Properties and Methods

        private static LogMessageInfo ConvertMessage(Loupe.Extensibility.Data.ILogMessage internalMessage)
        {
            if (internalMessage == null)
                return null;

            LogMessageInfo message = new LogMessageInfo(internalMessage);

            return message;
        }

        #endregion
    }
}