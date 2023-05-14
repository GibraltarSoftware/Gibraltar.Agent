#region File Header

// <copyright file="JavaScriptLogger.cs" company="Gibraltar Software Inc.">
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

using Gibraltar.Agent.Web.Module.DetailBuilders;
using Gibraltar.Agent.Web.Module.Models;

#endregion

namespace Gibraltar.Agent.Web.Module.Infrastructure
{
    public class JavaScriptLogger
    {
        /// <summary>
        /// Handles logging of the message to Loupe
        /// </summary>
        /// <param name="logRequest">A request received from the agent to log information</param>
        public virtual void Log(LogRequest logRequest)
        {
            var detailsBlockBuilder = new LogMessageBlockBuilder(logRequest);
            var sourceProvider = new JavaScriptSourceProvider();

            foreach (var logMessage in logRequest.LogMessages)
            {
                var jsException = CreateJavaScriptException(logMessage);

                var detailsBlock = detailsBlockBuilder.Build(logMessage);

                var messageSource = sourceProvider.ProcessMessage(logMessage);

                Gibraltar.Agent.Log.Write(logMessage.Severity,
                                          "Loupe",
                                           messageSource,
                                           "",
                                          jsException,
                                          LogWriteMode.Queued,
                                          detailsBlock,
                                          logMessage.Category,
                                          logMessage.Caption,
                                          logMessage.Description,
                                          logMessage.Parameters);
            }
        }

        private static JavaScriptException CreateJavaScriptException(LogMessage logMessage)
        {
            JavaScriptException jsException = null;
            if (logMessage.Exception != null && !logMessage.Exception.IsEmpty())
            {
                jsException = new JavaScriptException(logMessage.Exception.Message, logMessage.Exception.StackTrace);
            }
            return jsException;
        }
    }
}