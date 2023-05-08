using System;
using Gibraltar.Agent.Web.Module.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Infrastructure
{
    [TestFixture]
    public class When_checking_url:TestBase
    {
        [Test]
        public void Should_detect_loupe_url([Values("http://www.test.com/loupe/log",
            "http://www.test.com/Loupe/log",
            "http://www.test.com/loupe/Log",
            "http://www.test.com/Loupe/Log",
            "http://www.test.com/loupe/log/")] string url)
        {
            HttpRequest.Url.Returns(new Uri(url));
           var checker = new UrlCheck();

            Assert.That(checker.IsLoupeUrl(HttpContext), Is.True);
        }

        [Test]
        public void Should_indicate_not_loupe_url([Values("http://www.test.com/",
                                                   "http://www.test.com/Gibraltar",
                                                   "http://www.test.com/Gibraltar/log/things",
                                                   "http://www.test.com/gibraltar/data")] string url)
        {
            HttpRequest.Url.Returns(new Uri(url));
            var checker = new UrlCheck();

            Assert.That(checker.IsLoupeUrl(HttpContext), Is.False);
        }
    }
}