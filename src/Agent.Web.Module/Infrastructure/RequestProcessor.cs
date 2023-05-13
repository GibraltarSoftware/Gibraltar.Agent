#region File Header

// <copyright file="RequestProcessor.cs" company="Gibraltar Software Inc.">
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
using System.Web;
using System.Web.Caching;
using Gibraltar.Agent.Web.Module.DetailBuilders;
using Gibraltar.Agent.Web.Module.Models;

#endregion

namespace Gibraltar.Agent.Web.Module.Infrastructure
{
    public class RequestProcessor
    {
        readonly JavaScriptLogger _javaScriptLogger;


        public RequestProcessor()
        {
            _javaScriptLogger = new JavaScriptLogger();
        }

        public void Process(HttpContextBase context, LogRequest request)
        {

            if (request == null)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Error, Constants.LogSystem, 0, null, LogWriteMode.Queued,
                    context.StandardXmlRequestBlock(), Constants.Category, "Null request",
                    "The request received to process was null");
#endif                
                return;
            }

            EnhanceMessage(context, request);

            LogMessage(request);            
        }



        private void LogMessage(LogRequest logRequest)
        {
            _javaScriptLogger.Log(logRequest);
        }


        private void EnhanceMessage(HttpContextBase context, LogRequest logRequest)
        {
            CacheClientDetails(context, logRequest);
            AddSessionId(context, logRequest);
            logRequest.User = context.User;
        }

        private void CacheClientDetails(HttpContextBase context, LogRequest logRequest)
        {
            var sessionId = context.Items[Constants.SessionId] as string;
            if (sessionId != null && logRequest.Session != null && logRequest.Session.Client != null)
            {
                var clientDetailsBuilder = new ClientDetailsBuilder();

                context.Cache.Insert(sessionId, clientDetailsBuilder.Build(logRequest), null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
            }
        }

        private void AddSessionId(HttpContextBase context, LogRequest logRequest)
        {
            var sessionId = context.Items[Constants.SessionId] as string;
            var agentSessionId = context.Items[Constants.AgentSessionId] as string;

            if (string.IsNullOrWhiteSpace(agentSessionId) && logRequest.Session != null &&
                !string.IsNullOrWhiteSpace(logRequest.Session.CurrentAgentSessionId))
            {
                context.Items[Constants.AgentSessionId] = logRequest.Session.CurrentAgentSessionId;
            }

            for (int i = 0; i < logRequest.LogMessages.Count; i++)
            {
                var message = logRequest.LogMessages[i];

                if (sessionId == null && string.IsNullOrWhiteSpace(message.SessionId))
                {
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, Constants.LogSystem, 0, null, LogWriteMode.Queued,
                        context.StandardXmlRequestBlock(), Constants.Category, "No session Id",
                        "No session in context for Loupe or explicit session Id found on the request, not able to set session Id");
#endif
                    continue;
                }

                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    message.SessionId = sessionId;
                }

                if (string.IsNullOrWhiteSpace(message.AgentSessionId))
                {
                    message.AgentSessionId = agentSessionId;
                }

            }
        }

    }
}