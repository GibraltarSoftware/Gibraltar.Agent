using System;
using System.Linq;
using System.Threading;
using Gibraltar.Agent;
using Gibraltar.Agent.Web.Module.Models;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Controller
{
    /// <summary>
    /// All messages sent in these tests have their severity set to Warning to ensure that
    /// the Log.MessageAlert event is rasied simply so we can interrogate the details we
    /// have logged to ensure they are created correctly
    /// </summary>
    [TestFixture]
    public class When_logging_details:RequestProcessorTestBase
    {
        private ManualResetEventSlim _resetEvent;
        private LogMessageAlertEventArgs _eventArgs;

        private LogMessage _standardTestMessage;
        private LogRequestBuilder _requestBuilder;

        private const string ExpectedMethodSourceInfo =
            "<MethodSourceInfo><File>app.js</File><Line>3</Line><Column>5</Column></MethodSourceInfo>";

        private const string ExpectedClientDetails =
            "<ClientDetails><Description>Firefox 37.0 32-bit on Windows 8.1 64-bit</Description><Layout>Gecko</Layout><Name>Firefox</Name><UserAgentString>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgentString><Version>37.0</Version><OS><Architecture>64</Architecture><Family>Windows</Family><Version>8.1</Version></OS><Size><Height>873</Height><Width>1102</Width></Size></ClientDetails>";

        private const string ExpectedUserSuppliedJson = "<UserSupplied><numericValue>1</numericValue><stringValue>text value</stringValue><objectValue><childNumber>3</childNumber><childText>child text</childText></objectValue></UserSupplied>";

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

            _requestBuilder = new LogRequestBuilder();
            _standardTestMessage = new LogMessage
            {
                Severity = LogMessageSeverity.Error,
                Category = "Test",
                Caption = "test log",
                Description = "tests logs message",
                Sequence = 1
            };
        }

        [Test]
        public void Should_log_expected_timestamp_and_sequence()
        {
            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            _standardTestMessage.TimeStamp = timeStamp;

            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>"));
        }

        [Test]
        public void Should_not_log_timestamp_if_not_provided()
        {
            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Not.Contain("<TimeStamp>01/01/0001 00:00:00 +00:00</TimeStamp>"));            
        }

        [Test]
        public void Should_log_method_source_info_details()
        {
            _standardTestMessage.MethodSourceInfo = StdMethodSourceInfo;

            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain(ExpectedMethodSourceInfo));
        }

        [Test]
        public void Should_log_client_details()
        {
            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();
            
            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain(ExpectedClientDetails));
        }

        [Test]
        public void Should_output_user_supplied_JSON_as_xml()
        {
            _standardTestMessage.Details = "{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}";
            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain(ExpectedUserSuppliedJson));

        }

        [Test]
        public void Should_output_user_supplied_details_as_string()
        {
            _standardTestMessage.Details = "this is user supplied details";
            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<UserSupplied>this is user supplied details</UserSupplied>"));            
        }

        [Test]
        public void Should_output_session_id_in_details_block_even_if_no_session_details()
        {
            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<SessionId>" + Guid.Empty + "</SessionId>"));            
            
        }

        [Test]
        public void Should_output_session_id_in_details_block_when_session_details_exist()
        {
            var sessionId = Guid.NewGuid().ToString();
            SetContextLoupeSessionId(sessionId);

            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<SessionId>" + sessionId + "</SessionId>"));

        }

        [Test, Ignore("Raw debugging test")]
        public void Should_output_session_id_from_request_when_exists_and_no_cookie()
        {
            ClearLoupeSessionIdValue();

            //SendRequest("{Session:{sessionId: 'session-123'},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<SessionId>session-123</SessionId>"));            
            
        }

        [Test, Ignore("Raw debugging test")]
        public void Should_output_session_id_from_request_when_exists_even_if_cookie_present()
        {
            _standardTestMessage.SessionId = "session-123";
            var request = _requestBuilder.AddSession()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Does.Contain("<SessionId>session-123</SessionId>"));

        }

        [Test]
        public void Should_not_output_session_id_in_details_if_no_cookie_or_id_in_request()
        {
            ContextItems.Clear();

            var request = _requestBuilder.AddSession()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);
            
            Assert.That(loggedMessage.Details, Does.Not.Contain("<SessionId>"));
        }

        [Test]
        public void Should_output_expected_details_block()
        {

            var sessionId = Guid.NewGuid().ToString();

            SetContextLoupeSessionId(sessionId);

            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            _standardTestMessage.TimeStamp = timeStamp;
            _standardTestMessage.MethodSourceInfo = StdMethodSourceInfo;
            _standardTestMessage.Details = "{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}";
            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetailsBlock = "<Details><SessionId>" + sessionId + "</SessionId><AgentSessionId>" + DefaultAgentSessionId + "</AgentSessionId><TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>" +
                                       ExpectedClientDetails + ExpectedMethodSourceInfo + ExpectedUserSuppliedJson + "</Details>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetailsBlock));
        }

        [Test]
        public void Should_cache_client_details()
        {
            //SendRequest("{session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},logMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            var expected = CreateClientDetails();

            Assert.That(HttpContext.Cache[DefaultTestSessionId], Is.EqualTo(expected));

        }

        [Test]
        public void Should_not_remove_cached_item_if_no_client_details_sent_on_subsequent_request() {
            var existingDetails = CreateClientDetails();
            
            HttpContext.Cache.Insert(DefaultTestSessionId, existingDetails);

            var request = _requestBuilder.AddMessage(_standardTestMessage).Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            Assert.That(HttpContext.Cache[DefaultTestSessionId], Is.EqualTo(existingDetails));
        }

        [Test]
        public void Should_not_add_multiple_details_to_cache_for_same_session() {
            var existingDetails = CreateClientDetails();

            HttpContext.Cache.Insert(DefaultTestSessionId, existingDetails);

            _standardTestMessage.MethodSourceInfo = StdMethodSourceInfo;
            var request = _requestBuilder.AddSession()
                                         .AddClientDetails()
                                         .AddMessage(_standardTestMessage)
                                         .Build();

            Target.Process(HttpContext, request);

            WaitForEvent();

            Assert.That(HttpContext.Cache.Count, Is.EqualTo(1));
        }

        private string CreateClientDetails()
        {
            return "<ClientDetails><Description>Firefox 37.0 32-bit on Windows 8.1 64-bit</Description><Layout>Gecko</Layout><Name>Firefox</Name><UserAgentString>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgentString><Version>37.0</Version><OS><Architecture>64</Architecture><Family>Windows</Family><Version>8.1</Version></OS><Size><Height>873</Height><Width>1102</Width></Size></ClientDetails>";
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
}