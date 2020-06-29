#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by eSymmetrix, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of eSymmetrix, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by eSymmetrix, Inc.  All rights reserved.
 *******************************************************************/

using System;
using System.IO;
using Gibraltar.Data.Internal;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Internal;
using Gibraltar.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Data
{
    [TestFixture]
    public class RepositoryTests
    {
        [Test]
        public void LockRepository()
        {
            const string MultiprocessLockName = "LockRepository";

            string firstTestRepositoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string secondTestRepositoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string thirdTestRepositoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string fourthTestRepositoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {

                using (RepositoryLock testLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(testLock, "Unable to lock the repository");

                    //now that I have the test lock, it should fail if I try to get it again.
                    using (RepositoryLock failedLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    using (RepositoryLock failedLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath.ToUpperInvariant(), MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    using (RepositoryLock failedLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath.ToLowerInvariant(), MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    //but I should be able to lock different repositories.
                    using (RepositoryLock secondTestLock = RepositoryLockManager.Lock(this, secondTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNotNull(secondTestLock, "Unable to establish lock on second repository.");

                        using (RepositoryLock thirdTestLock = RepositoryLockManager.Lock(this, thirdTestRepositoryPath, MultiprocessLockName, 0))
                        {
                            Assert.IsNotNull(thirdTestLock, "Unable to establish lock on third repository.");

                            using (RepositoryLock fourthTestLock = RepositoryLockManager.Lock(this, fourthTestRepositoryPath, MultiprocessLockName, 0))
                            {
                                Assert.IsNotNull(fourthTestLock, "Unable to establish lock on fourth repository.");
                            }
                        }
                    }
                }

                //now the lock should be released and we should be able to gegt it again.
                using (RepositoryLock newTestLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(newTestLock, "Unable to re-lock the repository immediately after release.");
                }
            }
            finally
            {
                //and clean up after ourselves.
                if (Directory.Exists(firstTestRepositoryPath))
                    Directory.Delete(firstTestRepositoryPath, true);

                if (Directory.Exists(secondTestRepositoryPath))
                    Directory.Delete(secondTestRepositoryPath, true);

                if (Directory.Exists(thirdTestRepositoryPath))
                    Directory.Delete(thirdTestRepositoryPath, true);

                if (Directory.Exists(fourthTestRepositoryPath))
                    Directory.Delete(fourthTestRepositoryPath, true);
            }
        }

        [Test]
        public void LockRepositoryTimeout()
        {
            const string MultiprocessLockName = "LockRepositoryTimeout";

            string firstTestRepositoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
 
            try
            {
                using (RepositoryLock testLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(testLock, "Unable to lock the repository");

                    //now when we try to get it we should not, and should wait at least our timeout
                    DateTimeOffset lockStart = DateTimeOffset.Now;
                    using (RepositoryLock timeoutLock = RepositoryLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 5))
                    {
                        //we shouldn't have the lock
                        Assert.IsNull(timeoutLock, "Duplicate lock allowed");

                        //and we should be within a reasonable delta of our timeout.
                        TimeSpan delay = DateTimeOffset.Now - lockStart;
                        Assert.IsTrue(delay.TotalSeconds > 4.5, "Timeout happened too fast - {0} seconds", delay.TotalSeconds);
                        Assert.IsTrue(delay.TotalSeconds < 5.5, "Timeout happened too slow - {0} seconds", delay.TotalSeconds);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(firstTestRepositoryPath))
                    Directory.Delete(firstTestRepositoryPath, true);
            }
        }

        [Test]
        public void ConnectToCollectionRepository()
        {
            Repository collector = RepositoryManager.Collection;

            Assert.IsNotNull(collector);
        }

        [Test]
        public void ConnectToUserRepository()
        {
            Repository userRepository = RepositoryManager.User;

            Assert.IsNotNull(userRepository);
        }

        [Test]
        public void PerformCollectorCompact()
        {
            string userRepositoryPath = PathManager.FindBestPath(PathType.Collection);
            using(IndexManager indexManager = new IndexManager(userRepositoryPath, true))
            {
                indexManager.Open();

                bool isCompacted = indexManager.CompactDatabase();
                Assert.IsTrue(isCompacted);
            }
        }

        [Test]
        public void PerformCollectorMaintenance()
        {
            string userRepositoryPath = PathManager.FindBestPath(PathType.Collection);
            string productName, applicationName, applicationDescription;
            Version applicationVersion;
            SessionSummary.GetApplicationNameSafe(out productName, out applicationName, out applicationVersion, out applicationDescription);



//            using (RepositoryMaintenance maintenance = new RepositoryMaintenance(userRepositoryPath, productName, applicationName, 1, 1))
            using (RepositoryMaintenance maintenance = new RepositoryMaintenance(userRepositoryPath, "Demo", "AgentTest", 45, 50, true))
            {
                maintenance.PerformMaintenance(false);
            }
        }


        [Test]
        public void PerformSessionCleanup()
        {
            RepositoryManager.Collection.SessionCleanup(false);
        }
    }
}