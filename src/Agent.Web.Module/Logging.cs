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
using System.Web;
using Gibraltar.Agent.Web.Module.Handlers;

#endregion

namespace Gibraltar.Agent.Web.Module
{
    public class Logging:IHttpModule
    {
        private CORSHandler _corsHandler;
        private RequestHandler _requestHandler;
        private CookieHandler _cookieHandler;
        private HeaderHandler _headerHandler;

        public void Init(HttpApplication application)
        {
            _corsHandler = new CORSHandler();
            _requestHandler = new RequestHandler();
            _cookieHandler = new CookieHandler();
            _headerHandler = new HeaderHandler();
            application.PostAuthenticateRequest += OnPostAuthenticateRequest;
            application.BeginRequest += application_BeginRequest;
        }

        void application_BeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);

            if (_corsHandler.HandleRequest(context))
            {
                ((HttpApplication)sender).CompleteRequest();
            }
            else
            {
                _cookieHandler.HandleRequest(context);
                _headerHandler.HandleRequest(context);
            }
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;

            var handled = _requestHandler.HandleRequest(new HttpContextWrapper(application.Context));

            if (handled)
            {
               application.CompleteRequest();
            }
        }

        // Implemented as part of IHttpModule
        // but we don't use it
        public void Dispose() { }
    }
}
