
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Gibraltar.Agent.Test.LogMessages;
using Gibraltar.Agent.Test.Metrics;
using Gibraltar.Agent.Test.Packaging;

namespace Gibraltar.Agent.Test
{
    public partial class frmMain : Form
    {
        private const int MessagesPerTest = 1500;

        private bool m_BeginEventReceived;

        public frmMain()
        {
            Trace.TraceInformation("Starting Main Form");
            InitializeComponent();

            // Put our Process ID in the initial subject line.
            txtSubject.Text = String.Format(txtSubject.Text, Process.GetCurrentProcess().Id);

            ResetEvents();
        }

        private void ResetEvents()
        {
            m_BeginEventReceived = false;
            lblBeginSend.Enabled = false;
            lblEndSend.Enabled = false;
        }

        private void DisableCommands()
        {
            btnSendEmail.Enabled = false;
            btnSendEmailAsync.Enabled = false;
            btnSendEmailManual.Enabled = false;
            btnSendEmailManualAsync.Enabled = false;
        }

        private void EnableCommands()
        {
            btnSendEmail.Enabled = true;
            btnSendEmailAsync.Enabled = true;
            btnSendEmailManual.Enabled = true;
            btnSendEmailManualAsync.Enabled = true;
        }

        private void ThrowAnException()
        {
            throw new Exception("This is a test Exception.");
        }

        private void ActionCustomerTest(bool activeSessionsOnly)
        {
            string packageLocation = string.Empty;
            string filename = @"_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (string.IsNullOrEmpty(packageLocation))
            {
                //The location is not configured -- let's add them to the default location on the local machine
                packageLocation = @"c:\data\Monitoring\Packages";

                if (!Directory.Exists(packageLocation))
                {
                    Directory.CreateDirectory(packageLocation);
                }
            }

            filename = "Monitoring" + filename;
            packageLocation = Path.Combine(packageLocation, filename) + ".config.test";

            using (Gibraltar.Agent.Packager newPackage = new Gibraltar.Agent.Packager())
            {
                if (activeSessionsOnly)
                {
                    newPackage.SendToFile(SessionCriteria.ActiveSession, true, packageLocation);
                }
                else
                {
                    newPackage.SendToFile(SessionCriteria.NewSessions | SessionCriteria.ActiveSession, true, packageLocation);
                }
                //This is a synchronous send.  If you are doing this from a web server event you should use the Async equivalent method
                //But if you do it asynchronous do not immediately dispose the packager (like this using block does) or it will abort the send.
            }
        }

        private void btnSendEmail_Click(object sender, EventArgs e)
        {
            DisableCommands();
            
            using(Packager newPackager = new Packager("Gibraltar", null))
            {
                newPackager.SendEmail(SessionCriteria.AllSessions, false, txtSubject.Text);
            }

            EnableCommands();
        }

        private void btnSendEmailAsync_Click(object sender, EventArgs e)
        {
            DisableCommands();
            Packager newPackager = new Packager("Gibraltar", null);
            ResetEvents();
            newPackager.BeginSend += newPackager_BeginSend;
            newPackager.EndSend += newPackager_EndSend;

            newPackager.SendEmailAsync(SessionCriteria.AllSessions, false, txtSubject.Text);
        }

        private void btnSendEmailManual_Click(object sender, EventArgs e)
        {
            DisableCommands();

            using (Packager newPackager = new Packager("Gibraltar", null))
            {
                newPackager.SendEmail(SessionCriteria.AllSessions, false, txtSubject.Text, txtFromEmail.Text, txtToEmail.Text, txtServer.Text, txtUserName.Text, txtPassword.Text);
            }

            EnableCommands();
        }

        private void btnSendEmailManualAsync_Click(object sender, EventArgs e)
        {
            DisableCommands();
            Packager newPackager = new Packager("Gibraltar", null);
            ResetEvents();
            newPackager.BeginSend += newPackager_BeginSend;
            newPackager.EndSend += newPackager_EndSend;

            newPackager.SendEmailAsync(SessionCriteria.AllSessions, false, txtSubject.Text, txtFromEmail.Text, txtToEmail.Text, txtServer.Text, txtUserName.Text, txtPassword.Text);
        }

