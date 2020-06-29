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

using System.Collections.Generic;
using System.IO;
using Gibraltar.Messaging;
using Gibraltar.Monitor;

namespace Gibraltar.Data
{
    /// <summary>
    /// Performs repository level maintenance such as purging for size.  Should be used with collection repositories only.
    /// </summary>
    public class RepositoryMaintenance: FileMaintenanceBase
    {
        private const string FileExtension = Log.LogExtension;

        private readonly string m_RepositoryArchivePath;

        /// <summary>
        /// Create a repository maintenance object for the provided repository without the ability to perform pruning.
        /// </summary>
        /// <param name="repositoryPath"></param>
        /// <param name="loggingEnabled">Indicates if the maintenance process should log its actions.</param>
        public RepositoryMaintenance(string repositoryPath, bool loggingEnabled)
        : base(repositoryPath, loggingEnabled)
        {
            m_RepositoryArchivePath = Path.Combine(RepositoryPath, LocalRepository.RepositoryArchiveFolder);
        }

        /// <summary>
        /// Create the repository maintenance object for the provided repository.
        /// </summary>
        /// <param name="repositoryPath">The full path to the base of the repository (which must contain an index)</param>
        /// <param name="productName">The product name of the application(s) to restrict pruning to.</param>
        /// <param name="applicationName">Optional.  The application within the product to restrict pruning to.</param>
        /// <param name="maxAgeDays">The maximum allowed days since the session fragment was closed to keep the fragment around.</param>
        /// <param name="maxSizeMegabytes">The maximum number of megabytes of session fragments to keep</param>
        /// <param name="loggingEnabled">Indicates if the maintenance process should log its actions.</param>
        public RepositoryMaintenance(string repositoryPath, string productName, string applicationName, int maxAgeDays, int maxSizeMegabytes, bool loggingEnabled)
        :base(repositoryPath, productName, applicationName, maxAgeDays, maxSizeMegabytes, loggingEnabled)
        {
            m_RepositoryArchivePath = Path.Combine(RepositoryPath, LocalRepository.RepositoryArchiveFolder);
        }

        #region Public Properties and Methods

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Implemented by derived classes to supply the list of files that can be considered for pruning
        /// </summary>
        /// <returns></returns>
        protected override List<FileInfo> OnGetEligibleFileInfo()
        {
            var fragmentPattern = string.Format("{0}*.{1}", FileMessenger.SessionFileNamePrefix(ProductName, ApplicationName), FileExtension);

            var repository = new DirectoryInfo(RepositoryPath);
            var fileFragments = new List<FileInfo>(repository.GetFiles(fragmentPattern, SearchOption.TopDirectoryOnly));
            if (Directory.Exists(m_RepositoryArchivePath))
            {
                var archiveFolder = new DirectoryInfo(m_RepositoryArchivePath);
                fileFragments.AddRange(archiveFolder.GetFiles(fragmentPattern));
            }
            return fileFragments;
        }

        #endregion

        #region Private Properties and Methods

        #endregion
    }
}