using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Loupe.Agent.Web.Module.WebApiTest.Controllers
{
    public class MyController : ApiController
    {
        private static int count = 0;

        [HttpGet]
        public HttpResponseMessage data()
        {
            var stuff = new
            {
                theDate = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture),
                theValue = count
            };

            count++;

            return Request.CreateResponse(HttpStatusCode.OK,stuff);
        }
    }
}
