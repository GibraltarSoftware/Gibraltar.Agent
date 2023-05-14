#region File Header

// <copyright file="Logging.cs" company="Gibraltar Software Inc.">
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

#endregion

namespace Gibraltar.Agent.Web.Module.DetailBuilders
{
    public class RequestBlockBuilder:DetailsBuilderBase
    {
        private readonly IRequestDetailBuilder _requestDetailBuilder;

        const string DetailsFormat = "<UserAgent>{0}</UserAgent><ContentType>{1}</ContentType><ContentLength>{2}</ContentLength><IsLocal>{3}</IsLocal><IsSecureConnection>{4}</IsSecureConnection><UserHostAddress>{5}</UserHostAddress><UserHostName>{6}</UserHostName>";

        public RequestBlockBuilder(IRequestDetailBuilder requestDetailBuilder)
        {
            _requestDetailBuilder = requestDetailBuilder;
        }

        public string Build(string requestBody = null)
        {
            DetailBuilder.Clear();

            DetailBuilder.Append("<Request>");

            try
            {
                var details = _requestDetailBuilder.GetDetails();

                DetailBuilder.AppendFormat(DetailsFormat, 
                    details.Browser,
                    details.ContentType,
                    details.ContentLength,
                    details.IsLocal,
                    details.IsSecureConnection,
                    details.UserHostAddress,
                    details.UserHostName);

            }
            catch (System.Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Error(ex, "Loupe.Internal", "Unable to build standard Request details block due to " + ex.GetType(),
                    "Exception occurred whilst trying to build the standard Request details block, no request will be added to detail\r\n{0}",
                    ex.Message);
#endif
                DetailBuilder.Append(
                    "We were unable to record details from the Request itself due to an exception occurring whilst extracting information from the Request.");
            }

            if (requestBody != null)
            {
                DetailBuilder.Append("<RequestBody>" + requestBody + "</RequestBody>");
            }

            DetailBuilder.Append("</Request>");

            return DetailBuilder.ToString();
        }         
    }
}