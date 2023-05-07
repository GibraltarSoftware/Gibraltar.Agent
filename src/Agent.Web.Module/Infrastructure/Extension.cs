#region File Header

// <copyright file="Extension.cs" company="Gibraltar Software Inc.">
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
using System.Web;
using Gibraltar.Agent.Web.Module.DetailBuilders;

#endregion

namespace Gibraltar.Agent.Web.Module.Infrastructure
{
    public static class Extension
    {
        public static bool InterestedInRequest(this HttpRequestBase request)
        {
            List<string> extenstionWhiteList = new List<string> { ".html", ".htm", ".aspx", "" };

            return extenstionWhiteList.Contains(request.CurrentExecutionFilePathExtension) &&
                   !request.CurrentExecutionFilePath.Contains("__browserLink");
        }


        public static string StandardXmlRequestBlock(this HttpContextBase context, string requestBody = null)
        {
            var builder = new RequestBlockBuilder(new HttpContextRequestDetailBuilder(context));

            return builder.Build(requestBody);            
        }
        
    }
}