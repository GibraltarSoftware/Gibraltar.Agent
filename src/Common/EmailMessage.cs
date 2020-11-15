
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;

#endregion File Header

namespace Gibraltar
{
    /// <summary>
    /// Wrapper for the System.Net.Mail classes
    /// </summary>
    public class EmailMessage
    {
        private readonly List<string> m_Addresses = new List<string>();
        private readonly List<NameValuePair<string>> m_Attachments = new List<NameValuePair<string>>();

        /// <summary>
        /// Create a new email message to send using the current process default email server settings
        /// </summary>
        /// <param name="fromAddress">The email address the message is coming from</param>
        public EmailMessage(string fromAddress)
        {
            if (string.IsNullOrEmpty(fromAddress))
                throw new ArgumentNullException(nameof(fromAddress));

            FromAddress = fromAddress;
            Priority = MailPriority.Normal;
        }

        /// <summary>
        /// Create a new email message to send using an explicit server 
        /// </summary>
        /// <param name="emailServer">The full DNS name of the email server to use to send the message</param>
        /// <param name="fromAddress">The email address the message is coming from</param>
        public EmailMessage(string fromAddress, string emailServer)
        {
            if (string.IsNullOrEmpty(fromAddress))
                throw new ArgumentNullException(nameof(fromAddress));

            FromAddress = fromAddress;
            Priority = MailPriority.Normal;

            //protect from getting fooled in certain cases.
            EmailServer = StandardizeEmptyString(emailServer);
        }

        /// <summary>
        /// Create a new email message to send using an explicit server  and authentication
        /// </summary>
        /// <param name="emailServer">The full DNS name of the email server to use to send the message</param>
        /// <param name="emailServerUser">The user name to authenticate to the email server with.  Optional.</param>
        /// <param name="emailServerPassword">The password to authenticate to the email server with. Optional.</param>
        /// <param name="fromAddress">The email address the message is coming from</param>
        /// <remarks>If specifying a user a non-empty password must also be specified.</remarks>
        public EmailMessage(string fromAddress, string emailServer, string emailServerUser, string emailServerPassword)
        {
            if (string.IsNullOrEmpty(fromAddress))
                throw new ArgumentNullException(nameof(fromAddress));

            FromAddress = fromAddress;
            Priority = MailPriority.Normal;

            //protect from getting fooled in certain cases.
            EmailServer = StandardizeEmptyString(emailServer);

            if (string.IsNullOrEmpty(EmailServer) == false)
            {
                EmailServerPassword = StandardizeEmptyString(emailServerPassword);

                //Only set the user name if a password was set.
                if (string.IsNullOrEmpty(EmailServerPassword) == false)
                {
                    EmailServerUser = StandardizeEmptyString(emailServerUser);                    
                }
            }
        }

        /// <summary>
        /// Create a new email message to send using an explicit server, port, and authentication
        /// </summary>
        /// <param name="emailServer">The full DNS name of the email server to use to send the message</param>
        /// <param name="emailServerUser">The user name to authenticate to the email server with.  Optional.</param>
        /// <param name="emailServerPassword">The password to authenticate to the email server with. Optional.</param>
        /// <param name="fromAddress">The email address the message is coming from</param>
        /// <param name="serverPort">The TCP/IP port to use to connect to the server.  Specify 0 to use the default configured for this computer.</param>
        /// <param name="useSsl">True to encrypt the server communication with Ssl.</param>
        /// <remarks>If specifying a user a non-empty password must also be specified.</remarks>
        public EmailMessage(string fromAddress, string emailServer, string emailServerUser, string emailServerPassword, int serverPort, bool useSsl)
            : this(fromAddress, emailServer, emailServerUser, emailServerPassword )
        {
            ServerPort = serverPort;
            UseSsl = useSsl;
        }


        #region Public Properties and Methods

        /// <summary>
        /// Remove any invalid file name characters, replacing them with substitutes.
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <returns></returns>
        public string SanitizeFileName(string originalFileName)
        {
            string sanitizedName = originalFileName;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sanitizedName = sanitizedName.Replace(c, '_');
            }

            return sanitizedName;
        }

        /// <summary>
        /// Add an email address
        /// </summary>
        /// <param name="newAddress"></param>
        public void AddAddress(string newAddress)
        {
            string effectiveAddress = StandardizeEmptyString(newAddress);
            if (effectiveAddress != null)
            {
                m_Addresses.Add(effectiveAddress);
            }
        }

