using System;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.CORS_Handler
{
    [TestFixture]
    public class When_receiving_a_CORS_pre_flight_request:CORSTestBase
    {
        
        [Test]
        public void Should_pass_request_on_if_not_for_loupe([Values("http://www.test.com/",
                                                                    "http://www.test.com/Gibraltar",
                                                                    "http://www.test.com/Gibraltar/log/things",
                                                                    "http://www.test.com/gibraltar/data")] string url)
        {
            HttpRequest.Url.Returns(new Uri(url));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void Should_not_handle_if_no_origin_header()
        {
            HttpRequest.Url.Returns(new Uri("http://test.com/gibraltar/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void Should_respond_with_appropriate_headers()
        {
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin","http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Methods"],Is.EqualTo("POST"));
        }

        [Test]
        public void Should_include_allow_headers_if_present_on_request()
        {
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            HttpRequest.Headers.Add("Access-Control-Request-Headers", "content-type");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Headers"], Is.EqualTo("content-type"));            
        }

        [Test]
        public void Should_include_max_age_header_if_not_in_config()
        {
            FakeConfigProvider.GlobalMaxAge.Returns(false);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Max-Age"], Is.Not.Null);            
        }

        [Test]
        public void Should_not_include_max_age_header_if_set_in_config()
        {
            FakeConfigProvider.GlobalMaxAge.Returns(true);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Max-Age"], Is.Null);
        }

        [Test]
        public void Should_not_add_allow_origin_header_if_in_customHeaders_in_config()
        {
            FakeConfigProvider.GlobalAllowOrigin.Returns(true);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Allow-Origin"], Is.Null);
        }

        [Test]
        public void Should_not_add_allow_origin_header_if_not_set_in_config()
        {
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Allow-Origin"], Is.EqualTo("http://www.mysite.com/loupe/log"));
        }

        [Test]
        public void Should_not_add_allow_headers_if_set_in_config()
        {
            FakeConfigProvider.GlobalAllowHeaders.Returns(true);
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            HttpRequest.Headers.Add("Access-Control-Request-Headers", "content-type");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Headers"], Is.Null);                  
        }

        [Test]
        public void Should_return_500_if_error_during_processing()
        {
            FakeConfigProvider.GlobalAllowOrigin.Returns(x => { throw new Exception("Error"); });
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);
            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(500));
        }
    } 
}