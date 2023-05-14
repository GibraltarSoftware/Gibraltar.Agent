#region File Header

// <copyright file="LogMessage.cs" company="Gibraltar Software Inc.">
// Gibraltar Software Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;

#endregion

namespace Gibraltar.Agent.Web.Module.Models
{
    public class LogMessage
    {
        /// <summary>
        /// Severity of the message
        /// </summary>
        public LogMessageSeverity Severity { get; set; }

        /// <summary>
        /// The category to log against
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The log message caption
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// The log message description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional. Parameters to be added to the message
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Optional. Additional details, such as client browser settings
        /// </summary>
        /// <remarks>
        /// This is converted into the XML details
        /// </remarks>
        public string Details { get; set; }

        /// <summary>
        /// Optional. Details of a client side exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Information about the method that generated the message
        /// </summary>
        public MethodSourceInfo MethodSourceInfo { get; set; }

        /// <summary>
        /// Specifies when the log message was created on the client
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Specifics the sequence number of the message
        /// </summary>
        public long? Sequence { get; set; }

        /// <summary>
        /// SessionId from either cookie or set by client
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Session Id as set by JS Agent when it started
        /// </summary>
        public string AgentSessionId { get; set; }
    }
}