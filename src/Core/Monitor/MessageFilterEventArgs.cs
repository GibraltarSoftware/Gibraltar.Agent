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
using Gibraltar.Data;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// EventArgs for Message Filter events.
    /// </summary>
    public class MessageFilterEventArgs : EventArgs
    {
        /// <summary>
        /// A new log message received for possible display by the (LiveLogViewer) sender of this event.
        /// </summary>
        public readonly ILogMessage Message;

        /// <summary>
        /// Cancel (block) this message from being displayed to users by the (LiveLogViewer) sender of this event.
        /// </summary>
        public bool Cancel;

        internal MessageFilterEventArgs(ILogMessage message)
        {
            Message = message;
            Cancel = false;
        }
    }
}
