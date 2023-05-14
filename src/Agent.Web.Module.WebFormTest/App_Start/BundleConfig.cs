using System.Web.Optimization;
using System.Web.UI;

namespace Loupe.Agent.Web.Module.WebFormTest
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
            //                "~/Scripts/WebForms/WebForms.js",
            //                "~/Scripts/WebForms/WebUIValidation.js",
            //                "~/Scripts/WebForms/MenuStandards.js",
            //                "~/Scripts/WebForms/Focus.js",
            //                "~/Scripts/WebForms/GridView.js",
            //                "~/Scripts/WebForms/DetailsView.js",
            //                "~/Scripts/WebForms/TreeView.js",
            //                "~/Scripts/WebForms/WebParts.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));


            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            // Use the Development version of Modernizr to develop with and learn from. Then, when you’re
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                            "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/logging").Include(
                "~/Scripts/Loupe.Agent.Native.min.js",
                "~/Scripts/logging.js",
                "~/Scripts/handlers.js"));

            ScriptManager.ScriptResourceMapping.AddDefinition(
                "respond",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/respond.min.js",
                    DebugPath = "~/Scripts/respond.js",
                });
        }
    }
}