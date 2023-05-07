#region File Header

// <copyright file="CORSHandler.cs" company="Gibraltar Software Inc.">
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

#endregion

namespace Gibraltar.Agent.Web.Module.Handlers
{
    public class CORSHandler
    {
        private readonly UrlCheck _urlCheck;
        private HostCORSConfiguration _configruation;


        /// <summary>
        /// Allows tests to inject mock class for testing purposes
        /// </summary>
        internal HostCORSConfiguration Configuration
        {
            get { return _configruation ?? (_configruation = new HostCORSConfiguration()); }
            set { _configruation = value; }
        }

        public CORSHandler()
        {
            _urlCheck = new UrlCheck();
        }

        public bool HandleRequest(HttpContextBase context)
        {
            bool handled = false;

            if (IsCrossOriginRequest(context) && _urlCheck.IsLoupeUrl(context))
            {
                try
                {
                    switch (context.Request.HttpMethod)
                    {
                        case "OPTIONS":
                            CreateOptionsResponse(context);
                            handled = true;
                            break;

                        case "POST":
                            AddHeadersToPost(context);
                            // don't flag as handled as we need the request
                            // to continue through the pipeline
                            break;

                        default:
                            MethodNotSupportedResponse(context);
                            handled = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    GC.KeepAlive(ex);
#if DEBUG
                    Log.Error(ex, "Loupe.Internal", "Unable to handle CORS request due to " + ex.GetType(),
                        "Exception occurred trying to handle the {0} request of a CORS request\r\n{1}", context.Request.HttpMethod, ex.Message);
#endif
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    handled = true;
                }
            }

            return handled;
        }

        private void CreateOptionsResponse(HttpContextBase context)
        {
            AddAllowHeaders(context);

            AddAllowOrigin(context);

            AddAllowMethods(context);

            AddMaxAge(context);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private void AddHeadersToPost(HttpContextBase context)
        {
            AddAllowOrigin(context);
        }

        private void MethodNotSupportedResponse(HttpContextBase context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
        }

        private void AddAllowMethods(HttpContextBase context)
        {
            if (!Configuration.GlobalAllowMethods)
            {
                context.Response.AddHeader("Access-Control-Allow-Methods", "POST");
            }
        }

        private void AddAllowOrigin(HttpContextBase context)
        {
            if (!Configuration.GlobalAllowOrigin)
            {
                var origin = "*";

                //make sure we don't add a duplicate CORS header.
                var requestedOrigin = context.Request.Headers.Get("origin");
                if (string.IsNullOrWhiteSpace(requestedOrigin) == false)
                {
                    origin = requestedOrigin;

                    //and since we're being specific we can allow credentials.
                    context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                }

                context.Response.AddHeader("Access-Control-Allow-Origin", origin);
            }
        }

        private void AddAllowHeaders(HttpContextBase context)
        {
            var requestHeader = context.Request.Headers.Get("Access-Control-Request-Headers");

            if (requestHeader != null & !Configuration.GlobalAllowHeaders)
            {
                context.Response.AddHeader("Access-Control-Allow-Headers", requestHeader);
            }
        }

        private void AddMaxAge(HttpContextBase context)
        {
            if (!Configuration.GlobalMaxAge)
            {
                // to reduce number of pre-flight requests set the max
                // age to 20 mins (1200 seconds)
                context.Response.AddHeader("Access-Control-Max-Age","1200");
            }
        }

        private bool IsCrossOriginRequest(HttpContextBase context)
        {
            return context.Request.Headers.Get("Origin") != null;
        }
    }
}