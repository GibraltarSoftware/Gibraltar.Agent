using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Loupe.Agent.Web.Module.WebFormTest
{
    public partial class Account : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public static void Login(LoginModel model)
        {
            FormsAuthentication.SetAuthCookie(model.UserName,false);
        }
    }
}