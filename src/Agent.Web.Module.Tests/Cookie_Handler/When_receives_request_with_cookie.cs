using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using Gibraltar.Agent.Web.Module.Handlers;
using Gibraltar.Agent.Web.Module.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Cookie_Handler
{
    [TestFixture]
    public class When_receives_request_with_cookie
    {
        private CookieHandler target;

        protected HttpContextBase HttpContext;
        protected HttpRequestBase HttpRequest;
        protected HttpResponseBase HttpResponse;
        private Hashtable items;

        [SetUp]
        public void SetUp()
        {
            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();

            HttpRequest.Cookies.Returns(new HttpCookieCollection());
            HttpRequest.Headers.Returns(new NameValueCollection());
            HttpResponse.Cookies.Returns(new HttpCookieCollection());

            items = new Hashtable();
            HttpContext.Items.Returns(items);
            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.Returns(HttpResponse);

            target = new CookieHandler();
        }

        [Test]
        public void Should_not_alter_existing_cookie()
        {
            var loupeCookie = new HttpCookie(Constants.SessionId);
            loupeCookie.HttpOnly = true;
            loupeCookie.Value = Guid.Empty.ToString();

            HttpRequest.Cookies.Add(loupeCookie);
            HttpResponse.Cookies.Add(loupeCookie);

            target.HandleRequest(HttpContext);

            Assert.That(HttpResponse.Cookies, Contains.Item(Constants.SessionId));
            Assert.That(HttpResponse.Cookies[Constants.SessionId], Is.EqualTo(loupeCookie));
        }

        [Test]
        public void Should_not_value_to_context_items_for_non_html_resource([Values("imagee/test.jpg", "Content/site.css", "Scripts/app.js", "elmah.axd")] string file)
        {
            HttpRequest.Url.Returns(new Uri("http://www.test.com/" + file));

            HttpRequest.CurrentExecutionFilePathExtension.Returns(file.Substring(file.LastIndexOf(".")));
            HttpRequest.CurrentExecutionFilePath.Returns(file);

            target.HandleRequest(HttpContext);

            Assert.That(items.Keys, Has.No.Member(Constants.SessionId));
        }

        [Test]
        public void Should_add_value_to_context([Values("Views/index.html", "Content/about.htm", "Default.aspx", "")] string file)
        {
            var clientSessionId = Guid.NewGuid().ToString();

            var loupeCookie = new HttpCookie(Constants.SessionId);
            loupeCookie.HttpOnly = true;
            loupeCookie.Value = clientSessionId;

            HttpRequest.Cookies.Add(loupeCookie);
            HttpRequest.Url.Returns(new Uri("http://www.test.com/" + file));

            var extension = file.Contains(".") ? file.Substring(file.LastIndexOf(".")) : "";

            HttpRequest.CurrentExecutionFilePathExtension.Returns(extension);
            HttpRequest.CurrentExecutionFilePath.Returns(file);

            target.HandleRequest(HttpContext);

            Assert.That(items[Constants.SessionId], Is.EqualTo(clientSessionId));
        }

        [Test]
        public void Should_not_add_value_when_request_is_for_for_browserLink()
        {
            HttpRequest.Url.Returns(new Uri("http://www.test.com/__browserLink/requestData/fa2b198ba711488f9586768be6fe5d25?version=2"));

            HttpRequest.CurrentExecutionFilePathExtension.Returns("");
            HttpRequest.CurrentExecutionFilePath.Returns("/__browserLink/requestData/fa2b198ba711488f9586768be6fe5d25");

            target.HandleRequest(HttpContext);

            Assert.That(items.Keys, Has.No.Member(Constants.SessionId));
        }
    }
}