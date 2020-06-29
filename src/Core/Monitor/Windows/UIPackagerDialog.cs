#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Configuration;
using System.Windows.Forms;
using Gibraltar.Monitor.Windows.Internal;
using Gibraltar.Windows.UI;

#endregion File Header

namespace Gibraltar.Monitor.Windows
{
    /// <summary>
    /// A dialog version of the packager wizard
    /// </summary>
    /// <remarks>Applications can directly use this to package and send sessions.  This is the only way to 
    /// package information about a live session while it is still running.</remarks>
    public partial class UIPackagerDialog : UIWizardDialog
    {
        private readonly PackagerRequest m_Request;
        private readonly PackagerConfiguration m_Configuration;
        private bool m_DialogHasBeenUsed;

        private static readonly object s_PendingLock = new object();
        private static UIPackagerDialog s_PendingDialog; // LOCKED BY PENDINGLOCK

        private readonly object m_ResultLock = new object();
        private DialogResult m_Result = DialogResult.None; // LOCKED BY RESULTLOCK

        private event EventHandler<DialogResultEventArgs> PackagerResult;

        /// <summary>
        /// Create a new packager wizard dialog for the current application.
        /// </summary>
        public UIPackagerDialog() 
            : this(false, null, null)
        {
            
        }

        /// <summary>
        /// Create a new packager wizard dialog for the specified application.
        /// </summary>
        /// <param name="productName">The exact product name to restrict the package to.</param>
        /// <param name="applicationName">Optional.  The exact application within the product to restrict the package to.</param>
        /// <remarks>Specify just the product name to package up all data for any application in that product. Specify
        /// both product and application name to restrict the package to a specific application.  If not specified, 
        /// the values from the app.config file will be used (which default to null)</remarks>
        public UIPackagerDialog(string productName, string applicationName)
            : this(true, productName, applicationName)
        {

        }

        /// <summary>
        /// Create a new packager wizard dialog for the specified application.
        /// </summary>
        /// <param name="productName">The exact product name to restrict the package to.</param>
        /// <param name="applicationName">Optional.  The exact application within the product to restrict the package to.</param>
        /// <param name="useProductName">Indicates if the product name and application name should be specified and valid or not.</param>
        /// <remarks>Specify just the product name to package up all data for any application in that product. Specify
        /// both product and application name to restrict the package to a specific application.  If not specified, 
        /// the values from the app.config file will be used (which default to null)</remarks>
        private UIPackagerDialog(bool useProductName, string productName, string applicationName)
            : base("Health and Usage Reporting Wizard")
        {
            InitializeComponent();

            EnsureTopMost = true; // Make us stick to the front.

            m_Configuration = Log.Configuration.Packager;

            if (useProductName && (string.IsNullOrEmpty(productName)))
            {
                throw new ArgumentNullException(nameof(productName), "When specifying a specific product and application, the product name must be specified.");
            }

            //Different constructors depending on whether we're overriding the product/app name
            m_Request = useProductName ? new PackagerRequest(productName, applicationName) : new PackagerRequest();

        }

        #region Public Properties and Methods

        /// <summary>
        /// Raise the pending packager dialog to the front, or report that there is none.
        /// </summary>
        /// <returns>True if one is pending, false if not.</returns>
        public static bool RaisePendingToFront()
        {
            bool pending;
            lock (s_PendingLock)
            {
                if (s_PendingDialog != null)
                {
                    pending = true;
                    s_PendingDialog.BeginInvoke(new MethodInvoker(s_PendingDialog.RestoreToFront));
                }
                else
                {
                    pending = false;
                }
            }

            return pending;
        }


