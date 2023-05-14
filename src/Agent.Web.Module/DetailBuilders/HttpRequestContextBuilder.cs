#region File Header

// <copyright file="HttpRequestContextBuilder.cs" company="Gibraltar Software Inc.">
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

using System.Web;

#endregion
namespace Gibraltar.Agent.Web.Module.DetailBuilders
{
    public class HttpContextRequestDetailBuilder : IRequestDetailBuilder
    {
        private readonly HttpContextBase _context;

        public HttpContextRequestDetailBuilder(HttpContextBase context)
        {
            _context = context;
        }

        public RequestBlockDetail GetDetails()
        {
            return new RequestBlockDetail(_context.Request.Browser.Browser,
                _context.Request.ContentType,
                _context.Request.ContentLength,
                _context.Request.IsLocal,
                _context.Request.IsSecureConnection,
                _context.Request.UserHostAddress,
                _context.Request.UserHostName);
        }
    }
}