        /// <summary>
        /// Add a file attachment
        /// </summary>
        /// <param name="fileNamePath">The fully qualified name and path to the attachment to add</param>
        public void AddAttachment(string fileNamePath)
        {
            string effectiveFileNamePath = StandardizeEmptyString(fileNamePath);
            if (effectiveFileNamePath != null)
            {
                m_Attachments.Add(new NameValuePair<string>(fileNamePath, string.Empty));
            }
        }

        /// <summary>
        /// Add a file attachment
        /// </summary>
        /// <param name="fileNamePath">The fully qualified name and path to the attachment to add</param>
        /// <param name="displayName">A display name to use for the attachment instead of the file name.</param>
        public void AddAttachment(string fileNamePath, string displayName)
        {
            string effectiveFileNamePath = StandardizeEmptyString(fileNamePath);
            if (effectiveFileNamePath != null)
            {
                m_Attachments.Add(new NameValuePair<string>(fileNamePath, displayName));
            }
        }
        /// <summary>
        /// A body to send with the email
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The email server that will be used
        /// </summary>
        public string EmailServer { get; private set; }

        /// <summary>
        /// Optional.  User credentials for the email server
        /// </summary>
        public string EmailServerUser { get; private set; }

        /// <summary>
        /// Optional.  A password for the email server.
        /// </summary>
        public string EmailServerPassword { get; private set; }

        /// <summary>
        /// The email address the email is from.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Indicates the network port that should be used to communicate with the server
        /// </summary>
        /// <remarks>If set to zero, the default port (25) will be used.</remarks>
        public int ServerPort { get; set; }

        /// <summary>
        /// Indicates if the network connection should use Secure Sockets Layer.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// True if the body of the email is HTML formatted
        /// </summary>
        public bool IsBodyHTML { get; set; }

        /// <summary>
        /// The priority to flag the email with.
        /// </summary>
        public MailPriority Priority { get; set; }

        /// <summary>
        /// A subject line for the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Send the email
        /// </summary>
        public void Send()
        {
            //we're relying on exception handling here.
            SendEmail();
        }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// Convert an input null, empty, or purely whitespace string to null.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static string StandardizeEmptyString(string original)
        {
            if (string.IsNullOrEmpty(original))
                return null;

            string trimmedValue = original.Trim();

            if (string.IsNullOrEmpty(trimmedValue))
                return null;

            return trimmedValue;
        }

        private void SendEmail()
        {
            List<Stream> attachmentStreams = new List<Stream>();
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(FromAddress);

                    foreach (string address in m_Addresses)
                    {
                        message.To.Add(new MailAddress(address));
                    }

                    message.Subject = Subject;
                    message.IsBodyHtml = IsBodyHTML;
                    message.Priority = Priority;
                    message.Body = Body;

                    foreach (NameValuePair<string> attachment in m_Attachments)
                    {
                        Attachment fileAttachment;
                        if (string.IsNullOrEmpty(attachment.Value))
                        {
                            //we want to use the file name as the display file name, so do a direct attachment.
                            fileAttachment = new Attachment(attachment.Name);
                        }
                        else
                        {
                            //we want to override the file name, so we have to do our own stream handling.  We can't dispose of the stream until
                            //after the message is sent, so no using for us!
                            Stream fileStream = File.OpenRead(attachment.Name);
                            attachmentStreams.Add(fileStream); //so we can dispose of it later.

                            fileAttachment = new Attachment(fileStream, attachment.Value);
                        }

                        message.Attachments.Add(fileAttachment);
                    }

                    //now that we've made the message, get a valid SMTP client and send that bad boy
                    SmtpClient client = GetSmtpClient();

                    client.Send(message);
                }
            }
            finally
            {
                //dispose of any attachment streams we opened.
                foreach (Stream stream in attachmentStreams)
                {
                    stream.Dispose();
                }
            }
        }

        private SmtpClient GetSmtpClient()
        {
            //The email server doesn't have to be provided; it may be supposed to come from the 
            //application or machine configuration files.  See if we got one to determine what constructor to use.
            SmtpClient client = string.IsNullOrEmpty(EmailServer) ? new SmtpClient()
                : (ServerPort == 0) ? new SmtpClient(EmailServer) : new SmtpClient(EmailServer, ServerPort);

            //now see if we need to set credentials for the SMTP server
            client.Credentials = string.IsNullOrEmpty(EmailServerPassword) == false ? new NetworkCredential(EmailServerUser, EmailServerPassword) : CredentialCache.DefaultNetworkCredentials;

            client.EnableSsl = UseSsl;

            client.Timeout = 600000; //ten minutes...

            return client;
        }

        #endregion
    }
}
