using System;
using System.Linq;
using System.Threading;
using System.Web;
using Gibraltar.Agent;
using Gibraltar.Agent.Data;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Controller
{
#if DEBUG
    [TestFixture]
    public class When_logging_internal_exceptions:RequestProcessorTestBase
    {
        private ManualResetEventSlim _resetEvent;
        private LogMessageAlertEventArgs _eventArgs;
        private HttpBrowserCapabilitiesBase _fakeBrowser;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            Log.MessageAlert += Log_MessageAlert;
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            Log.MessageAlert -= Log_MessageAlert;
        }

        [SetUp]
        public void SetUp()
        {
            _eventArgs = null;
            _resetEvent = new ManualResetEventSlim();

        }

        [Test]
        public void Should_output_request_details_even_if_fails_construction_of_block()
        {

            _fakeBrowser = Substitute.For<HttpBrowserCapabilitiesBase>();
            HttpRequest.Browser.Returns(x => null);

            HttpRequest.ContentLength.Returns(10);
            HttpRequest.ContentType.Returns("application/json");
            HttpRequest.IsLocal.Returns(false);
            HttpRequest.IsSecureConnection.Returns(true);
            HttpRequest.UserHostAddress.Returns("216.58.211.4");
            HttpRequest.UserHostName.Returns("www.google.com");

            HttpRequest.HttpMethod.Returns("POST");

            Target.Process(HttpContext, null);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault(m => !string.IsNullOrEmpty(m.Details));

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetails =
                "<Request>We were unable to record details from the Request itself due to an exception occurring whilst extracting information from the Request.</Request>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetails));

        }


        void Log_MessageAlert(object sender, LogMessageAlertEventArgs e)
        {
            _eventArgs = e;
            e.MinimumDelay = new TimeSpan(0);
            _resetEvent.Set();
        }

        private void WaitForEvent()
        {
            _resetEvent.Wait(new TimeSpan(0, 0, 0, 5));
        }         
    }
#endif
}