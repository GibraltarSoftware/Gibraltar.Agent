using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;

namespace Loupe.Agent.Web.Module.MVCTest.Controllers
{
    public class HomeController : Controller
    {
        private static int count = 0;

        public ActionResult Index()
        {
            ViewBag.SessionId = ControllerContext.HttpContext.Items["LoupeSessionId"].ToString();

            return View();
        }

        // Add Output cache to ensure the get from the js isn't
        // cached by IE
        [OutputCache(NoStore = true, Duration = 0)]
        public HttpResponseMessage data()
        {

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{\"theDate\":'" + DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "', \"theValue\": " + count + "}")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            count++;

            return resp;
        }
    }
}