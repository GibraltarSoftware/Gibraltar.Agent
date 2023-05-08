#region File Header

// <copyright file="HeaderHandler.cs" company="Gibraltar Software Inc.">
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
using Gibraltar.Agent.Web.Module.Infrastructure;

#endregion

namespace Gibraltar.Agent.Web.Module.Handlers
{
    public class HeaderHandler
    {
        private const string clientHeaderName = "loupe-agent-sessionId";

        public void HandleRequest(HttpContextBase context)
        {
            if (context.Request.InterestedInRequest())
            {
                CreateContextItem(context);

                if (HasHeader(context))
                {
                    AddValueToContext(context);
                }
            }
        }

        private bool HasHeader(HttpContextBase context)
        {
            return context.Request.Headers[clientHeaderName] != null;
        }

        private void CreateContextItem(HttpContextBase context)
        {
            context.Items.Add(Constants.AgentSessionId, "");
        }

        private void AddValueToContext(HttpContextBase context)
        {
            context.Items[Constants.AgentSessionId] = context.Request.Headers[clientHeaderName];
        }
    }
}