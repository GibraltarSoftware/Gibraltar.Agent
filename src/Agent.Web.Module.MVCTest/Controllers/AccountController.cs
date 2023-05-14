using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Loupe.Agent.Web.Module.MVCTest.Models;

namespace Loupe.Agent.Web.Module.MVCTest.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
         public ActionResult Logon()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logon(LoginModel model)
        {
            FormsAuthentication.SetAuthCookie(model.UserName,false);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

    }
}
