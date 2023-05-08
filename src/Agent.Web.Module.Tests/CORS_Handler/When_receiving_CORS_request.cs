using System;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.CORS_Handler
{
    [TestFixture]
    public class When_receiving_CORS_request:CORSTestBase
    {
        [Test]
        public void Should_not_return_true_to_indicate_handled_request()
        {
            HttpRequest.HttpMethod.Returns("POST");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
        }

        [Test]
        public void Should_return_405_for_a_method_that_is_not_supported()
        {
            HttpRequest.HttpMethod.Returns("PUT");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(405));
        }

        [Test]
        public void Should_add_allow_origin_if_not_set_in_config()
        {
            FakeConfigProvider.GlobalAllowOrigin.Returns(false);
            HttpRequest.HttpMethod.Returns("POST");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Origin"], Is.EqualTo("http://www.mysite.com/loupe/log"));
        }

        [Test]
        public void Should_not_add_allow_headers_if_not_set_in_config()
        {
            FakeConfigProvider.GlobalAllowHeaders.Returns(false);
            HttpRequest.HttpMethod.Returns("POST");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            HttpRequest.Headers.Add("Access-Control-Request-Headers", "content-type");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Headers"], Is.Null);              
        }

        [Test]
        public void Should_return_500_if_error_during_processing()
        {
            FakeConfigProvider.GlobalAllowOrigin.Returns(x => { throw new Exception("Error"); });
            HttpRequest.HttpMethod.Returns("POST");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);
            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(500));
        }
    }
}