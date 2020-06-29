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

using Gibraltar.Agent.Data;


namespace Gibraltar.Agent.Internal
{
    /// <summary>
    /// A wrapper class which provides read-only access to our internal ExceptionInfo properties.
    /// </summary>
    internal class ExceptionInfo : IExceptionInfo
    {
        private readonly Loupe.Extensibility.Data.IExceptionInfo m_WrappedExceptionInfo;
        private ExceptionInfo m_InnerException;

        internal ExceptionInfo(Loupe.Extensibility.Data.IExceptionInfo wrappedExceptionInfo)
        {
            m_WrappedExceptionInfo = wrappedExceptionInfo;
        }

        /// <summary>
        /// The full name of the type of the Exception.
        /// </summary>
        public string TypeName { get { return m_WrappedExceptionInfo.TypeName; } }

        /// <summary>
        /// The Message string of the Exception.
        /// </summary>
        public string Message { get { return m_WrappedExceptionInfo.Message; } }

        /// <summary>
        /// A formatted string describing the source of an Exception.
        /// </summary>
        public string Source { get { return m_WrappedExceptionInfo.Source; } }

        /// <summary>
        /// A string dump of the Exception stack trace information.
        /// </summary>
        public string StackTrace { get { return m_WrappedExceptionInfo.StackTrace; } }

        /// <summary>
        /// The information about this exception's inner exception (or null if none).
        /// </summary>
        public IExceptionInfo InnerException
        {
            get
            {
                if (m_WrappedExceptionInfo.InnerException == null)
                    return null;

                if (m_InnerException == null)
                    m_InnerException = new ExceptionInfo(m_WrappedExceptionInfo.InnerException);

                return m_InnerException;
            }
        }
    }
}