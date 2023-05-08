#region File Header

// <copyright file="UrlCheck.cs" company="Gibraltar Software Inc.">
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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

#endregion

namespace Gibraltar.Agent.Web.Module.Infrastructure
{
    public class UrlCheck
    {
        private readonly Regex _urlRegex = new Regex("/[Ll]oupe/[Ll]og(?!/.)", RegexOptions.Compiled);

        public bool IsLoupeUrl(HttpRequestMessage request)
        {
            return CheckUrl(request.RequestUri.LocalPath);
        }

        public bool IsLoupeUrl(HttpContextBase context)
        {
            return CheckUrl(context.Request.Url.LocalPath);
        }

        private bool CheckUrl(string path)
        {
            try
            {
                var urlMatch = _urlRegex.Match(path);

                return urlMatch.Success;
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);

                // for whatever reason the match has thrown an error, in 
                // normal circumstances this should not happen but to ensure
                // we do not cause problems elsewhere we will swallow this 
                // exception and let the request continue through the pipeline
#if DEBUG
                Log.Write(LogMessageSeverity.Error, "Loupe", 0, ex, LogWriteMode.Queued, null, "Loupe.Internal", "Unable to match regex due to " + ex.GetType(),
                    "Exception thrown when attempting to match the regex against the local path");
#endif
            }

            return false;
        }
    }
}