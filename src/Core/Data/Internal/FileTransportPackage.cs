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
using System.Globalization;
using System.IO;

#endregion File Header

namespace Gibraltar.Data.Internal
{
    /// <summary>
    /// A transport package that is just being written out to a file.
    /// </summary>
    internal class FileTransportPackage : TransportPackageBase
    {
        public FileTransportPackage(string product, string application, SimplePackage package, string fileNamePath)
            : base(product, application, package)
        {
            if (string.IsNullOrEmpty(fileNamePath))
                throw new ArgumentNullException(nameof(fileNamePath));

            FileNamePath = fileNamePath;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The full file name and path to write out to.
        /// </summary>
        public string FileNamePath { get; private set; }

        #endregion

        #region Private Properties and Methods

        protected override PackageSendEventArgs OnSend(ProgressMonitorStack progressMonitors)
        {
            int fileSizeBytes = 0;
            AsyncTaskResult result;
            string statusMessage;
            Exception taskException = null;
            try
            {
                //all we do is save the file out to our target path.
                Package.Save(progressMonitors, FileNamePath); // Uh-oh, we have to save it again!

                result = AsyncTaskResult.Success;
                fileSizeBytes = (int)FileSystemTools.GetFileSize(FileNamePath);
                statusMessage = string.Format(CultureInfo.CurrentCulture, "Package written to file {0}",
                                              Path.GetFileNameWithoutExtension(FileNamePath));
            }
            catch (Exception ex)
            {
                result = AsyncTaskResult.Error;
                statusMessage =
                    "Unable to save the package to disk.\r\n\r\nIt's possible that you don't have sufficient access to the directory to write the file or the media is read-only.";
                taskException = ex;
            }

            return new PackageSendEventArgs(fileSizeBytes, result, statusMessage, taskException);
        }

        #endregion
    }
}
