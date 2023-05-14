#region File Header

// <copyright file="JavaScriptException.cs" company="Gibraltar Software Inc.">
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
using System.Linq;

#endregion

namespace Gibraltar.Agent.Web.Module.Models
{
    /// <summary>
    /// Defines a JavaScript exception
    /// </summary>
    public class JavaScriptException : System.Exception
    {
        private readonly string _stackTrace;

        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="message">The exception message</param>
        public JavaScriptException(string message) : base(message) { }

        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="stackTrace">The stack trace, as a list of strings</param>
        public JavaScriptException(string message, IEnumerable<string> stackTrace) : base(message)
        {
            if (stackTrace != null)
            {
                _stackTrace = string.Join("\r  ", stackTrace.Select(s => s).ToArray());
            }
        }

        /// <summary>
        /// Show the stack trace for the exception
        /// </summary>
        public override string StackTrace
        {
            get { return _stackTrace; }
        }
    }
}