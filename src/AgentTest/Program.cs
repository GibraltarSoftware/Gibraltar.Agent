
#pragma warning disable 162, 169, 429

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Agent.Data;
using Gibraltar.Agent.Net;

namespace Gibraltar.Agent.Test
{
    static class Program
    {
        private const bool AllowLogging = true;
        private const bool CancelFirstInitialization = false;
        private static bool g_FirstInitializationPassed;

        private static bool g_ReentryDetection;
        private static string g_ApplicationName;

        private static int g_WarningTotal;
        private static int g_ErrorTotal;
        private static int g_CriticalTotal;
        private static ILogMessage g_FirstError;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol
                                                   | SecurityProtocolTypeExtensions.Tls12;

            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.NoPrincipal);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //lets jack with the culture for fun.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-es");

            //Connect to the log initializing event before we do ANYTHING with the agent so we can 
            //configure it and even cancel it.
            Log.Initializing += Log_Initializing;

            Trace.TraceInformation("Application starting.");

            Application.Run(new frmMain());

            Trace.TraceInformation("Application exiting.");
            Trace.Close();
        }


        /*
        static void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            if (g_ReentryDetection)
            {
                g_ApplicationName = "Reentrant Application";
                e.Configuration.Publisher.ApplicationName = g_ApplicationName;
                return;
            }
            g_ApplicationName = "Override Application";
            g_ReentryDetection = true;

            Log.TraceInformation("Attempt to log from within Log_Initializing() event handler.");

            Trace.TraceInformation("Attempt to do Trace from within Log_Initializing() event handler.");

            //you can use the publisher configuration to override the product & application name
            e.Configuration.Publisher.ProductName = "Override Product";
            e.Configuration.Publisher.ApplicationName = g_ApplicationName; // "Override Application", unless we reentered!

            //you can also add name/value pairs to be recorded in the session header for improved
            //categorization and analysis later.
            e.Configuration.Properties.Add("CustomerName", "The customer name from our license");

            //you can also use the cancel feature combined with #define to turn off the agent at compile time.
#if (!DEBUG)
            //this is not a debug compile, disable the agent.
            e.Cancel = true;
#endif
            g_ReentryDetection = false;
        }
*/

        static void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            if (AllowLogging == false)
            {
                e.Cancel = true;
                g_FirstInitializationPassed = true;
                return;
            }

            if ((CancelFirstInitialization) && (g_FirstInitializationPassed == false))
            {
                e.Cancel = true;
                g_FirstInitializationPassed = true;
                return;
            }

//            e.Configuration.Server.UseGibraltarService = false;
//            e.Configuration.Server.Server = "testhub.gibraltarsoftware.com";
//            e.Configuration.Server.Server = "localhost";
//            e.Configuration.Server.Port = 58330;
//            e.Configuration.Server.ApplicationBaseDirectory = "Customers/Gibraltar Software";
            e.Configuration.Server.AutoSendSessions = true;
            e.Configuration.Server.PurgeSentSessions = true;

            e.Configuration.Publisher.ProductName = "Loupe";
            e.Configuration.Publisher.ApplicationName = "Agent Test Tool";
            e.Configuration.Publisher.ApplicationVersion = new Version(2,4);

#if DEBUG
            e.Configuration.Publisher.EnableDebugMode = true;
#endif

            e.Configuration.AutoSendConsent.Enabled = true;
            e.Configuration.AutoSendConsent.CompanyName = "Your Software Company";
            e.Configuration.AutoSendConsent.ServiceName = "Customer Service Platform";
            e.Configuration.AutoSendConsent.PrivacyPolicyUrl = "http://www.gibraltarsoftware.com";
            e.Configuration.AutoSendConsent.PromptUserOnStartupLimit = 5;
            e.Configuration.AutoSendConsent.ConsentDefault = false;

            e.Configuration.NetworkViewer.Enabled = true;
            e.Configuration.NetworkViewer.AllowLocalClients = true;
            e.Configuration.NetworkViewer.AllowRemoteClients = true;

            Log.MessageAlert += Log_ErrorAlertNotification;

            e.Configuration.Listener.EnableMemoryPerformance = true;
            /*
            e.Configuration.Listener.EnableDiskPerformance = false;
            e.Configuration.Listener.EnableNetworkPerformance = false;
            e.Configuration.Listener.EnableProcessPerformance = false;
            e.Configuration.Listener.EnableSystemPerformance = false;*/

            //Log.ServerAuthenticationProvider = new BasicAuthenticationProvider("kendall", "myPassword");
        }

        static void Log_ErrorAlertNotification(object sender, LogMessageAlertEventArgs e)
        {
            g_WarningTotal += e.WarningCount;
            g_ErrorTotal += e.ErrorCount;
            g_CriticalTotal += e.CriticalCount;

            TimeSpan latency = DateTimeOffset.Now - e.Messages[0].Timestamp;
            TimeSpan span = e.Messages[e.TotalCount - 1].Timestamp - e.Messages[0].Timestamp;
            Log.TraceInformation("Error Alert Notification latency report\r\nCount = {0}\r\nLatency = {1:F4} ms\r\nSpan = {2:F4} ms",
                                 e.TotalCount, latency.TotalMilliseconds, span.TotalMilliseconds);

            if (g_FirstError == null)
            {
                for (int i = 0; i < e.TotalCount; i++)
                {
                    ILogMessage message = e.Messages[i];
                    if (message.Severity > LogMessageSeverity.Error) // Severities compare backwards.
                        continue; // Skip the Warnings.

                    if (g_FirstError == null)
                        g_FirstError = message;
                }
            }

            e.MinimumDelay = new TimeSpan(0, 0, 10);

            // Now try screwing with the messages in the event.

            ILogMessageCollection messageList = e.Messages;
            //messageList.Clear();
            //if (g_FirstError != null)
            //    messageList.Add(g_FirstError);

            //int index = messageList.IndexOf(g_FirstError);
            //if (index >= 0)
            //    messageList[index] = null;
        }
    }
}
