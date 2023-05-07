#region File Header

// <copyright file="LoupeController.cs" company="Gibraltar Software Inc.">
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
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Gibraltar.Agent.Web.Module.Infrastructure;
using Gibraltar.Agent.Web.Module.Models;

#endregion

namespace Gibraltar.Agent.Web.Module
{
    public class LoupeController:ApiController
    {
        private readonly RequestProcessor _requestProcessor;

        public LoupeController()
        {
            _requestProcessor = new RequestProcessor();
        }

        [HttpPost]
        public HttpResponseMessage Log(LogRequest logRequest)
        {
            var context = new HttpContextWrapper(HttpContext.Current);

            try
            {
                _requestProcessor.Process(new HttpContextWrapper(HttpContext.Current), logRequest);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (System.Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Gibraltar.Agent.Log.Write(LogMessageSeverity.Critical, Constants.LogSystem, 0, ex, LogWriteMode.Queued,
                    context.StandardXmlRequestBlock(Request.Content.ReadAsStringAsync().Result), Constants.Category, "Unable to process message due to " + ex.GetType(),
                    "Exception caught in top level catch block, this should have be caught by error handler specific to the part of the request processing that failed.");
#endif
            }

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

    }
}