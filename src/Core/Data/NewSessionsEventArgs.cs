
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
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Data
{
    /// <summary>
    /// Supplies summary information about new sessions that are available to be retrieved or just retrieved into the repository
    /// </summary>
    public class NewSessionsEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new sessions event arguments container
        /// </summary>
        /// <param name="newSessions"></param>
        /// <param name="warningSessions"></param>
        /// <param name="errorSessions"></param>
        /// <param name="criticalSessions"></param>
        /// <param name="maxSeverity"></param>
        public NewSessionsEventArgs(int newSessions, int warningSessions, int errorSessions, int criticalSessions, LogMessageSeverity maxSeverity)
        {
            NewSessions = newSessions;
            WarningSessions = warningSessions;
            ErrorSessions = errorSessions;
            CriticalSessions = criticalSessions;
            MaxSeverity = maxSeverity;
        }

        /// <summary>
        /// The number of new sessions affected
        /// </summary>
        public int NewSessions { get; private set; }

        /// <summary>
        /// The number of new sessions with a max severity of warning.
        /// </summary>
        public int WarningSessions { get; private set; }

        /// <summary>
        /// The number of new sessions with a max severity of error.
        /// </summary>
        public int ErrorSessions { get; private set; }

        /// <summary>
        /// The number of new sessions with a max severity of critical.
        /// </summary>
        public int CriticalSessions { get; private set; }

        /// <summary>
        /// The maximum severity of new sessions.
        /// </summary>
        public LogMessageSeverity MaxSeverity { get; private set; }
    }

    /// <summary>
    /// An event handler for the New Sessions Event Arguments
    /// </summary>
    /// <param name="state"></param>
    /// <param name="e"></param>
    public delegate void NewSessionsEventHandler(object state, NewSessionsEventArgs e); 
}
