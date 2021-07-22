
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
using System.Windows.Forms;
using Gibraltar.Monitor.Windows;

#endregion File Header

namespace Gibraltar.Agent
{
    /// <summary>
    /// A wizard dialog version of the packager
    /// </summary>
    /// <remarks><para>The user will be presented with a wizard user interface
    /// to complete the package and transmission process based on the package configuration
    /// in the application configuration file.  The length of the wizard will depend on the 
    /// application configuration settings and the specific method called on this class to
    /// start the process.</para>
    /// 	<para>There are two constructors for the dialog. The default constructor will package
    /// sessions for the current application based on its current product and application name.
    /// To send packages for a different application or for multiple applications in the same product
    /// use the constructor that accepts a product and (optional) application name.</para>
    /// 	<para>Product and Application names are determined automatically by inspecting the .NET
    /// assembly that was used to start the current process. If you want to override the product or 
    /// application name you can do so in your application configuration file.</para>
    /// 	<para>Overriding product or application is typically necessary either because the values compiled 
    /// into the assembly aren't optimal for this purpose or because the automatic values are incorrect due to
    /// the application being hosted within another process (like an ASP.NET application).</para></remarks>
    /// <example>
    /// 	<code lang="CS" description="The following example uses the PackagerDialog to display a wizard to the user to send data for the current process and logs to Trace if it doesn't complete successfully. The Packager automatically logs information to the Agent whenever there is a problem, so it isn't necessary to do additional logging.">
    /// 		<![CDATA[
    /// //create a new packager dialog for the user and start the process to send data
    /// //for the current application.
    /// PackagerDialog packagerDialog = new PackagerDialog();
    /// DialogResult result = packagerDialog.Send();
    /// if (result != DialogResult.OK)
    /// {
    ///     //The user may have canceled (DialogResult.Cancel) or 
    ///     //there may have been an error (DialogResult.Abort)
    ///     if (result == System.Windows.Forms.DialogResult.Abort)
    ///     {
    ///         Trace.TraceWarning("Package and send process generated an error for the user.");
    ///     }
    ///     else
    ///     {
    ///         Trace.TraceInformation("Package not sent.  Reason: " + result);
    ///     }
    /// }]]>
    /// 	</code>
    /// </example>
    public sealed class PackagerDialog
    {
        private readonly bool m_UseProductFilter;
        private readonly string m_ProductName;
        private readonly string m_ApplicationName;
        private string m_EmailServer;
        private int m_ServerPort;
        private bool m_UseSsl;
        private string m_EmailServerUser;
        private string m_EmailServerPassword;

        /// <summary>Create a new packager wizard dialog for the current application's product.</summary>
        /// <remarks>
        /// Using this constructor all of the applications related to the current
        /// application's product will be included in the package. For example, if you create
        /// several modules within a single product family you should give each module its own
        /// application name but share a product name. This will provide the best grouping for
        /// reporting &amp; analysis, and the Packager will capture all of the information for
        /// applications in the same product family in a package.
        /// </remarks>
        public PackagerDialog() 
        {
        }

        /// <summary>
        /// Create a new packager wizard dialog for the specified application.
        /// </summary>
        /// <remarks>
        /// 	<para>Specify just the product name to package up all data for any application in
        ///     that product. Specify both product and application name to restrict the package to
        ///     a specific application. If not specified, the values from the app.config file will
        ///     be used (which default to null).</para>
        /// 	<para>To send information for the product related to the current application, use
        ///     the default constructor.</para>
        /// </remarks>
        /// <param name="productName">The exact product name to restrict the package to.</param>
        /// <param name="applicationName">Optional.  The exact application within the product to restrict the package to.</param>
        public PackagerDialog(string productName, string applicationName)
        {
            m_UseProductFilter = true;
            m_ProductName = productName;
            m_ApplicationName = applicationName;
        }

