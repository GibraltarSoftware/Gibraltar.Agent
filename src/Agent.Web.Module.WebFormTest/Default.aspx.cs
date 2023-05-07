using System;
using System.Globalization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;

namespace Loupe.Agent.Web.Module.WebFormTest
{
    public partial class _Default : Page
    {
        private static int count = 0;
        public string SessionId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionId = Context.Items["LoupeSessionId"].ToString();
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public static string Data()
        {
            var output = "{\"theDate\":'" + DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) +
                   "', \"theValue\": " + count + "}";

            count++;

            return output;
        }
    }
}