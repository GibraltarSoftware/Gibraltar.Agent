
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Gibraltar.Agent.Data;
using NUnit.Framework;

namespace Gibraltar.Agent.Test.LogMessages
{
    [TestFixture]
    public class MessageAlertTests
    {
        private readonly object m_Lock = new object();
        private int m_Count;
        private TimeSpan m_Latency;
        private TimeSpan m_Span;

        private void Log_MessageAlertMeasurement(object sender, LogMessageAlertEventArgs e)
        {
            int count = e.TotalCount;
            TimeSpan latency = DateTimeOffset.Now - e.Messages[0].Timestamp;
            TimeSpan span = e.Messages[count - 1].Timestamp - e.Messages[0].Timestamp;

            lock (m_Lock)
            {
                m_Latency = latency;
                m_Span = span;
                m_Count = count;

                System.Threading.Monitor.PulseAll(m_Lock);
            }
        }

        private int WaitForEvent(out TimeSpan latency, out TimeSpan span)
        {
            int count;
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            TimeSpan timeoutWait = new TimeSpan(0, 0, 1); // Wait up to 1 second (1000 ms) for the event to fire.
            DateTimeOffset endTime = startTime + timeoutWait;
            lock (m_Lock)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow; // Grab it again; we don't know how long we waited for the lock.
                timeoutWait = endTime - now;
                while (m_Count == 0 && timeoutWait > TimeSpan.Zero)
                {
                    System.Threading.Monitor.Wait(m_Lock, timeoutWait);

                    now = DateTimeOffset.UtcNow;
                    timeoutWait = endTime - now;
                }

                latency = m_Latency;
                span = m_Span;
                count = m_Count;

                m_Count = 0;
                m_Latency = TimeSpan.Zero;
                m_Span = TimeSpan.Zero;
            }
            if (count > 0)
                Trace.TraceInformation("Message Alert event received: Count = {0}; Latency = {1:F4} ms; Span = {2:F4} ms;",
                                       count, latency.TotalMilliseconds, span.TotalMilliseconds);
            else
                Trace.TraceInformation("WaitForEvent timed out {0:F4} ms after target (1000 ms).", timeoutWait.Negate().TotalMilliseconds);

            return count;
        }