        /// <summary>
        /// Start the package and send process, using the application configuration.
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para></remarks>
        /// <example>
        /// 	<code lang="CS" description="The following example uses the PackagerDialog to display a wizard to the user to send data for the current process and logs to Trace if it doesn't complete successfully. The Packager automatically logs information to the Agent whenever there is a problem, so it isn't necessary to do additional logging.">
        /// 		<![CDATA[
        /// //create a new packager dialog for the user and start the process to send data
        /// //for the current application.
        /// PackagerDialog packagerDialog = new PackagerDialog();
        /// DialogResult result = packagerDialog.Send();
        /// if (result != DialogResult.OK)
        /// {
        ///     //The user may have canceled (DialogResult.Cancel) or 
        ///     //there may have been an error (DialogResult.Abort)
        ///     if (result == System.Windows.Forms.DialogResult.Abort)
        ///     {
        ///         Trace.TraceWarning("Package and send process generated an error for the user.");
        ///     }
        ///     else
        ///     {
        ///         Trace.TraceInformation("Package not sent.  Reason: " + result);
        ///     }
        /// }]]>
        /// 	</code>
        /// </example>
        public DialogResult Send()
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send();
            }
        }

        /// <summary>
        /// Start the package and send process but only allow sending to email
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para>The wizard will not prompt the user to select how to send the package
        /// and will ignore the application configuration restrictions on allowed transports.</remarks>
        public DialogResult SendEmail()
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send(false, false, true, false, null, null, null);
            }
        }

        /// <summary>
        /// Start the package and send process but only allow sending to email
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para>The wizard will not prompt the user to select how to send the package
        /// and will ignore the application configuration restrictions on allowed transports.
        /// If a destination and/or source email address are specified they will override
        /// the application configuration.</remarks>
        public DialogResult SendEmail(string destinationAddress, string fromAddress)
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send(false, false, true, false, destinationAddress, fromAddress, null);
            }
        }

        /// <summary>
        /// Start the package and send process but only allow sending to a file.
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para>The wizard will not prompt the user to select how to send the package
        /// and will ignore the application configuration restrictions on allowed transports.
        /// The user will be prompted for a file name &amp; path to save the package as.</remarks>
        public DialogResult SendToFile()
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send(true, false, false, false, null, null, null);
            }
        }


        /// <summary>
        /// Start the package and send process but only allow sending to a file.
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <param name="fileNamePath">The fully qualified file name and path to store the final package as.</param>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para>The wizard will not prompt the user to select how to send the package
        /// and will ignore the application configuration restrictions on allowed transports.</remarks>
        public DialogResult SendToFile(string fileNamePath)
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send(true, false, false, false, null, null, fileNamePath);
            }
        }

        /// <summary>
        /// Start the package and send process but only allow sending to a server.
        /// </summary>
        /// <returns>A dialog result indicating if the packager completed successfully, failed, or was canceled.</returns>
        /// <remarks><para>The dialog result will be OK if the process completed successfully, Cancel if the
        /// user cancels the wizard before it attempts to send the package, and Abort if the package and send
        /// process encounters an error.</para>The wizard will not prompt the user to select how to send the package
        /// and will ignore the application configuration restrictions on allowed transports.</remarks>
        public DialogResult SendToServer()
        {
            using (UIPackagerDialog packagerDialog = GetPackagerDialog())
            {
                return packagerDialog.Send(false, false, false, true, null, null, null);
            }
        }


        /// <summary>
        /// Specify email server information to use if required instead of the information stored in the application configuration.
        /// </summary>
        /// <remarks>
        /// 	<para>Use this option if you want to have the packager user a specific email server
        ///     instead of the email server in the current agent configuration. This is
        ///     particularly useful if you need to authenticate to your mail relay and do not want
        ///     to have the credentials in your configuration file.</para>
        /// 	<para>The order that configuration data is used is:</para>
        /// 	<list type="bullet">
        /// 		<item>A server provided to this method will override all other configuration
        ///         options.</item>
        /// 		<item>
        ///             If no server is provided to this method, the running configuration will be
        ///             used. The running configuration can be set dynamically by subscribing to
        ///             the <see cref="Log.Initializing">Initializing Event
        ///             (Gibraltar.Agent.Log)</see> event.
        ///         </item>
        /// 		<item>If no server was provided to the Initializing event the email section in
        ///         the application configuration file is used.</item>
        /// 		<item>If no server information was specified in the gibraltar email section of
        ///         the application configuration file then the general .NET email server
        ///         configuration is used.</item>
        /// 	</list>
        /// </remarks>
        /// <param name="emailServer">The full DNS name of the SMTP server to use to relay the mail message.</param>
        /// <param name="serverPort">Optional.  The TCP/IP Port number to connect to the server on.  Use 0 for the default configured port.</param>
        /// <param name="useSsl">True to encrypt the server communication with Secure Sockets Layer.</param>
        /// <param name="emailServerUser">Optional.  A user name to use instead of the current windows user credentials to communicate with the email server.</param>
        /// <param name="emailServerPassword">Optional.  A password to use for the user name for credentials to communicate with the email server.</param>
        public void SetEmailServer(string emailServer, int serverPort, bool useSsl, string emailServerUser, string emailServerPassword)
        {
            m_EmailServer = emailServer;
            m_ServerPort = serverPort;
            m_UseSsl = useSsl;
            m_EmailServerUser = emailServerUser;
            m_EmailServerPassword = emailServerPassword;
        }

        #region Private Properties and Methods

        private UIPackagerDialog GetPackagerDialog()
        {
            //we have to ping the log object to make sure everything has been initialized.
            Monitor.Log.IsLoggingActive(false);

            UIPackagerDialog newDialog = m_UseProductFilter ? new UIPackagerDialog(m_ProductName, m_ApplicationName) : new UIPackagerDialog(); 

            if (string.IsNullOrEmpty(m_EmailServer) == false)
            {
                newDialog.SetEmailServer(m_EmailServer, m_ServerPort, m_UseSsl, m_EmailServerUser, m_EmailServerPassword);
            }

            return newDialog;
        }

        #endregion
    }
}
