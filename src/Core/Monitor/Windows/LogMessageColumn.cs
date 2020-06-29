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