        /// <summary>
        /// Specify email server information to use if required instead of the information stored in the application configuration.
        /// </summary>
        /// <param name="emailServer">The full dns name of the SMTP server to use to relay the mail message.</param>
        /// <param name="serverPort">Optional.  The TCP/IP Port number to connect to the server on.  Use 0 for the default configured port.</param>
        /// <param name="useSsl">True to encrypt the server communication with Secure Sockets Layer.</param>
        /// <param name="emailServerUser">Optional.  A username to use instead of the current windows user credentials to communicate with the email server.</param>
        /// <param name="emailServerPassword">Optional.  A password to use for the user name for credentials to communicate with the email server.</param>
        /// <remarks>Use this option if you want to have the packager user a specific email server instead of the information stored
        /// in the application configuration file.  This is particularly useful if you need to authenticate to your mail relay and
        /// do not want to have the credentials in your configuration file.</remarks>
        public void SetEmailServer(string emailServer, int serverPort, bool useSsl, string emailServerUser, string emailServerPassword)
        {
            //this goes into the request so it's available to the final packaging step.
            m_Request.EmailServer = emailServer;
            m_Request.ServerPort = serverPort;
            m_Request.UseSsl = useSsl;
            m_Request.EmailServerUser = emailServerUser;
            m_Request.EmailServerPassword = emailServerPassword;
        }

        /// <summary>
        /// Perform the package &amp; send process using the application configuration
        /// </summary>
        /// <remarks>A given dialog instance can only be used one time.  Reuse will cause errors.</remarks>
        public DialogResult Send()
        {
            //the allow flags don't have any way of defaulting back to configuration so we have to pass them in explicitly
            return Send(m_Configuration.AllowFile, m_Configuration.AllowRemovableMedia, m_Configuration.AllowEmail, m_Configuration.AllowServer,
                        null, null, null);
        }

        /// <summary>
        /// Perform the package &amp; send process using the specified options.
        /// </summary>
        /// <remarks>A given dialog instance can only be used one time.  Reuse will cause errors.</remarks>
        public DialogResult Send(bool allowFile, bool allowRemovableMedia, bool allowEmail, bool allowServer, string destinationAddress, string fromAddress, string fileNamePath)
        {
            if (m_DialogHasBeenUsed)
                throw new InvalidOperationException("The packager dialog has already been used once to send a package and can't be reused.");

            m_DialogHasBeenUsed = true;

            //we can pre-populate our request from the configuration...
            if (string.IsNullOrEmpty(destinationAddress) == false)
            {
                m_Request.DestinationEmailAddress = destinationAddress;
            }
            else if (string.IsNullOrEmpty(m_Configuration.DestinationEmailAddress) == false)
            {
                m_Request.DestinationEmailAddress = m_Configuration.DestinationEmailAddress;
            }

            if (string.IsNullOrEmpty(fromAddress) == false)
            {
                m_Request.FromEmailAddress = fromAddress;
            }
            else if (string.IsNullOrEmpty(m_Configuration.FromEmailAddress) == false)
            {
                m_Request.FromEmailAddress = m_Configuration.FromEmailAddress;
            }

            if (string.IsNullOrEmpty(fileNamePath) == false)
            {
                m_Request.FileNamePath = fileNamePath;
            }

            //now add our new pages
            Initialize(m_Request);
            AddPage(new UIPackagerCriteria(m_Configuration));

            bool transportInputRequired = false;

            //and here's a subtle side effect:  The order we check these creates our default. (The last one is the default)
            if (allowFile)
            {
                m_Request.AllowFile = true;
                m_Request.Transport = PackageTransport.File;
                transportInputRequired = true;
            }

            if (allowRemovableMedia)
            {
                m_Request.AllowRemovableMedia = true;
                m_Request.Transport = PackageTransport.RemovableMedia;
                transportInputRequired = true;
            }

            if (allowEmail)
            {
                m_Request.AllowEmail = true;
                m_Request.Transport = PackageTransport.Email;
                transportInputRequired = true;
            }

            if (allowServer)
            {
                m_Request.AllowServer = true;
                m_Request.Transport = PackageTransport.Server;
                //this is not a choice - you get this if it's available, the others are available as a choice.
            }

            //check for the "we're screwed" case.
            if ((transportInputRequired == false) && (allowServer == false))
            {
                throw new ConfigurationErrorsException("The current configuration disallows all packaging modes, effectively disabling the packager.");
            }

            //Only put in the transport option if they need to provide input to the transport.
            if (transportInputRequired)
            {
                AddPage(new UIPackagerTransport(m_Configuration, m_Request));
            }

            //configure the shared stuff for the wizard
            SetSequencer(new PackagerSequencer(m_Configuration));
            SetFinishedPage(new UIPackagerFinish(m_Configuration));

            //now that we've configured everything we can actually show the dialog...
            return JoinShowDialog();
        }

