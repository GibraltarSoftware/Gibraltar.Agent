#region File Header

// <copyright file="Exception.cs" company="Gibraltar Software Inc.">
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

using System.Collections.Generic;

#endregion

namespace Gibraltar.Agent.Web.Module.Models
{
    public class Exception
    {
        /// <summary>
        /// The message associated with the error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The URL upon which the error occurred
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The stack trace
        /// </summary>
        public List<string> StackTrace { get; set; }

        /// <summary>
        /// Optional. The cause of the error
        /// </summary>
        public string Cause { get; set; }

        /// <summary>
        /// Optional. The line number upon which the error occurred
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// Optional. The column number upon which the error occurred
        /// </summary>
        public int? Column { get; set; }


        /// <summary>
        /// Indicates if the class is in fact empty i.e created but with no values
        /// </summary>
        /// <remarks>This method is only used by the code creating a <see cref="JavaScriptException"/>
        /// as such it only needs to know if message and stack trace are not null.
        /// We need this method as if a request is received with an empty object rather
        /// than null for Error then JSON.net will create a new empty object with no data
        /// which we don't want to log.</remarks>
        /// <returns>true if necessary properties not null; otherwise false</returns>
        public bool IsEmpty()
        {
            return Message == null && StackTrace != null;
        }
    }
}