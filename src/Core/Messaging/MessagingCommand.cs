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
namespace Gibraltar.Messaging
{
    /// <summary>
    /// Different types of commands.
    /// </summary>
    internal enum MessagingCommand
    {
        /// <summary>
        /// Not a command.
        /// </summary>
        None = 0,

        /// <summary>
        /// Flush the queue
        /// </summary>
        Flush = 1,

        /// <summary>
        /// Close the current file (and open a new one because the session isn't ending)
        /// </summary>
        CloseFile = 2,

        /// <summary>
        /// Alert the messaging system to make preparations for the application exiting.
        /// </summary>
        ExitMode = 3,

        /// <summary>
        /// Close the messenger (and don't restart it)
        /// </summary>
        CloseMessenger = 4,

        /// <summary>
        /// Cause the Gibraltar Live View form to be (generated if necessary and) shown.
        /// </summary>
        ShowLiveView = 5,

        /// <summary>
        /// Causes the network messenger to connect out to a remote viewer
        /// </summary>
        OpenRemoteViewer = 6
    }
}
