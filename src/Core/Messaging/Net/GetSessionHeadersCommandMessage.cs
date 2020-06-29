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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gibraltar.Messaging.Net
{
    /// <summary>
    /// Command for retrieving the list of session headers
    /// </summary>
    public class GetSessionHeadersCommandMessage : NetworkMessage
    {
        /// <summary>
        /// create a new session headers command message
        /// </summary>
        public GetSessionHeadersCommandMessage()
        {
            TypeCode = NetworkMessageTypeCode.GetSessionHeaders;
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Write the packet to the stream
        /// </summary>
        protected override void OnWrite(Stream stream)
        {
        }

        /// <summary>
        /// Read packet data from the stream
        /// </summary>
        protected override void OnRead(Stream stream)
        {
        }    
    }
}
