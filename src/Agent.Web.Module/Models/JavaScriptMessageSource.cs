#region File Header

// <copyright file="JavaScriptMessageSource.cs" company="Gibraltar Software Inc.">
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

#endregion

namespace Gibraltar.Agent.Web.Module.Models
{
    public class JavaScriptMessageSource: IMessageSourceProvider
    {

        /// <summary>
        /// The name of the class in which the message occurred
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The name of the file in which the message occurred
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The line number upon which the message occurred
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The name of the method in which the message occurred
        /// </summary>
        public string MethodName { get; set; }      
    }
}