        [Test]
        public void MessageAlertTest()
        {
            try
            {
                Log.Verbose(LogWriteMode.WaitForCommit, "Gibraltar.Agent.Unit Tests.MessageAlert", "Pre-test flush", null);

                Log.MessageAlert += Log_MessageAlertMeasurement;
                TimeSpan latency;
                TimeSpan span;

                DateTimeOffset start = DateTimeOffset.UtcNow;
                int count = WaitForEvent(out latency, out span);
                Assert.AreEqual(0, count, "WaitForEvent did not timeout as expected");
                span = DateTimeOffset.UtcNow - start;
                Assert.GreaterOrEqual(span.TotalMilliseconds, 1000, "WaitForEvent timed out in less than 1000 ms.");
                Assert.LessOrEqual(span.TotalMilliseconds, 1050, "WaitForEvent took more than 1050 ms to time out.");

                Log.Warning("Gibraltar.Agent.Unit Tests.MessageAlert", "Single warning to test Message Alert", null);
                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.AreEqual(1, count, "Message Alert event included more than the expected message.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 250, "Initial event latency exceeded 250 ms");

                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Single error to test Message Alert", null);
                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.AreEqual(1, count, "Message Alert event included more than the expected message.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 75, "Second event latency exceeded 75 ms");

                Log.Critical("Gibraltar.Agent.Unit Tests.MessageAlert", "Single critical to test Message Alert", null);
                Log.Information("Gibraltar.Agent.Unit Tests.MessageAlert", "Single information to test Message Alert", null);
                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 75, "Event latency exceeded 72 ms");

                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple error to test Message Alert", "1 of 3");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Single verbose to test Message Alert", null);
                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple error to test Message Alert", "2 of 3");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Double verbose to test Message Alert", "1 of 2");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Double verbose to test Message Alert", "2 of 2");
                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple error to test Message Alert", "3 of 3");
                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.LessOrEqual(count, 3, "Message Alert event included extra messages.");
                Assert.AreEqual(3, count, "Message Alert did not include the expected burst of 3 messages.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 75, "Event latency exceeded 75 ms");

                Log.Warning("Gibraltar.Agent.Unit Tests.MessageAlert", "Warning in burst to test Message Alert", "1 of 3");
                Log.Information("Gibraltar.Agent.Unit Tests.MessageAlert", "Double information to test Message Alert", "1 of 2");
                Log.Information("Gibraltar.Agent.Unit Tests.MessageAlert", "Double information to test Message Alert", "2 of 2");
                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Error in burst to test Message Alert", "2 of 3");
                Log.Information("Gibraltar.Agent.Unit Tests.MessageAlert", "Single information to test Message Alert", null);
                Log.Critical("Gibraltar.Agent.Unit Tests.MessageAlert", "Critical in burst to test Message Alert", "3 of 3");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple verbose to test Message Alert", "1 of 3");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple verbose to test Message Alert", "2 of 3");
                Log.Verbose("Gibraltar.Agent.Unit Tests.MessageAlert", "Triple verbose to test Message Alert", "3 of 3");
                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.LessOrEqual(count, 3, "Message Alert event included extra messages.");
                Assert.AreEqual(3, count, "Message Alert did not include the expected burst of 3 messages.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 75, "Event latency exceeded 75 ms");

                for (int i=0; i < 50; i++)
                    Log.Warning("Gibraltar.Agent.Unit Tests.MessageAlert", "Numerous warnings to test Message Alert",
                                "{0} of 50", i);

                count = WaitForEvent(out latency, out span);
                Assert.AreNotEqual(0, count, "Message Alert event didn't fire within 1 second timeout.");
                Assert.LessOrEqual(latency.TotalMilliseconds, 100, "Event latency exceeded 100 ms");
                Assert.LessOrEqual(span.TotalMilliseconds, 75, "Event spanned more than 75 ms");
                Assert.AreEqual(50, count, "Event did not include all 50 warnings");
            }
            finally
            {
                Log.MessageAlert -= Log_MessageAlertMeasurement;
            }
        }

        [Ignore("local debugging test")]
        [Test]
        public void MessageAlertDemo()
        {
            try
            {
                Log.MessageAlert += Log_MessageAlert;
                Log.Warning("Gibraltar.Agent.Unit Tests.MessageAlert", "This should not trigger a package", "Because this is not an error");
                Log.Error("Gibraltar.Agent.Unit Tests.MessageAlert", "Red Alert!", "Here is an error that definitely should cause our handler to package up stuff!");
            }
            finally
            {
                Log.MessageAlert -= Log_MessageAlert;
            }
        }

        [Ignore("local debugging test")]
        [Test]
        public void MessageAlertSendEmail()
        {
            try
            {
                m_MessageAlertProcessed = false;
                Log.MessageAlert += Log_MessageAlertSendEmail;
                InvalidOperationException innerException = new InvalidOperationException("This is our innermost exception");
                InvalidOperationException middleException = new InvalidOperationException("This is a middle exception", innerException);
                ArgumentException outerException = new ArgumentException("This is our outermost exception", middleException);

                Log.Error(outerException, "Gibraltar.Agent.Unit Tests.MessageAlert", "Red Alert!", "Here is an error that definitely should cause our handler to package up stuff!");
                
                //now we need to wait until our alert really fires before returning.
                DateTimeOffset waitExpiration = DateTimeOffset.Now.AddMinutes(1);
                while ((m_MessageAlertProcessed == false) && (waitExpiration > DateTimeOffset.Now))
                {
                    Thread.Sleep(500);
                }

                Thread.Sleep(500);

                Assert.IsTrue(m_MessageAlertProcessed, "Message alert was not successful.");
            }
            finally
            {
                Log.MessageAlert -= Log_MessageAlertSendEmail;
            }
        }

