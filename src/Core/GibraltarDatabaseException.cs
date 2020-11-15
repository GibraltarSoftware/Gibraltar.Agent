
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
using System;

#endregion File Header

namespace Gibraltar
{
    /// <summary>
    /// This exception indicates that an error occurred in Gibraltar's Index database.
    /// </summary>
    /// <remarks>This exception type encapsulates a VistaDB error code encountered by Gibraltar's Index database.
    /// For more information, see the root class, Exception.</remarks>
    [Serializable]
    public class GibraltarDatabaseException : GibraltarException
    {
        private readonly int m_Code;

        /// <summary>
        /// Initializes a new instance of the GibraltarDatabaseException class.
        /// </summary>
        /// <remarks>This constructor initializes the Message property of the new instance to a system-supplied
        /// message that describes the error and takes into account the current system culture.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarDatabaseException()
        {
            // Just the base default constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarDatabaseException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <remarks>This constructor initializes the Message property of the new instance using the
        /// message parameter.  The InnerException property is left as a null reference.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarDatabaseException(string message)
            : base(message)
        {
            // Just the base constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarDatabaseException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference if no inner exception is specified.</param>
        /// <remarks>An exception that is thrown as a direct result of a previous exception should include
        /// a reference to the previous exception in the innerException parameter.
        /// For more information, see the base constructor in Exception.</remarks>
        public GibraltarDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Just the base constructor
        }

        /// <summary>
        /// Initializes a new instance of the GibraltarDatabaseException class with a specified
        /// VistaDB error code, error message, and a reference to the inner exception that is the cause
        /// of this exception.
        /// </summary>
        /// <param name="code">The VistaDB error code that is the cause of the current exception.</param>
        /// <param name="message">The error message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// null reference if no inner exception is specified.</param>
        /// <remarks>This is the preferred way to initialize this exception type, because
        /// it allows the Code property to be initialized to a supplied VistaDB error code.</remarks>
        public GibraltarDatabaseException(int code, string message, Exception innerException)
            :base(message, innerException)
        {
            m_Code = code;
        }

        /// <summary>
        /// The VistaDB error code which is encapsulated by this exception.
        /// </summary>
        public int Code { get { return m_Code; } }
    }
}