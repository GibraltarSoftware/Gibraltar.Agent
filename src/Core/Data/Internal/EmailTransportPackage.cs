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
using System.IO;
using System.Net.Mail;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Data.Internal
{
    /// <summary>
    /// Transport a package via email
    /// </summary>
    internal class EmailTransportPackage : TransportPackageBase
    {
        private string m_TemporaryFileNamePath;

        public EmailTransportPackage(string product, string application, SimplePackage package)
            : base(product, application, package)
        {

        }

        public string EmailFromAddress { get; set; }

        public string EmailServer { get; set; }

        public string EmailServerUser { get; set; }

        public string EmailServerPassword { get; set; }

        public string EmailToAddress { get; set; }

        public string EmailSubject { get; set; }

        public int EmailServerPort { get; set; }

        public bool? EmailUseSsl { get; set; }

        #region Protected Properties and Methods

        /// <summary>
        /// The location of the temporary package file
        /// </summary>
        protected string PackageFileNamePath
        {
            get { return m_TemporaryFileNamePath; }
        }

        /// <summary>
        /// Prepares the package for transport, primarily by saving it to a temporary file location.
        /// </summary>
        /// <param name="progressMonitors"></param>
        protected void PreparePackage(ProgressMonitorStack progressMonitors)
        {
            if (Package.IsDirty)
            {
                if (string.IsNullOrEmpty(Package.FileNamePath))
                {
                    //never saved (huh?) assign it a temp file name.
                    string temporaryFileNamePath = Path.GetTempFileName();
                    //it just made a file in that location, best to blow it away before we go further (get temp file name creates a 0 byte file)
                    File.Delete(temporaryFileNamePath);

                    Package.Save(progressMonitors, temporaryFileNamePath);
                }
            }

            //store off the file name of the package so we can be sure to blow it away on close.
            m_TemporaryFileNamePath = Package.FileNamePath;
        }

        protected override PackageSendEventArgs OnSend(ProgressMonitorStack progressMonitors)
        {
            int fileSizeBytes = 0;
            AsyncTaskResult result;
            string statusMessage;
            Exception taskException = null;
            try
            {
                PreparePackage(progressMonitors);

                MailPriority priority = HasProblemSessions ? MailPriority.High : MailPriority.Normal;

                EmailMessage newMessage = new EmailMessage(EmailFromAddress, EmailServer, EmailServerUser, EmailServerPassword, EmailServerPort, EmailUseSsl.Value);

                //generate a pretty display name for the attachment.
                string packageName = string.Format(FileSystemTools.UICultureFormat, "{0}{1}{2} Sessions.{3}", Product, string.IsNullOrEmpty(Application) ? string.Empty : " ", Application, Log.PackageExtension);
                packageName = newMessage.SanitizeFileName(packageName);

                newMessage.AddAddress(EmailToAddress);
                newMessage.AddAttachment(PackageFileNamePath, packageName);
                newMessage.Subject = (string.IsNullOrEmpty(EmailSubject) ? Package.Caption : EmailSubject);
                newMessage.Body = Package.Description;
                newMessage.Priority = priority;

                newMessage.Send();

                statusMessage = string.Format(FileSystemTools.UICultureFormat, "Email successfully sent to {0}",
                                              EmailToAddress);
                result = AsyncTaskResult.Success;
                FileInfo destinationFile = new FileInfo(PackageFileNamePath);
                fileSizeBytes = (int)destinationFile.Length;
            }
            catch (Exception ex)
            {
                result = AsyncTaskResult.Error;
                statusMessage =
                    "Unable to send email.\r\n\r\nIt's possible that your mail server is unavailable, your Internet connection is unavailable, or that Antivirus software on your computer is blocking email.\r\n\r\nVerify that you have an active Internet connection and that you don't have software on your computer that will block applications sending email.";
                taskException = ex;
            }

            return new PackageSendEventArgs(fileSizeBytes, result, statusMessage, taskException);
        }

        /// <summary>
        /// Performs the actual releasing of managed and unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Most usage should instead call Dispose(), which will call Dispose(true) for you
        /// and will suppress redundant finalization. Note to inheritors:  Be sure to call base to enable the base class to release resources.</remarks>
        /// <param name="releaseManaged">Indicates whether to release managed resources.
        /// This should only be called with true, except from the finalizer which should call Dispose(false).</param>
        protected override void Dispose(bool releaseManaged)
        {
            base.Dispose(releaseManaged);

            if (releaseManaged)
            {
                try
                {
                    File.Delete(m_TemporaryFileNamePath);
                }
                catch (Exception ex)
                {
                    if (!Log.SilentMode)
                        Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, Packager.LogCategory, "Unable to delete temporary working email package file",
                                  "Unable to delete temporary working package at {0} due to an exception ({1}): {2}",
                                  m_TemporaryFileNamePath, ex.GetType().FullName, ex.Message);
                }
            }
        }

        #endregion
    }

}
