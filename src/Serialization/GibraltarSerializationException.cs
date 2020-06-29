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
using System.Text;

namespace Gibraltar.Serialization
{
    /// <summary>
    /// This is a base class for any new serialization Exception types we define and for generic exceptions
    /// generated in Serialization.
    /// </summary>
    /// <remarks>Any generation of an ApplicationException in Serialization should probably use this class instead.</remarks>
    [Serializable]
    public class GibraltarSerializationException : GibraltarException
    {
        // This is a dummy wrapper around Gibraltar exceptions (for now)

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class.
        /// </summary>
        /// <remarks>This constructor initializes the Message property of the new instance to a system-supplied
        /// message that describes the error and takes into account the current system culture.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarSerializationException()
        {
            // Just the base default constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <remarks>This constructor initializes the Message property of the new instance using the
        /// message parameter.  The InnerException property is left as a null reference.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarSerializationException(string message)
            : base(message)
        {
            // Just the base constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <param name="streamFailed">Indicates if the entire stream is now considered corrupt and no further packets can be retrieved.</param>
        /// <remarks>This constructor initializes the Message property of the new instance using the
        /// message parameter.  The InnerException property is left as a null reference.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarSerializationException(string message, bool streamFailed)
            : base(message)
        {
            StreamFailed = streamFailed;
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference if no inner exception is specified.</param>
        /// <remarks>An exception that is thrown as a direct result of a previous exception should include
        /// a reference to the previous exception in the innerException parameter.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Just the base constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference if no inner exception is specified.</param>
        /// <param name="streamFailed">Indicates if the entire stream is now considered corrupt and no further packets can be retrieved.</param>
        /// <remarks>An exception that is thrown as a direct result of a previous exception should include
        /// a reference to the previous exception in the innerException parameter.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarSerializationException(string message, Exception innerException, bool streamFailed)
            : base(message, innerException)
        {
            StreamFailed = streamFailed;
        }

        /// <summary>
        /// Indicates if the exception is a stream error, so no further packets can be serialized
        /// </summary>
        public bool StreamFailed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the GibraltarSerializationException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about
        /// the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about
        /// the source or destination.</param>
        /// <remarks>This constructor is called during deserialization to reconstitute the exception object
        /// transmitted over a stream.  For more information, see the base constructor in Exception.</remarks>
        protected GibraltarSerializationException(System.Runtime.Serialization.SerializationInfo info,
                                                  System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            // Just the base constructor           
        }
    }
}