        private void Log_MessageAlert(object sender, LogMessageAlertEventArgs e)
        {
            //if there are any errors (or worse - criticals) we want to send the
            //up to date information on this session immediately.
            if (e.TopSeverity <= LogMessageSeverity.Error) //numeric values DROP for more severe enum values
            {
                //set our auto-send to true.
                e.SendSession = true;

                //and lets make sure we don't send again for at least a few minutes
                //to ensure we don't flood in the event of a storm of errors.
                e.MinimumDelay = new TimeSpan(0, 5, 0); //5 minutes
            }
        }

        private bool m_MessageAlertProcessed;

        private void Log_MessageAlertSendEmail(object sender, LogMessageAlertEventArgs e)
        {
            //if we had an error or critical we want to send
            if (e.TopSeverity <= LogMessageSeverity.Error) //numeric values DROP for more severe enum values
            {
                //get the set of messages that are the "worst" in this event.
                List<ILogMessage> badMessages = new List<ILogMessage>();
                foreach (ILogMessage message in e.Messages)
                {
                    if (message.Severity == e.TopSeverity)
                    {
                        badMessages.Add(message);
                    }
                }

                //now make us an email message describing these guys
                string messageBody = FormatMessageBody(badMessages);
                string subject = string.Format("{0} {1}: {2}", Log.SessionSummary.Application, 
                    e.TopSeverity.ToString().ToUpper(), badMessages[0].Caption);

                //and for safety's sake lets keep our subject from being crazy long.
                if (subject.Length > 120)
                {
                    subject = subject.Substring(0, 120) + "...";
                }

                //now that we've done all of that, lets send our message using the Agent's email config
                using (MailMessage message = new MailMessage())
                {
                    message.Subject = subject;
                    message.Body = messageBody;
                    message.Priority = MailPriority.High;

                    //now send our email!  I'm not bothering to catch exceptions since the Agent handles that nicely for us
                    Log.SendMessage(message); //synchronous OK because we're already async from the flow of logging.
                }

                //and lets make sure we don't send again for at least a few minutes
                //to ensure we don't flood in the event of a storm of errors.
                e.MinimumDelay = new TimeSpan(0, 5, 0); //5 minutes
                m_MessageAlertProcessed = true;
            }
        }

        private string FormatMessageBody(List<ILogMessage> messages)
        {
            StringBuilder messageBody = new StringBuilder(1024);

            //we write out more detail about the first item, then just summarize.
            ILogMessage firstMessage = messages[0];

            messageBody.AppendFormat("Timestamp: {0:g}\r\n", firstMessage.Timestamp);
            messageBody.AppendFormat("Category:  {0}\r\n", firstMessage.CategoryName);
            messageBody.AppendFormat("Class:     {0}\r\n------------------------------\r\n", firstMessage.ClassName);
            messageBody.AppendFormat("{0}\r\n", firstMessage.Caption);
            messageBody.AppendFormat("{0}\r\n\r\n", firstMessage.Description);

            //report any exceptions on this first object.
            IExceptionInfo currentException = firstMessage.Exception;

            if (currentException != null)
            {
                messageBody.Append("Exceptions:\r\n");
                while (currentException != null)
                {
                    messageBody.AppendFormat("{0}: {1}\r\n\r\n", currentException.TypeName, currentException.Message);

                    //Each outer exception can point to an inner exception, we get null when there are no more.
                    currentException = currentException.InnerException;
                }
            }

            //summarize the rest of the messages
            if (messages.Count > 1)
            {
                messageBody.AppendFormat("Other {0}s:\r\n", firstMessage.Severity);
                for (int curMessageIndex = 1; curMessageIndex < messages.Count; curMessageIndex++)
                {
                    ILogMessage currentMessage = messages[curMessageIndex];
                    messageBody.AppendFormat("------------------------------\r\nMessage {0} of {1}: {2}: {3}\r\n\r\n", 
                        curMessageIndex, messages.Count, currentMessage.Severity, currentMessage.Caption);
                }
            }

            return messageBody.ToString();
        }
    }
}
