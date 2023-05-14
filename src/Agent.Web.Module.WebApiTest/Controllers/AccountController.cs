using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Loupe.Agent.Web.Module.WebApiTest.Models;

namespace Loupe.Agent.Web.Module.WebApiTest.Controllers
{
    public class AccountController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Logon(LoginModel model)
        {
            FormsAuthentication.SetAuthCookie(model.UserName,false);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}