﻿#region File Header
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

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Thrown when an operation times out
    /// </summary>
    [Serializable]
    public class GibraltarTimeoutException : GibraltarException
    {
        /// <summary>
        /// Initializes a new instance of the GibraltarTimeoutException class.
        /// </summary>
        /// <remarks>This constructor initializes the Message property of the new instance to a system-supplied
        /// message that describes the error and takes into account the current system culture.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarTimeoutException()
        {
            // Just the base default constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarTimeoutException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <remarks>This constructor initializes the Message property of the new instance using the
        /// message parameter.  The InnerException property is left as a null reference.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarTimeoutException(string message)
            : base(message)
        {
            // Just the base constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarTimeoutException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference if no inner exception is specified.</param>
        /// <remarks>An exception that is thrown as a direct result of a previous exception should include
        /// a reference to the previous exception in the innerException parameter.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Just the base constructor
        }    
    }
}
