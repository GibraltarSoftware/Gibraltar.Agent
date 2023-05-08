using System.Collections.Generic;
using Gibraltar.Agent.Web.Module.Infrastructure;
using Gibraltar.Agent.Web.Module.Models;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Infrastructure
{
    [TestFixture]
    public class When_providing_source_information
    {
        private JavaScriptSourceProvider _sourceProvider;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _sourceProvider = new JavaScriptSourceProvider();
        }

        [Test]
        public void Should_have_null_for_properties_where_no_MethodSourceInfo_or_Exception()
        {
            var message = new LogMessage();
            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.Null);
            Assert.That(target.LineNumber, Is.EqualTo(0));
        }

        [Test]
        public void Should_have_properties_set_from_MethodSourceInfo()
        {
            var message = new LogMessage
            {
                MethodSourceInfo = new MethodSourceInfo
                {
                    File = "app.js",
                    Line = 5
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.EqualTo("app.js"));
            Assert.That(target.LineNumber, Is.EqualTo(5));
        }


        [Test]
        public void Should_have_method_set_even_if_no_file()
        {
            var message = new LogMessage
            {
                MethodSourceInfo = new MethodSourceInfo
                {
                    Method = "theFunction",
                    Line = 5
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("theFunction"));
            Assert.That(target.FileName, Is.Null);
            Assert.That(target.LineNumber, Is.EqualTo(5));
        }

        [Test]
        public void Should_have_line_number_even_if_has_no_other_info()
        {
            var message = new LogMessage
            {
                MethodSourceInfo = new MethodSourceInfo
                {
                    Line = 5
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.Null);
            Assert.That(target.LineNumber, Is.EqualTo(5));
        }

        [Test]
        public void Should_create_method_source_info_even_if_only_has_column_number()
        {
            var message = new LogMessage
            {
                MethodSourceInfo = new MethodSourceInfo
                {
                    Column = 5
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.Null);
            Assert.That(target.LineNumber, Is.EqualTo(0));
        }

        [Test]
        public void Should_have_properties_set_from_firefox_stack()
        {
            var stack = new List<string>
            {
                "InnerItem/this.throwUnitializeError@http://localhost:3378/spec/helpers/Utils.js:92:9",
                "TestingStack/this.createError@http://localhost:3378/spec/helpers/Utils.js:74:9",
                "throwUninitializeError/<@http://localhost:3378/spec/When_logging_stack_trace.js:125:13"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "uninitializedObject is undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("InnerItem/this.throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }

        [Test]
        public void Should_have_properties_set_from_chrome_stack()
        {
            var stack = new List<string>
            {
                "TypeError: Cannot read property 'doStuff' of undefined",
                "    at InnerItem.throwUnitializeError (http://localhost:3378/spec/helpers/Utils.js:92:28)",
                "    at TestingStack.createError (http://localhost:3378/spec/helpers/Utils.js:74:15)",
                "    at http://localhost:3378/spec/When_logging_stack_trace.js:125:20"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "Uncaught TypeError: Cannot read property 'doStuff' of undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("InnerItem.throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }

        [Test]
        public void Should_have_properties_set_from_ie_stack()
        {
            var stack = new List<string>
            {
                "TypeError: Unable to get property 'doStuff' of undefined or null reference",
                "   at throwUnitializeError (http://localhost:3378/spec/helpers/Utils.js:92:9)",
                "   at createError (http://localhost:3378/spec/helpers/Utils.js:74:9)",
                "   at Anonymous function (http://localhost:3378/spec/When_logging_stack_trace.js:125:13)"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "Uncaught TypeError: Cannot read property 'doStuff' of undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }
    }
}