        #endregion

        #region Internal Properties and Methods

        /// <summary>
        /// Restore the form to the front of the Z order.
        /// </summary>
        /// <remarks>This must be called from the UI thread for this form.  The form will be restored from a Minimized
        /// state to Normal (but Maximized left unchanged), pushed to the front, and ensured to be Visible.</remarks>
        internal void RestoreToFront()
        {
            // For a minimized window, restore it.
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            // This little bit with TopMost ensures that the window, if already open,
            // but beneath other windows, will be raised to the front.
            TopMost = true; // Raise us to the front...
            Visible = true; // Make sure we're visible.
            TopMost = EnsureTopMost; // ...Should we stick to the front?
        }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// Show the first Dialog, or block and wait on the result of one already pending. (Only called by Send(...))
        /// </summary>
        /// <returns>The DialogResult of the first interactive packager wizard to call this method simultaneously.</returns>
        private DialogResult JoinShowDialog()
        {
            DialogResult result;
            UIPackagerDialog pending;
            lock (s_PendingLock)
            {
                if (s_PendingDialog == null)
                {
                    s_PendingDialog = this; // We're the first!
                    pending = null;
                }
                else
                {
                    m_Result = DialogResult.None; // Make sure our result is clear.
                    pending = s_PendingDialog;
                    s_PendingDialog.PackagerResult += UIPackagerDialog_PackagerResult; // Subscribe to the result event.

                    // Try to bring the one we're waiting on to the front, since that would otherwise be us!
                    pending.BeginInvoke(new MethodInvoker(pending.RestoreToFront));
                }

                System.Threading.Monitor.PulseAll(s_PendingLock);
            }

            if (pending != null) // Are we waiting on another one?
            {
                lock (m_ResultLock)
                {
                    while (m_Result == DialogResult.None)
                    {
                        System.Threading.Monitor.Wait(m_ResultLock);
                    }

                    result = m_Result; // Cache it while in the lock.
                }
            }
            else
            {
                result = ShowDialog(); // This blocks until the dialog form closes.

                EventHandler<DialogResultEventArgs> packagerResult;
                lock (s_PendingLock)
                {
                    packagerResult = PackagerResult; // Snapshot our subscribers before we clear the pending.
                    s_PendingDialog = null; // Clear the pending.  Any new caller can proceed without us.
                    System.Threading.Monitor.PulseAll(s_PendingLock);
                }

                if (packagerResult != null)
                    packagerResult(this, new DialogResultEventArgs(result)); // Send event to any waiting on us.
            }

            return result;
        }

        private void UIPackagerDialog_PackagerResult(object sender, DialogResultEventArgs e)
        {
            lock (m_ResultLock)
            {
                m_Result = e.Result;
                if (m_Result == DialogResult.None) // Safety check to avoid waiting forever!
                    m_Result = DialogResult.Cancel; // If something was wrong with the result event, call it cancelled.

                System.Threading.Monitor.PulseAll(m_ResultLock);
            }
        }

        #endregion

        #region Private subclass DialogResultEventArgs

        private class DialogResultEventArgs : EventArgs
        {
            private readonly DialogResult m_DialogResult;

            public DialogResultEventArgs(DialogResult result)
            {
                m_DialogResult = result;
            }

            public DialogResult Result { get { return m_DialogResult; } }
        }

        #endregion
    }
}
