using System;
using System.Collections.Specialized;
using System.Web;
using Gibraltar.Agent.Web.Module.Handlers;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Cookie_Handler
{
    [TestFixture]
    public class When_recieves_request_without_cookie
    {
        private CookieHandler target;

        protected HttpContextBase HttpContext;
        protected HttpRequestBase HttpRequest;
        protected HttpResponseBase HttpResponse;
        private const string LoupeCookieName = "LoupeSessionId";

        [SetUp]
        public void SetUp()
        {
            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();

            HttpRequest.Cookies.Returns(new HttpCookieCollection());
            HttpRequest.Headers.Returns(new NameValueCollection());
            HttpResponse.Cookies.Returns(new HttpCookieCollection());


            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.Returns(HttpResponse);

            target = new CookieHandler();
        }

        [Test]
        public void Should_add_cookie_to_response_if_it_does_not_have_one()
        {
            target.HandleRequest(HttpContext);

            Assert.That(HttpResponse.Cookies[LoupeCookieName], Is.Not.Null);
        }

        [Test]
        public void Should_add_cookie_to_request_if_it_does_not_have_one()
        {
            target.HandleRequest(HttpContext);

            Assert.That(HttpRequest.Cookies[LoupeCookieName], Is.Not.Null);            
        }


        [Test]
        public void Should_not_add_cookie_to_call_for_non_html_resource([Values("imagee/test.jpg","Content/site.css","Scripts/app.js", "elmah.axd")] string file)
        {
            HttpRequest.Url.Returns(new Uri("http://www.test.com/" + file));

            HttpRequest.CurrentExecutionFilePathExtension.Returns(file.Substring(file.LastIndexOf(".")));
            HttpRequest.CurrentExecutionFilePath.Returns(file);

            target.HandleRequest(HttpContext);

            Assert.That(HttpResponse.Cookies[LoupeCookieName], Is.Null);
        }

        [Test]
        public void Should_add_cookie([Values("Views/index.html", "Content/about.htm", "Default.aspx", "")] string file)
        {
            HttpRequest.Url.Returns(new Uri("http://www.test.com/" + file));

            var extension = file.Contains(".") ? file.Substring(file.LastIndexOf(".")) : "";

            HttpRequest.CurrentExecutionFilePathExtension.Returns(extension);
            HttpRequest.CurrentExecutionFilePath.Returns(file);

            target.HandleRequest(HttpContext);

            Assert.That(HttpResponse.Cookies[LoupeCookieName], Is.Not.Null);
        }

        [Test]
        public void Should_not_add_cookie_for_browserLink()
        {
            HttpRequest.Url.Returns(new Uri("http://www.test.com/__browserLink/requestData/fa2b198ba711488f9586768be6fe5d25?version=2"));

            HttpRequest.CurrentExecutionFilePathExtension.Returns("");
            HttpRequest.CurrentExecutionFilePath.Returns("/__browserLink/requestData/fa2b198ba711488f9586768be6fe5d25");

            target.HandleRequest(HttpContext);


            Assert.That(HttpRequest.Cookies.Get(LoupeCookieName), Is.Null);
        }

        [Test]
        public void Should_not_add_cookie_for_CORS_request()
        {
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            target.HandleRequest(HttpContext);


            Assert.That(HttpRequest.Cookies.Get(LoupeCookieName), Is.Null);
        }
    }
}