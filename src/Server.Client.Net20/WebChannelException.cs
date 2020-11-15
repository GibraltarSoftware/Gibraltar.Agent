
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
    /// The base class for all exceptions thrown by the Web Channel
    /// </summary>
    [Serializable]
    public class WebChannelException : GibraltarException
    {
        /// <summary>
        /// Create a new web channel exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public WebChannelException(string message, Exception innerException, Uri requestUri)
            : base(message, innerException)
        {
            RequestUri = requestUri;
        }

        /// <summary>
        /// Create a new web channel exception
        /// </summary>
        public WebChannelException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// The inner exception as a web exception.  May be null.
        /// </summary>
        public WebException WebException
        {
            get { return InnerException as WebException; }
        }

        /// <summary>
        /// the url that was requested.
        /// </summary>
        public Uri RequestUri { get; private set; }
    }
}
