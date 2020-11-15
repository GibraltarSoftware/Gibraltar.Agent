
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

#endregion

namespace Gibraltar.Agent.Windows
{
    /// <summary>
    /// EventArgs for MessageChanged events.
    /// </summary>
    /// <remarks>Contains a string Message property.</remarks>
    public sealed class MessageChangedEventArgs : EventArgs
    {
        private readonly string m_Message;

        /// <summary>
        /// Construct MessageChangedEventArgs with the specified new Message string.
        /// </summary>
        /// <param name="message">The new Message string.</param>
        internal MessageChangedEventArgs(string message)
        {
            m_Message = message;
        }

        /// <summary>
        /// Construct MessageChangedEventArgs based on an internal MessageChangedEventArgs.
        /// </summary>
        /// <param name="internalArgs">The internal MessageChangedEventArgs</param>
        internal MessageChangedEventArgs(Monitor.Windows.MessageChangedEventArgs internalArgs)
        {
            m_Message = internalArgs.Message;
        }

        /// <summary>
        /// Get the new Message string property from this event.
        /// </summary>
        public string Message { get { return m_Message; } }
    }
}
