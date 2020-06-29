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
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/



#endregion File Header

using System;
using System.Net;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// An exception thrown to indicate a connection failure on the web channel.
    /// </summary>
    [Serializable]
    public class WebChannelConnectFailureException : WebChannelException
    {
        /// <summary>
        /// Create a new connection failure exception
        /// </summary>
        public WebChannelConnectFailureException(string message, Exception innerException, Uri requestUri)
            : base(message, innerException, requestUri)
        {
            
        }

        /// <summary>
        /// Create a new connection failure exception
        /// </summary>
        public WebChannelConnectFailureException(string message)
            : base(message)
        {

        }
    }
}
