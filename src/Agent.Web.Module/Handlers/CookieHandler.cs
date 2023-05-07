#region File Header

// <copyright file="CookieHandler.cs" company="Gibraltar Software Inc.">
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
using Gibraltar.Agent.Web.Module.Infrastructure;

#endregion

namespace Gibraltar.Agent.Web.Module.Handlers
{
    public class CookieHandler
    {
        public void HandleRequest(HttpContextBase context)
        {
            if (InterestedInRequest(context.Request))
            {
                if (CookieDoesNotExist(context.Request))
                {
                    AddSessionCookie(context);
                }

                AddContextItem(context);
            }
        }

        private void AddContextItem(HttpContextBase context)
        {
            var sessionId = context.Request.Cookies[Constants.SessionId].Value;
            context.Items.Add(Constants.SessionId, sessionId);
        }

        private void AddSessionCookie(HttpContextBase context)
        {
            var loupeCookie = new HttpCookie(Constants.SessionId);
            loupeCookie.HttpOnly = true;
            loupeCookie.Value = Guid.NewGuid().ToString();
            
            context.Request.Cookies.Add(loupeCookie);
            context.Response.Cookies.Add(loupeCookie);            
        }

        private bool CookieDoesNotExist(HttpRequestBase request)
        {
            return request.Cookies[Constants.SessionId] == null;
        }


        private bool InterestedInRequest(HttpRequestBase request)
        {
            // cookies not supported for CORS
            if (request.Headers.Get("Origin") != null)
            {
                return false;
            }

            return request.InterestedInRequest();
        }

    }
}