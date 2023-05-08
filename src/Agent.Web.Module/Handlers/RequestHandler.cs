#region File Header

// <copyright file="RequestHandler.cs" company="Gibraltar Software Inc.">
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
using System.Web;
using Gibraltar.Agent.Web.Module.Infrastructure;
using Exception = System.Exception;

#endregion
#pragma warning disable 1591

namespace Gibraltar.Agent.Web.Module.Handlers
{
    public class RequestHandler
    {
        private readonly UrlCheck _urlCheck;


        public RequestHandler()
        {
            _urlCheck = new UrlCheck();   
        }

        public bool HandleRequest(HttpContextBase context)
        {
            try
            {
                if (_urlCheck.IsLoupeUrl(context))
                {
                    return InputStreamInvalid(context);
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Critical, Constants.LogSystem, 0, ex, LogWriteMode.Queued,
                    context.StandardXmlRequestBlock(), Constants.Category, "Unable to process message due to " + ex.GetType(),
                    "Exception caught in top level catch block, this should have be caught by error handler specific to the part of the request processing that failed.");
#endif
            }

            return false;
        }

        private static void ResponseHandled(HttpContextBase context, HttpStatusCode statusCode, string userDescription = null)
        {
            if (!string.IsNullOrWhiteSpace(userDescription))
            {
                context.Response.StatusDescription = userDescription;
            }
            context.Response.StatusCode = (int)statusCode;
        }

        private bool InputStreamInvalid(HttpContextBase context)
        {
            if (context.Request.InputStream.Length == 0)
            {
                // No request body return bad request
#if DEBUG
                Log.Write(LogMessageSeverity.Information, Constants.LogSystem, 0, null, LogWriteMode.Queued,
                    context.StandardXmlRequestBlock(), Constants.Category, "Empty request body",
                    "Request was received for but no body was included in the POST");
#endif

                ResponseHandled(context, HttpStatusCode.BadRequest, "Empty request body");

                return true;
            }

            if (context.Request.InputStream.Length > 204800)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Information, Constants.LogSystem, 0, null, LogWriteMode.Queued,
                    context.StandardXmlRequestBlock(), Constants.Category, "Request body exceeds limit",
                    "Request was received but the body included exceeded the size limit of 2k and was {0}", SizeSuffix(context.Request.InputStream.Length));   
#endif       
                ResponseHandled(context, HttpStatusCode.RequestEntityTooLarge, "Request body exceeds size limit");

                return true;
            }

            return false;
        }

        private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB" };

        private string SizeSuffix(long value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
    }
}