        private void newPackager_EndSend(object sender, PackageSendEventArgs args)
        {
            if (InvokeRequired)
            {
                PackageSendEventHandler d = newPackager_EndSend;
                Invoke(d, new []{sender, args });
            }
            else
            {
                if (m_BeginEventReceived == false)
                    MessageBox.Show("No begin send event received, just an end send event");

                lblEndSend.Enabled = true;
                EnableCommands();            
            }
        }

        private void newPackager_BeginSend(object sender, EventArgs args)
        {
            if (InvokeRequired)
            {
                EventHandler d = newPackager_BeginSend;
                Invoke(d, new[] { sender, args });
            }
            else
            {
                if (m_BeginEventReceived)
                    MessageBox.Show("Redundant begin send event received");

                m_BeginEventReceived = true;
                lblBeginSend.Enabled = true;
            }
        }

        private void btnAsyncPerf_Click(object sender, EventArgs e)
        {
            btnAsyncPerf.Enabled = false;

            //now that we know it's flushed everything, lets do our timed loop.
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            for (int curMessage = 0; curMessage < MessagesPerTest; curMessage++)
            {
                string caption = string.Format("Test Message {0} Caption", curMessage);
                string description = string.Format("Test Message {0} Description, with some content added to it's at least the size you'd expect a normal description to be of a message", curMessage);
                Log.Verbose(LogWriteMode.Queued, "Test.Agent.LogMessages.Performance", caption, description);
            }
            DateTimeOffset messageEndTime = DateTimeOffset.UtcNow;

            //one wait for commit message to force the buffer to flush.
            //Log.Write(LogMessageSeverity.Verbose, LogWriteMode.WaitForCommit, "Test.Agent.LogMessages.Performance", "Committing performance test", null);

            //and store off our time
            DateTimeOffset endTime = DateTimeOffset.UtcNow;

            TimeSpan duration = endTime - startTime;

            Trace.TraceInformation("Async Write Test Completed in {0}ms.  {1} messages were written at an average duration of {2}ms per message.  The flush took {3}ms.",
                                   duration.TotalMilliseconds, MessagesPerTest, (duration.TotalMilliseconds) / MessagesPerTest, (endTime - messageEndTime).TotalMilliseconds);

            btnAsyncPerf.Enabled = true;
        }

        private void btnAsyncWriteMessagePerf_Click(object sender, EventArgs e)
        {
            PerformanceTests perfTests = new PerformanceTests();
            perfTests.Setup();
            perfTests.AsyncPassThrough();
        }

        private void btnAllPerfTests_Click(object sender, EventArgs e)
        {
            btnAllPerfTests.Enabled = false;

            PerformanceTests perfTests = new PerformanceTests();
            perfTests.MessagesPerTest = 10000;
            perfTests.Setup();
            perfTests.AsyncPassThrough();
            perfTests.SynchronousPassThrough();
            perfTests.AsyncMessage();
            perfTests.SyncMessage();
            perfTests.TraceDirectCaption();
            perfTests.TraceDirectCaptionDescription();
            btnAllPerfTests.Enabled = true;
        }

