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
namespace Gibraltar.Agent.Internal
{
    /// <summary>
    /// Exchanges information between the Agent's IMessageSourceProvider implementation and the internal Monitor implementation.
    /// </summary>
    internal class MessageSourceProvider : Monitor.IMessageSourceProvider, IMessageSourceProvider
    {
        private readonly IMessageSourceProvider m_CallingProvider;

        public MessageSourceProvider(IMessageSourceProvider callingProvider)
        {
            m_CallingProvider = callingProvider;
        }

        /// <summary>
        /// Should return the simple name of the method which issued the log message.
        /// </summary>
        public string MethodName
        {
            get { return m_CallingProvider.MethodName; }
        }

        /// <summary>
        /// Should return the full name of the class (with namespace) whose method issued the log message.
        /// </summary>
        public string ClassName
        {
            get { return m_CallingProvider.ClassName; }
        }

        /// <summary>
        /// Should return the name of the file containing the method which issued the log message.
        /// </summary>
        public string FileName
        {
            get { return m_CallingProvider.FileName; }
        }

        /// <summary>
        /// Should return the line within the file at which the log message was issued.
        /// </summary>
        public int LineNumber
        {
            get { return m_CallingProvider.LineNumber; }
        }
    }
}
