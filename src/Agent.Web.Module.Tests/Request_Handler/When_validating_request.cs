using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Request_Handler
{
    [TestFixture]
    public class When_validating_request: TestBase
    {

        [Test]
        public void Should_return_400_if_no_content_in_stream()
        {
            SendRequest(null);

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Should_return_400_if_content_exceeds_20k()
        {
            using (var stream = new MemoryStream())
            using(var writer = new StreamWriter(stream))
            {
                do
                {
                    writer.Write('X');
                    writer.Flush();
                } while (stream.Length < 204801);

                HttpRequest.Url.Returns(new Uri(LogUrl));
                HttpRequest.InputStream.Returns(stream);

                Target.HandleRequest(HttpContext);
            }

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(413));
        }
    }
}