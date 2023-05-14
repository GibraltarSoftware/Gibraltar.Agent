#region File Header

// <copyright file="MethodSourceInfo.cs" company="Gibraltar Software Inc.">
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
    public class MethodSourceInfo
    {
        /// <summary>
        /// File that the error occurred in
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Class that the error occured in
        /// </summary>
        /// <value>The class.</value>
        public string Class { get; set; }

        /// <summary>
        /// Function that was being executed when error occurred
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Optional. The line number upon which the error occurred
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// Optional. The column number upon which the error occurred
        /// </summary>
        public int? Column { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(File) 
                   && string.IsNullOrWhiteSpace(Class) 
                   && string.IsNullOrWhiteSpace(Method) 
                   && !Line.HasValue && !Column.HasValue;
        }
    }
}