        private void btnPackageWizard_Click(object sender, EventArgs e)
        {
            try
            {
                //create a new packager dialog for the user and start the process to send data
                //for the current application.
                PackagerDialog packagerDialog = new PackagerDialog();
                DialogResult result = packagerDialog.Send();
                if (result != DialogResult.OK)
                {
                    //The user may have canceled (DialogResult.Cancel) or 
                    //there may have been an error (DialogResult.Abort)
                    if (result == DialogResult.Abort)
                    {
                        Trace.TraceWarning("Package and send process generated an error for the user.");
                    }
                    else
                    {
                        Trace.TraceInformation("Package not sent.  Reason: " + result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to send package");
            }
        }

        private void btnEventManual_Click(object sender, EventArgs e)
        {
            btnEventManual.Enabled = false;

            EventMetricsByMethodsTests perfTests = new EventMetricsByMethodsTests();
            perfTests.Setup();
            perfTests.RecordEventMetricPerformanceTest();

            btnEventManual.Enabled = true;
        }

        private void btnEventReflection_Click(object sender, EventArgs e)
        {
            btnEventReflection.Enabled = false;

            EventMetricsByAttributesTests perfTests = new EventMetricsByAttributesTests();
            perfTests.RecordEventMetricReflectionPerformanceTest();

            btnEventReflection.Enabled = true;
        }

        private void btnSampledManual_Click(object sender, EventArgs e)
        {
            btnSampledManual.Enabled = false;

            SampledMetricsByMethodsTests perfTests = new SampledMetricsByMethodsTests();
            perfTests.PerformanceTest();

            btnSampledManual.Enabled = true;
        }

        private void btnSampledReflection_Click(object sender, EventArgs e)
        {
            btnSampledReflection.Enabled = false;

            SampledMetricsByAttributesTests perfTests = new SampledMetricsByAttributesTests();
            perfTests.PerformanceTest();

            btnSampledReflection.Enabled = true;
        }

        private void btnDatabaseEventMetricExample_Click(object sender, EventArgs e)
        {
            btnDatabaseEventMetricExample.Enabled = false;

            //this test was removed when open sourcing the agent

            btnDatabaseEventMetricExample.Enabled = true;
        }

        private void btnUnhandledForeground_Click(object sender, EventArgs e)
        {
            Trace.TraceInformation("Starting a foreground thread to throw an Exception.");
            Thread newThread = new Thread(WaitAndThrowException);
            newThread.SetApartmentState(ApartmentState.MTA);
            newThread.IsBackground = false;
            newThread.Name = "Foreground thread";
            newThread.Start();
        }

        private void btnUnhandledBackground_Click(object sender, EventArgs e)
        {
            Trace.TraceInformation("Starting a background thread to throw an Exception.");
            Thread newThread = new Thread(WaitAndThrowException);
            newThread.SetApartmentState(ApartmentState.MTA);
            newThread.IsBackground = true;
            newThread.Name = "Background thread";
            newThread.Start();
        }

        private void WaitAndThrowException()
        {
            Trace.TraceInformation("Thread to throw an exception has started.");
            Thread.Sleep(500);

            long nowTicks = DateTimeOffset.UtcNow.Ticks;
            nowTicks -= nowTicks % 1000;
            if (nowTicks % 10 != 1)
                throw new Exception("This is a test Exception.");

            Trace.TraceWarning("Thread has thrown an exception.");

            Thread.Sleep(500);
        }

        private void btnUnhandledUserInterface_Click(object sender, EventArgs e)
        {
            Trace.TraceWarning("Exception button clicked.");
            if (e != null)
                throw new NotImplementedException("Oops!");

            Trace.WriteLine("Exception thrown by click event handler on UI thread.");
            return;
        }

        private void btnGLVException_Click(object sender, EventArgs e)
        {
            FormCollection openForms = Application.OpenForms;
            Form glv = null;
            foreach (Form form in openForms)
            {
                Type formType = form.GetType();
                string typeName = formType.Name;
                if (typeName == "LiveViewerForm")
                {
                    glv = form;
                    break;
                }
            }
            if (glv != null)
            {
                Trace.WriteLine("BeginInvoke to GLV thread to throw an Exception.");
                glv.BeginInvoke(new MethodInvoker(ThrowAnException));
            }
            else
            {
                Trace.WriteLine("GLV form not found to BeginInvoke to it.");
            }
        }

        /// <summary>
        /// This is a temporary performance test meant to check the difference in VistaDB performance for different scenarios.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDatabaseReadTest_Click(object sender, EventArgs e)
        {
            btnDatabaseReadTest.Enabled = false;

            try
            {
                //this test was removed when open sourcing the agent
            }
            finally
            {
                btnDatabaseReadTest.Enabled = true;
            }
        }

        private void btnShowNotifier_Click(object sender, EventArgs e)
        {
            ApplicationException ex = new ApplicationException("This is the exception message that was originally generated by the source of the exception.");

            //this option records the exception but does not display any user interface.  
            Log.RecordException(ex, "Exceptions", true);

            //this option records the exception and displays a user interface, optionally waiting for the user 
            //to decide to continue or exit before returning.
            Log.ReportException(ex, "Exceptions", true, true);
        }

        private void btnNeverEndingLogging_Click(object sender, EventArgs e)
        {
            while(true)
            {
                btnAllPerfTests_Click(sender, e);
            }
        }

        private void btnStartSession_Click(object sender, EventArgs e)
        {
            btnStartSession.Enabled = false;

            Log.StartSession(0, "Starting Session by user request");

            btnStartSession.Enabled = true;
        }

        private void btnEndSession_Click(object sender, EventArgs e)
        {
            btnEndSession.Enabled = false;

            Log.EndSession(0, "Switching to Synchronous mode by user request to End Session");

            btnEndSession.Enabled = true;
        }

        private void btnPackagerUnitTests_Click(object sender, EventArgs e)
        {
            btnPackagerUnitTests.Enabled = false;

            PackagerTests unitTests = new PackagerTests();

            unitTests.Init();
            try
            {
                RunOneUnitTestWithTryCatch(unitTests.CreateActiveSessionPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateAllSessionsPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateCombinationSessionPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateCompletedSessionPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateCriticalSessionPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateEmptyPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateErrorSessionPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateNewSessionsPackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateNonePackage);
                RunOneUnitTestWithTryCatch(unitTests.CreateWarningSessionPackage);

                try
                {
                    unitTests.SendPackageViaEmailMissingArgs(); // Should this one be removed?
                }
                catch 
                {
                }

                RunOneUnitTestWithTryCatch(unitTests.SendPackageViaEmailOverrideServer);
                //RunOneUnitTestWithTryCatch(unitTests.SendPackageViaEmailOverrideServerAndUser);
                //RunOneUnitTestWithTryCatch(unitTests.SendPackageViaEmailOverrideServerAndUser);
            }
            finally
            {
                unitTests.Cleanup();                
            }

            btnPackagerUnitTests.Enabled = true;
        }

        private void btnPackageMerge_Click(object sender, EventArgs e)
        {
            string tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName); // It creates it, so delete it first.
            try
            {
                Packager packager = new Packager("NUnit", "Gibraltar.Agent.Test");
                //Packager packager = new Packager("Performance", "Sample App");
                DateTimeOffset startTime = DateTimeOffset.UtcNow;
                packager.SendToFile(SessionCriteria.CompletedSessions, false, tempFileName);

                FileInfo fileInfo = new FileInfo(tempFileName);
                DateTimeOffset entTime = DateTimeOffset.UtcNow;
                long fileSize = (fileInfo.Length + 1023) / 1024;
                TimeSpan duration = entTime - startTime;
                MessageBox.Show(this, string.Format("Package file size: {0:N0} kb\r\nCompleted in: {1:N2} s", fileSize, duration.TotalSeconds),
                                "Package file created");
                packager.Dispose();
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        private static void RunOneUnitTestWithTryCatch(MethodInvoker test)
        {
            try
            {
                test();
            }
            catch (Exception ex)
            {
                Log.RecordException(ex, "Gibraltar.Agent.Test.Unit tests", true);
            }
        }

        private void btnShowLiveViewer_Click(object sender, EventArgs e)
        {
            Log.ShowLiveViewer();
        }

        private void btnLiveViewerControl_Click(object sender, EventArgs e)
        {
            new SecondForm().Show();
        }

        private void btnLogXmlDetails_Click(object sender, EventArgs e)
        {
            const string detailsXml = "<xml>\r\n    <test value1=\"test\" value2=\"more test\" />\r\n"+
                                      "    <test value1=\"test again\" value2=\"and again\" />\r\n</xml>";

            Log.InformationDetail(detailsXml, "Gibraltar.AgentTest.XML", "Logging a message with XML details", null);
        }

        private void btnTraceVariations_Click(object sender, EventArgs e)
        {
            TraceListenerTests traceListenerTests = new TraceListenerTests();
            RunOneUnitTestWithTryCatch(traceListenerTests.WriteTrace);
        }

        private void btnDisplayConsent_Click(object sender, EventArgs e)
        {
            Log.DisplayConsentDialog();
        }

        private void btnStartupConsent_Click(object sender, EventArgs e)
        {
            Log.DisplayStartupConsentDialog();
        }

        private void btnCustomerTest_Click(object sender, EventArgs e)
        {
            ActionCustomerTest(true);
        }
    }
}
