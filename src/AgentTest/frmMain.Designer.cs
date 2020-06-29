namespace Gibraltar.Agent.Test
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.txtToEmail = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFromEmail = new System.Windows.Forms.TextBox();
            this.btnSendEmail = new System.Windows.Forms.Button();
            this.btnSendEmailAsync = new System.Windows.Forms.Button();
            this.btnSendEmailManual = new System.Windows.Forms.Button();
            this.btnSendEmailManualAsync = new System.Windows.Forms.Button();
            this.lblBeginSend = new System.Windows.Forms.Label();
            this.lblEndSend = new System.Windows.Forms.Label();
            this.btnAsyncPerf = new System.Windows.Forms.Button();
            this.btnAsyncWriteMessagePerf = new System.Windows.Forms.Button();
            this.btnAllPerfTests = new System.Windows.Forms.Button();
            this.perfGroupBox = new System.Windows.Forms.GroupBox();
            this.btnDatabaseReadTest = new System.Windows.Forms.Button();
            this.btnSampledReflection = new System.Windows.Forms.Button();
            this.btnSampledManual = new System.Windows.Forms.Button();
            this.btnEventReflection = new System.Windows.Forms.Button();
            this.btnEventManual = new System.Windows.Forms.Button();
            this.btnPackageWizard = new System.Windows.Forms.Button();
            this.btnDatabaseEventMetricExample = new System.Windows.Forms.Button();
            this.exceptionGroupBox = new System.Windows.Forms.GroupBox();
            this.btnCustomerTest = new System.Windows.Forms.Button();
            this.btnShowNotifier = new System.Windows.Forms.Button();
            this.btnGLVException = new System.Windows.Forms.Button();
            this.btnUnhandledUserInterface = new System.Windows.Forms.Button();
            this.btnUnhandledBackground = new System.Windows.Forms.Button();
            this.btnUnhandledForeground = new System.Windows.Forms.Button();
            this.btnNeverEndingLogging = new System.Windows.Forms.Button();
            this.btnStartSession = new System.Windows.Forms.Button();
            this.btnPackagerUnitTests = new System.Windows.Forms.Button();
            this.btnPackageMerge = new System.Windows.Forms.Button();
            this.btnShowLiveViewer = new System.Windows.Forms.Button();
            this.btnLiveViewerControl = new System.Windows.Forms.Button();
            this.btnLogXmlDetails = new System.Windows.Forms.Button();
            this.btnTraceVariations = new System.Windows.Forms.Button();
            this.btnDisplayConsent = new System.Windows.Forms.Button();
            this.btnStartupConsent = new System.Windows.Forms.Button();
            this.btnEndSession = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.perfGroupBox.SuspendLayout();
            this.exceptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtPassword);
            this.panel1.Controls.Add(this.txtUserName);
            this.panel1.Controls.Add(this.txtServer);
            this.panel1.Controls.Add(this.txtSubject);
            this.panel1.Controls.Add(this.txtToEmail);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtFromEmail);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(320, 153);
            this.panel1.TabIndex = 0;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(67, 130);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(252, 20);
            this.txtPassword.TabIndex = 11;
            this.txtPassword.Text = "jZixquck1";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(67, 104);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(252, 20);
            this.txtUserName.TabIndex = 10;
            this.txtUserName.Text = "mailrelay";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(67, 78);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(252, 20);
            this.txtServer.TabIndex = 9;
            this.txtServer.Text = "ajax.srellim.org";
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(67, 52);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(252, 20);
            this.txtSubject.TabIndex = 8;
            this.txtSubject.Text = "This is a test subject from process {0}";
            // 
            // txtToEmail
            // 
            this.txtToEmail.Location = new System.Drawing.Point(67, 26);
            this.txtToEmail.Name = "txtToEmail";
            this.txtToEmail.Size = new System.Drawing.Size(252, 20);
            this.txtToEmail.TabIndex = 7;
            this.txtToEmail.Text = "Rob.Parker@Gibraltar Software.com";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 107);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "User Name:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Server:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Subject:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "To Email:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "From Email:";
            // 
            // txtFromEmail
            // 
            this.txtFromEmail.Location = new System.Drawing.Point(67, 0);
            this.txtFromEmail.Name = "txtFromEmail";
            this.txtFromEmail.Size = new System.Drawing.Size(252, 20);
            this.txtFromEmail.TabIndex = 0;
            this.txtFromEmail.Text = "sysops@srellim.org";
            // 
            // btnSendEmail
            // 
            this.btnSendEmail.Location = new System.Drawing.Point(353, 10);
            this.btnSendEmail.Name = "btnSendEmail";
            this.btnSendEmail.Size = new System.Drawing.Size(128, 23);
            this.btnSendEmail.TabIndex = 1;
            this.btnSendEmail.Text = "Email";
            this.btnSendEmail.UseVisualStyleBackColor = true;
            this.btnSendEmail.Click += new System.EventHandler(this.btnSendEmail_Click);
            // 
            // btnSendEmailAsync
            // 
            this.btnSendEmailAsync.Location = new System.Drawing.Point(353, 88);
            this.btnSendEmailAsync.Name = "btnSendEmailAsync";
            this.btnSendEmailAsync.Size = new System.Drawing.Size(128, 23);
            this.btnSendEmailAsync.TabIndex = 2;
            this.btnSendEmailAsync.Text = "Email Async";
            this.btnSendEmailAsync.UseVisualStyleBackColor = true;
            this.btnSendEmailAsync.Click += new System.EventHandler(this.btnSendEmailAsync_Click);
            // 
            // btnSendEmailManual
            // 
            this.btnSendEmailManual.Location = new System.Drawing.Point(353, 36);
            this.btnSendEmailManual.Name = "btnSendEmailManual";
            this.btnSendEmailManual.Size = new System.Drawing.Size(128, 23);
            this.btnSendEmailManual.TabIndex = 3;
            this.btnSendEmailManual.Text = "Email Manual";
            this.btnSendEmailManual.UseVisualStyleBackColor = true;
            this.btnSendEmailManual.Click += new System.EventHandler(this.btnSendEmailManual_Click);
            // 
            // btnSendEmailManualAsync
            // 
            this.btnSendEmailManualAsync.Location = new System.Drawing.Point(353, 114);
            this.btnSendEmailManualAsync.Name = "btnSendEmailManualAsync";
            this.btnSendEmailManualAsync.Size = new System.Drawing.Size(128, 23);
            this.btnSendEmailManualAsync.TabIndex = 4;
            this.btnSendEmailManualAsync.Text = "Email Manual Async";
            this.btnSendEmailManualAsync.UseVisualStyleBackColor = true;
            this.btnSendEmailManualAsync.Click += new System.EventHandler(this.btnSendEmailManualAsync_Click);
            // 
            // lblBeginSend
            // 
            this.lblBeginSend.AutoSize = true;
            this.lblBeginSend.Enabled = false;
            this.lblBeginSend.Location = new System.Drawing.Point(357, 146);
            this.lblBeginSend.Name = "lblBeginSend";
            this.lblBeginSend.Size = new System.Drawing.Size(34, 13);
            this.lblBeginSend.TabIndex = 5;
            this.lblBeginSend.Text = "Begin";
            // 
            // lblEndSend
            // 
            this.lblEndSend.AutoSize = true;
            this.lblEndSend.Enabled = false;
            this.lblEndSend.Location = new System.Drawing.Point(455, 146);
            this.lblEndSend.Name = "lblEndSend";
            this.lblEndSend.Size = new System.Drawing.Size(26, 13);
            this.lblEndSend.TabIndex = 6;
            this.lblEndSend.Text = "End";
            this.lblEndSend.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnAsyncPerf
            // 
            this.btnAsyncPerf.Location = new System.Drawing.Point(6, 19);
            this.btnAsyncPerf.Name = "btnAsyncPerf";
            this.btnAsyncPerf.Size = new System.Drawing.Size(130, 25);
            this.btnAsyncPerf.TabIndex = 7;
            this.btnAsyncPerf.Text = "Write Perf";
            this.btnAsyncPerf.UseVisualStyleBackColor = true;
            this.btnAsyncPerf.Click += new System.EventHandler(this.btnAsyncPerf_Click);
            // 
            // btnAsyncWriteMessagePerf
            // 
            this.btnAsyncWriteMessagePerf.Location = new System.Drawing.Point(6, 50);
            this.btnAsyncWriteMessagePerf.Name = "btnAsyncWriteMessagePerf";
            this.btnAsyncWriteMessagePerf.Size = new System.Drawing.Size(130, 25);
            this.btnAsyncWriteMessagePerf.TabIndex = 8;
            this.btnAsyncWriteMessagePerf.Text = "Write Message Perf";
            this.btnAsyncWriteMessagePerf.UseVisualStyleBackColor = true;
            this.btnAsyncWriteMessagePerf.Click += new System.EventHandler(this.btnAsyncWriteMessagePerf_Click);
            // 
            // btnAllPerfTests
            // 
            this.btnAllPerfTests.Location = new System.Drawing.Point(6, 81);
            this.btnAllPerfTests.Name = "btnAllPerfTests";
            this.btnAllPerfTests.Size = new System.Drawing.Size(130, 25);
            this.btnAllPerfTests.TabIndex = 9;
            this.btnAllPerfTests.Text = "All Perf Tests";
            this.btnAllPerfTests.UseVisualStyleBackColor = true;
            this.btnAllPerfTests.Click += new System.EventHandler(this.btnAllPerfTests_Click);
            // 
            // perfGroupBox
            // 
            this.perfGroupBox.Controls.Add(this.btnDatabaseReadTest);
            this.perfGroupBox.Controls.Add(this.btnSampledReflection);
            this.perfGroupBox.Controls.Add(this.btnSampledManual);
            this.perfGroupBox.Controls.Add(this.btnEventReflection);
            this.perfGroupBox.Controls.Add(this.btnEventManual);
            this.perfGroupBox.Controls.Add(this.btnAsyncPerf);
            this.perfGroupBox.Controls.Add(this.btnAllPerfTests);
            this.perfGroupBox.Controls.Add(this.btnAsyncWriteMessagePerf);
            this.perfGroupBox.Location = new System.Drawing.Point(12, 180);
            this.perfGroupBox.Name = "perfGroupBox";
            this.perfGroupBox.Size = new System.Drawing.Size(320, 145);
            this.perfGroupBox.TabIndex = 10;
            this.perfGroupBox.TabStop = false;
            this.perfGroupBox.Text = "Performance Testing";
            // 
            // btnDatabaseReadTest
            // 
            this.btnDatabaseReadTest.Location = new System.Drawing.Point(6, 114);
            this.btnDatabaseReadTest.Name = "btnDatabaseReadTest";
            this.btnDatabaseReadTest.Size = new System.Drawing.Size(130, 25);
            this.btnDatabaseReadTest.TabIndex = 15;
            this.btnDatabaseReadTest.Text = "DB Read Variations";
            this.btnDatabaseReadTest.UseVisualStyleBackColor = true;
            this.btnDatabaseReadTest.Click += new System.EventHandler(this.btnDatabaseReadTest_Click);
            // 
            // btnSampledReflection
            // 
            this.btnSampledReflection.Location = new System.Drawing.Point(193, 112);
            this.btnSampledReflection.Name = "btnSampledReflection";
            this.btnSampledReflection.Size = new System.Drawing.Size(112, 25);
            this.btnSampledReflection.TabIndex = 13;
            this.btnSampledReflection.Text = "Sampled Reflection";
            this.btnSampledReflection.UseVisualStyleBackColor = true;
            this.btnSampledReflection.Click += new System.EventHandler(this.btnSampledReflection_Click);
            // 
            // btnSampledManual
            // 
            this.btnSampledManual.Location = new System.Drawing.Point(193, 81);
            this.btnSampledManual.Name = "btnSampledManual";
            this.btnSampledManual.Size = new System.Drawing.Size(112, 25);
            this.btnSampledManual.TabIndex = 12;
            this.btnSampledManual.Text = "Sampled Manual";
            this.btnSampledManual.UseVisualStyleBackColor = true;
            this.btnSampledManual.Click += new System.EventHandler(this.btnSampledManual_Click);
            // 
            // btnEventReflection
            // 
            this.btnEventReflection.Location = new System.Drawing.Point(193, 50);
            this.btnEventReflection.Name = "btnEventReflection";
            this.btnEventReflection.Size = new System.Drawing.Size(112, 25);
            this.btnEventReflection.TabIndex = 11;
            this.btnEventReflection.Text = "Event Reflection";
            this.btnEventReflection.UseVisualStyleBackColor = true;
            this.btnEventReflection.Click += new System.EventHandler(this.btnEventReflection_Click);
            // 
            // btnEventManual
            // 
            this.btnEventManual.Location = new System.Drawing.Point(193, 19);
            this.btnEventManual.Name = "btnEventManual";
            this.btnEventManual.Size = new System.Drawing.Size(112, 25);
            this.btnEventManual.TabIndex = 10;
            this.btnEventManual.Text = "Event Manual";
            this.btnEventManual.UseVisualStyleBackColor = true;
            this.btnEventManual.Click += new System.EventHandler(this.btnEventManual_Click);
            // 
            // btnPackageWizard
            // 
            this.btnPackageWizard.Location = new System.Drawing.Point(353, 199);
            this.btnPackageWizard.Name = "btnPackageWizard";
            this.btnPackageWizard.Size = new System.Drawing.Size(128, 25);
            this.btnPackageWizard.TabIndex = 11;
            this.btnPackageWizard.Text = "Packager Wizard";
            this.btnPackageWizard.UseVisualStyleBackColor = true;
            this.btnPackageWizard.Click += new System.EventHandler(this.btnPackageWizard_Click);
            // 
            // btnDatabaseEventMetricExample
            // 
            this.btnDatabaseEventMetricExample.Location = new System.Drawing.Point(353, 292);
            this.btnDatabaseEventMetricExample.Name = "btnDatabaseEventMetricExample";
            this.btnDatabaseEventMetricExample.Size = new System.Drawing.Size(128, 25);
            this.btnDatabaseEventMetricExample.TabIndex = 12;
            this.btnDatabaseEventMetricExample.Text = "DB Event Example";
            this.btnDatabaseEventMetricExample.UseVisualStyleBackColor = true;
            this.btnDatabaseEventMetricExample.Click += new System.EventHandler(this.btnDatabaseEventMetricExample_Click);
            // 
            // exceptionGroupBox
            // 
            this.exceptionGroupBox.Controls.Add(this.btnCustomerTest);
            this.exceptionGroupBox.Controls.Add(this.btnShowNotifier);
            this.exceptionGroupBox.Controls.Add(this.btnGLVException);
            this.exceptionGroupBox.Controls.Add(this.btnUnhandledUserInterface);
            this.exceptionGroupBox.Controls.Add(this.btnUnhandledBackground);
            this.exceptionGroupBox.Controls.Add(this.btnUnhandledForeground);
            this.exceptionGroupBox.Location = new System.Drawing.Point(8, 338);
            this.exceptionGroupBox.Name = "exceptionGroupBox";
            this.exceptionGroupBox.Size = new System.Drawing.Size(324, 146);
            this.exceptionGroupBox.TabIndex = 13;
            this.exceptionGroupBox.TabStop = false;
            this.exceptionGroupBox.Text = "Exceptions";
            // 
            // btnCustomerTest
            // 
            this.btnCustomerTest.Location = new System.Drawing.Point(10, 112);
            this.btnCustomerTest.Name = "btnCustomerTest";
            this.btnCustomerTest.Size = new System.Drawing.Size(130, 25);
            this.btnCustomerTest.TabIndex = 24;
            this.btnCustomerTest.Text = "Customer Test";
            this.btnCustomerTest.UseVisualStyleBackColor = true;
            this.btnCustomerTest.Click += new System.EventHandler(this.btnCustomerTest_Click);
            // 
            // btnShowNotifier
            // 
            this.btnShowNotifier.Location = new System.Drawing.Point(10, 19);
            this.btnShowNotifier.Name = "btnShowNotifier";
            this.btnShowNotifier.Size = new System.Drawing.Size(130, 25);
            this.btnShowNotifier.TabIndex = 16;
            this.btnShowNotifier.Text = "Show Notify";
            this.btnShowNotifier.UseVisualStyleBackColor = true;
            this.btnShowNotifier.Click += new System.EventHandler(this.btnShowNotifier_Click);
            // 
            // btnGLVException
            // 
            this.btnGLVException.Location = new System.Drawing.Point(197, 112);
            this.btnGLVException.Name = "btnGLVException";
            this.btnGLVException.Size = new System.Drawing.Size(112, 25);
            this.btnGLVException.TabIndex = 15;
            this.btnGLVException.Text = "GLV";
            this.btnGLVException.UseVisualStyleBackColor = true;
            this.btnGLVException.Click += new System.EventHandler(this.btnGLVException_Click);
            // 
            // btnUnhandledUserInterface
            // 
            this.btnUnhandledUserInterface.Location = new System.Drawing.Point(197, 19);
            this.btnUnhandledUserInterface.Name = "btnUnhandledUserInterface";
            this.btnUnhandledUserInterface.Size = new System.Drawing.Size(112, 25);
            this.btnUnhandledUserInterface.TabIndex = 14;
            this.btnUnhandledUserInterface.Text = "User Interface";
            this.btnUnhandledUserInterface.UseVisualStyleBackColor = true;
            this.btnUnhandledUserInterface.Click += new System.EventHandler(this.btnUnhandledUserInterface_Click);
            // 
            // btnUnhandledBackground
            // 
            this.btnUnhandledBackground.Location = new System.Drawing.Point(197, 81);
            this.btnUnhandledBackground.Name = "btnUnhandledBackground";
            this.btnUnhandledBackground.Size = new System.Drawing.Size(112, 25);
            this.btnUnhandledBackground.TabIndex = 12;
            this.btnUnhandledBackground.Text = "Background";
            this.btnUnhandledBackground.UseVisualStyleBackColor = true;
            this.btnUnhandledBackground.Click += new System.EventHandler(this.btnUnhandledBackground_Click);
            // 
            // btnUnhandledForeground
            // 
            this.btnUnhandledForeground.Location = new System.Drawing.Point(197, 50);
            this.btnUnhandledForeground.Name = "btnUnhandledForeground";
            this.btnUnhandledForeground.Size = new System.Drawing.Size(112, 25);
            this.btnUnhandledForeground.TabIndex = 11;
            this.btnUnhandledForeground.Text = "Foreground";
            this.btnUnhandledForeground.UseVisualStyleBackColor = true;
            this.btnUnhandledForeground.Click += new System.EventHandler(this.btnUnhandledForeground_Click);
            // 
            // btnNeverEndingLogging
            // 
            this.btnNeverEndingLogging.Location = new System.Drawing.Point(351, 323);
            this.btnNeverEndingLogging.Name = "btnNeverEndingLogging";
            this.btnNeverEndingLogging.Size = new System.Drawing.Size(130, 25);
            this.btnNeverEndingLogging.TabIndex = 14;
            this.btnNeverEndingLogging.Text = "Log Forever";
            this.btnNeverEndingLogging.UseVisualStyleBackColor = true;
            this.btnNeverEndingLogging.Click += new System.EventHandler(this.btnNeverEndingLogging_Click);
            // 
            // btnStartSession
            // 
            this.btnStartSession.Location = new System.Drawing.Point(351, 357);
            this.btnStartSession.Name = "btnStartSession";
            this.btnStartSession.Size = new System.Drawing.Size(130, 25);
            this.btnStartSession.TabIndex = 15;
            this.btnStartSession.Text = "Start Session";
            this.btnStartSession.UseVisualStyleBackColor = true;
            this.btnStartSession.Click += new System.EventHandler(this.btnStartSession_Click);
            // 
            // btnPackagerUnitTests
            // 
            this.btnPackagerUnitTests.Location = new System.Drawing.Point(353, 230);
            this.btnPackagerUnitTests.Name = "btnPackagerUnitTests";
            this.btnPackagerUnitTests.Size = new System.Drawing.Size(128, 25);
            this.btnPackagerUnitTests.TabIndex = 16;
            this.btnPackagerUnitTests.Text = "Packager Unit Tests";
            this.btnPackagerUnitTests.UseVisualStyleBackColor = true;
            this.btnPackagerUnitTests.Click += new System.EventHandler(this.btnPackagerUnitTests_Click);
            // 
            // btnPackageMerge
            // 
            this.btnPackageMerge.Location = new System.Drawing.Point(353, 261);
            this.btnPackageMerge.Name = "btnPackageMerge";
            this.btnPackageMerge.Size = new System.Drawing.Size(128, 25);
            this.btnPackageMerge.TabIndex = 17;
            this.btnPackageMerge.Text = "Packager Merge";
            this.btnPackageMerge.UseVisualStyleBackColor = true;
            this.btnPackageMerge.Click += new System.EventHandler(this.btnPackageMerge_Click);
            // 
            // btnShowLiveViewer
            // 
            this.btnShowLiveViewer.Location = new System.Drawing.Point(353, 450);
            this.btnShowLiveViewer.Name = "btnShowLiveViewer";
            this.btnShowLiveViewer.Size = new System.Drawing.Size(128, 25);
            this.btnShowLiveViewer.TabIndex = 18;
            this.btnShowLiveViewer.Text = "Show Live Viewer";
            this.btnShowLiveViewer.UseVisualStyleBackColor = true;
            this.btnShowLiveViewer.Click += new System.EventHandler(this.btnShowLiveViewer_Click);
            // 
            // btnLiveViewerControl
            // 
            this.btnLiveViewerControl.Location = new System.Drawing.Point(353, 419);
            this.btnLiveViewerControl.Name = "btnLiveViewerControl";
            this.btnLiveViewerControl.Size = new System.Drawing.Size(128, 25);
            this.btnLiveViewerControl.TabIndex = 19;
            this.btnLiveViewerControl.Text = "Live Viewer Control";
            this.btnLiveViewerControl.UseVisualStyleBackColor = true;
            this.btnLiveViewerControl.Click += new System.EventHandler(this.btnLiveViewerControl_Click);
            // 
            // btnLogXmlDetails
            // 
            this.btnLogXmlDetails.Location = new System.Drawing.Point(353, 481);
            this.btnLogXmlDetails.Name = "btnLogXmlDetails";
            this.btnLogXmlDetails.Size = new System.Drawing.Size(128, 25);
            this.btnLogXmlDetails.TabIndex = 20;
            this.btnLogXmlDetails.Text = "Log XML Details";
            this.btnLogXmlDetails.UseVisualStyleBackColor = true;
            this.btnLogXmlDetails.Click += new System.EventHandler(this.btnLogXmlDetails_Click);
            // 
            // btnTraceVariations
            // 
            this.btnTraceVariations.Location = new System.Drawing.Point(353, 512);
            this.btnTraceVariations.Name = "btnTraceVariations";
            this.btnTraceVariations.Size = new System.Drawing.Size(128, 25);
            this.btnTraceVariations.TabIndex = 21;
            this.btnTraceVariations.Text = "Trace Log Variations";
            this.btnTraceVariations.UseVisualStyleBackColor = true;
            this.btnTraceVariations.Click += new System.EventHandler(this.btnTraceVariations_Click);
            // 
            // btnDisplayConsent
            // 
            this.btnDisplayConsent.Location = new System.Drawing.Point(205, 512);
            this.btnDisplayConsent.Name = "btnDisplayConsent";
            this.btnDisplayConsent.Size = new System.Drawing.Size(112, 25);
            this.btnDisplayConsent.TabIndex = 22;
            this.btnDisplayConsent.Text = "Display Consent";
            this.btnDisplayConsent.UseVisualStyleBackColor = true;
            this.btnDisplayConsent.Click += new System.EventHandler(this.btnDisplayConsent_Click);
            // 
            // btnStartupConsent
            // 
            this.btnStartupConsent.Location = new System.Drawing.Point(18, 512);
            this.btnStartupConsent.Name = "btnStartupConsent";
            this.btnStartupConsent.Size = new System.Drawing.Size(112, 25);
            this.btnStartupConsent.TabIndex = 23;
            this.btnStartupConsent.Text = "Startup Consent";
            this.btnStartupConsent.UseVisualStyleBackColor = true;
            this.btnStartupConsent.Click += new System.EventHandler(this.btnStartupConsent_Click);
            // 
            // btnEndSession
            // 
            this.btnEndSession.Location = new System.Drawing.Point(351, 388);
            this.btnEndSession.Name = "btnEndSession";
            this.btnEndSession.Size = new System.Drawing.Size(130, 25);
            this.btnEndSession.TabIndex = 24;
            this.btnEndSession.Text = "End Session";
            this.btnEndSession.UseVisualStyleBackColor = true;
            this.btnEndSession.Click += new System.EventHandler(this.btnEndSession_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 548);
            this.Controls.Add(this.btnEndSession);
            this.Controls.Add(this.btnStartupConsent);
            this.Controls.Add(this.btnDisplayConsent);
            this.Controls.Add(this.btnTraceVariations);
            this.Controls.Add(this.btnLogXmlDetails);
            this.Controls.Add(this.btnLiveViewerControl);
            this.Controls.Add(this.btnShowLiveViewer);
            this.Controls.Add(this.btnPackageMerge);
            this.Controls.Add(this.btnPackagerUnitTests);
            this.Controls.Add(this.btnStartSession);
            this.Controls.Add(this.btnNeverEndingLogging);
            this.Controls.Add(this.exceptionGroupBox);
            this.Controls.Add(this.btnDatabaseEventMetricExample);
            this.Controls.Add(this.btnPackageWizard);
            this.Controls.Add(this.perfGroupBox);
            this.Controls.Add(this.lblEndSend);
            this.Controls.Add(this.lblBeginSend);
            this.Controls.Add(this.btnSendEmailManualAsync);
            this.Controls.Add(this.btnSendEmailManual);
            this.Controls.Add(this.btnSendEmailAsync);
            this.Controls.Add(this.btnSendEmail);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "Agent Test Tool";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.perfGroupBox.ResumeLayout(false);
            this.exceptionGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtFromEmail;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.TextBox txtToEmail;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSendEmail;
        private System.Windows.Forms.Button btnSendEmailAsync;
        private System.Windows.Forms.Button btnSendEmailManual;
        private System.Windows.Forms.Button btnSendEmailManualAsync;
        private System.Windows.Forms.Label lblBeginSend;
        private System.Windows.Forms.Label lblEndSend;
        private System.Windows.Forms.Button btnAsyncPerf;
        private System.Windows.Forms.Button btnAsyncWriteMessagePerf;
        private System.Windows.Forms.Button btnAllPerfTests;
        private System.Windows.Forms.GroupBox perfGroupBox;
        private System.Windows.Forms.Button btnPackageWizard;
        private System.Windows.Forms.Button btnEventManual;
        private System.Windows.Forms.Button btnEventReflection;
        private System.Windows.Forms.Button btnSampledReflection;
        private System.Windows.Forms.Button btnSampledManual;
        private System.Windows.Forms.Button btnDatabaseEventMetricExample;
        private System.Windows.Forms.GroupBox exceptionGroupBox;
        private System.Windows.Forms.Button btnUnhandledBackground;
        private System.Windows.Forms.Button btnUnhandledForeground;
        private System.Windows.Forms.Button btnUnhandledUserInterface;
        private System.Windows.Forms.Button btnGLVException;
        private System.Windows.Forms.Button btnDatabaseReadTest;
        private System.Windows.Forms.Button btnShowNotifier;
        private System.Windows.Forms.Button btnNeverEndingLogging;
        private System.Windows.Forms.Button btnStartSession;
        private System.Windows.Forms.Button btnPackagerUnitTests;
        private System.Windows.Forms.Button btnPackageMerge;
        private System.Windows.Forms.Button btnShowLiveViewer;
        private System.Windows.Forms.Button btnLiveViewerControl;
        private System.Windows.Forms.Button btnLogXmlDetails;
        private System.Windows.Forms.Button btnTraceVariations;
        private System.Windows.Forms.Button btnDisplayConsent;
        private System.Windows.Forms.Button btnStartupConsent;
        private System.Windows.Forms.Button btnCustomerTest;
        private System.Windows.Forms.Button btnEndSession;
    }
}