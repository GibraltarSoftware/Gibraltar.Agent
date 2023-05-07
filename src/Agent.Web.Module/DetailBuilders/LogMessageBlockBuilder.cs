#region File Header

// <copyright file="LogMessageBlockBuilder.cs" company="Gibraltar Software Inc.">
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
using Gibraltar.Agent.Web.Module.Models;

#endregion

namespace Gibraltar.Agent.Web.Module.DetailBuilders
{
    public class LogMessageBlockBuilder:DetailsBuilderBase
    {
        private readonly LogRequest _request;
        private string _clientDetails;

        public LogMessageBlockBuilder(LogRequest request)
        {
            _request = request;
            _clientDetails = null;
        }

        public string Build(LogMessage logMessage)
        {
            if (_clientDetails == null)
            {
                var clientDetailsBuilder = new ClientDetailsBuilder();
                _clientDetails = clientDetailsBuilder.Build(_request);
            }

            DetailBuilder.Clear();

            DetailBuilder.Append("<Details>");
            DetailBuilder.Append(SessionDetailString(logMessage));
            DetailBuilder.Append(TimeStampDetailString(logMessage));
            DetailBuilder.Append(SequenceDetailString(logMessage));
            DetailBuilder.Append(_clientDetails);
            DetailBuilder.Append(CreateMethodSourceInfoString(logMessage));
            DetailBuilder.Append(UserSuppliedDetails(logMessage));
            DetailBuilder.Append("</Details>");

            return DetailBuilder.ToString();
        }

        private static string SessionDetailString(LogMessage message)
        {
            string sessionDetails = null;
                
            if (!string.IsNullOrWhiteSpace(message .SessionId))
            {
                sessionDetails = "<SessionId>" + message.SessionId + "</SessionId>";
            }

            if (!string.IsNullOrWhiteSpace(message.AgentSessionId))
            {
                sessionDetails = sessionDetails + "<AgentSessionId>" + message.AgentSessionId + "</AgentSessionId>";
            }

            return sessionDetails;
        }

        private static string TimeStampDetailString(LogMessage logMessage)
        {
            string timeStamp = null;

            if (!logMessage.TimeStamp.Equals(new DateTimeOffset()))
            {
                timeStamp = string.Format("<TimeStamp>{0}</TimeStamp>", logMessage.TimeStamp);
            }

            return timeStamp;
        }

        private static string SequenceDetailString(LogMessage logMessage)
        {
            return string.Format("<Sequence>{0}</Sequence>", logMessage.Sequence);
        }

        private string CreateMethodSourceInfoString(LogMessage logMessage)
        {
            if (logMessage.MethodSourceInfo != null && !logMessage.MethodSourceInfo.IsEmpty())
            {
                return ObjectToXmlString(logMessage.MethodSourceInfo);
            }

            return null;
        }

        private string UserSuppliedDetails(LogMessage logMessage)
        {
            if (string.IsNullOrWhiteSpace(logMessage.Details))
            {
                return null;
            }

            if (logMessage.Details.Equals("null", StringComparison.OrdinalIgnoreCase))
                return null; //we've seen this in the JSON...

            try
            {
                return JObjectToXmlString(logMessage.Details);
            }
            catch
            {
                // details are not JSON or we cannot convert to
                // XML so we will simply add the details to the block
                // as a string
            }

            return "<UserSupplied>" + logMessage.Details + "</UserSupplied>";
        }
         
    }
}