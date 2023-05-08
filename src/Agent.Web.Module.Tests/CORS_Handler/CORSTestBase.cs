using System;
using System.Collections.Specialized;
using System.Web;
using Gibraltar.Agent.Web.Module.Handlers;
using Gibraltar.Agent.Web.Module.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.CORS_Handler
{
    public class CORSTestBase
    {
        protected CORSHandler Target;

        protected HttpContextBase HttpContext;
        protected HttpRequestBase HttpRequest;
        protected HttpResponseBase HttpResponse;
        protected HostCORSConfiguration FakeConfigProvider;

        protected NameValueCollection httpResponseHeaders;

        [SetUp]
        public void SetUp()
        {
            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();
            FakeConfigProvider = Substitute.For<HostCORSConfiguration>();
            FakeConfigProvider.GlobalAllowOrigin.Returns(false);
            FakeConfigProvider.GlobalAllowHeaders.Returns(false);
            FakeConfigProvider.GlobalAllowMethods.Returns(false);

            httpResponseHeaders = new NameValueCollection();

            HttpResponse.Headers.Returns(httpResponseHeaders);
            HttpResponse.When(x => x.AddHeader(Arg.Any<string>(), Arg.Any<string>())).Do(x =>
            {
                AddToResponseHeaders(x.Args()[0].ToString(), x.Args()[1].ToString());
            });

            HttpRequest.Headers.Returns(new NameValueCollection());
            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.AddHeader(Arg.Any<string>(), Arg.Any<String>());
            HttpContext.Response.Returns(HttpResponse);

            Target = new CORSHandler();
            Target.Configuration = FakeConfigProvider;
        }


        private void AddToResponseHeaders(string name, string value)
        {
            httpResponseHeaders.Add(name, value);
        }
    }
}