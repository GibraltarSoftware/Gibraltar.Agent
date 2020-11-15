
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
    /// Thrown by the web channel when it is unable to authenticate to the remote server.
    /// </summary>
    [Serializable]
    public class WebChannelAuthorizationException : WebChannelException
    {
        /// <summary>
        /// Create a new authorization exception
        /// </summary>
        public WebChannelAuthorizationException(string message, WebException innerException, Uri requestUri)
            : base(message, innerException, requestUri)
        {
            
        }

        /// <summary>
        /// Create a new authorization exception
        /// </summary>
        public WebChannelAuthorizationException(string message)
            : base(message)
        {
            
        }
    }
}
