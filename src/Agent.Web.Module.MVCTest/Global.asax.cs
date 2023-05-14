using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Gibraltar.Agent;
using Gibraltar.Agent.Web.Mvc.Filters;

namespace Loupe.Agent.Web.Module.MVCTest
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {

            Log.StartSession(); //Prompt the Loupe Agent to start immediately
            GlobalFilters.Filters.Add(new MvcRequestMonitorAttribute());
            GlobalFilters.Filters.Add(new UnhandledExceptionAttribute());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
