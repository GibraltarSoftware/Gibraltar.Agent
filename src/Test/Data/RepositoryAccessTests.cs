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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Gibraltar.Data.Internal;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Internal;
using Gibraltar.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test.Data
{
    [TestFixture]
    public class RepositoryAccessTests
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
                // First test new re-entrant lock capability.
                using (InterprocessLock outerLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(outerLock, "Unable to outer lock the repository");

                    // Now check that we can get the same lock on the same thread.
                    using (InterprocessLock middleLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNotNull(middleLock, "Unable to reenter the repository lock on the same thread");

                        using (InterprocessLock innerLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                        {
                            Assert.IsNotNull(innerLock, "Unable to reenter the repository lock on the same thread twice");
                        }
                    }

                    using (OtherThreadLockHelper otherLock = OtherThreadLockHelper.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNull(otherLock, "Another thread was allowed to get the lock");
                    }
                }

                // Now test other scenarios while another thread holds the lock.
                using (OtherThreadLockHelper testLock = OtherThreadLockHelper.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(testLock, "Unable to lock the repository");

                    //now that I have the test lock, it should fail if I try to get it again.
                    using (InterprocessLock failedLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    using (InterprocessLock failedLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath.ToUpperInvariant(), MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    using (InterprocessLock failedLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath.ToLowerInvariant(), MultiprocessLockName, 0))
                    {
                        Assert.IsNull(failedLock, "Duplicate lock was allowed.");
                    }

                    //but I should be able to lock different repositories.
                    using (InterprocessLock secondTestLock = InterprocessLockManager.Lock(this, secondTestRepositoryPath, MultiprocessLockName, 0))
                    {
                        Assert.IsNotNull(secondTestLock, "Unable to establish lock on second repository.");

                        using (InterprocessLock thirdTestLock = InterprocessLockManager.Lock(this, thirdTestRepositoryPath, MultiprocessLockName, 0))
                        {
                            Assert.IsNotNull(thirdTestLock, "Unable to establish lock on third repository.");

                            using (InterprocessLock fourthTestLock = InterprocessLockManager.Lock(this, fourthTestRepositoryPath, MultiprocessLockName, 0))
                            {
                                Assert.IsNotNull(fourthTestLock, "Unable to establish lock on fourth repository.");
                            }
                        }
                    }
                }

                //now the lock should be released and we should be able to get it again.
                using (InterprocessLock newTestLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
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
                using (OtherThreadLockHelper testLock = OtherThreadLockHelper.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 0))
                {
                    Assert.IsNotNull(testLock, "Unable to lock the repository");

                    //now when we try to get it we should not, and should wait at least our timeout
                    DateTimeOffset lockStart = DateTimeOffset.Now;
                    using (InterprocessLock timeoutLock = InterprocessLockManager.Lock(this, firstTestRepositoryPath, MultiprocessLockName, 5))
                    {
                        //we shouldn't have the lock
                        Assert.IsNull(timeoutLock, "Duplicate lock allowed");

                        //and we should be within a reasonable delta of our timeout.
                        TimeSpan delay = DateTimeOffset.Now - lockStart;
                        Trace.Write(string.Format("Repository Timeout Requested: {0} Actual: {1}", 5, delay.TotalSeconds));
                        Assert.Greater(delay.TotalSeconds, 4.5, "Timeout happened too fast - {0} seconds", delay.TotalSeconds);
                        Assert.Less(delay.TotalSeconds, 5.5, "Timeout happened too slow - {0} seconds", delay.TotalSeconds);
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
            ContextSummary.GetApplicationNameSafe(out productName, out applicationName, out applicationVersion, out applicationDescription);



//            using (RepositoryMaintenance maintenance = new RepositoryMaintenance(userRepositoryPath, productName, applicationName, 1, 1))
            using (RepositoryMaintenance maintenance = new RepositoryMaintenance(userRepositoryPath, "Demo", "AgentTest", 45, 50, true))
            {
                maintenance.PerformMaintenance(false);
            }
        }


        #region Private subclass OtherThreadLockHelper

        /// <summary>
        /// This class will create another thread to attempt to get (and hold) a multiprocess lock, to help in testing.
        /// </summary>
        /// <remarks>It is intended to be used in a using statement just like the RepositoryLock system for easy replacement.</remarks>
        private class OtherThreadLockHelper : IDisposable
        {
            private readonly object m_Requester;
            private readonly string m_Path;
            private readonly string m_Name;
            private readonly int m_Timeout;
            private readonly object m_Lock = new object();

            private InterprocessLock m_RepositoryLock;
            private bool m_Exiting;
            private bool m_Exited;

            private OtherThreadLockHelper(object requester, string testPath, string lockName, int timeout)
            {
                m_RepositoryLock = null;
                m_Requester = requester;
                m_Path = testPath;
                m_Name = lockName;
                m_Timeout = timeout;
            }
            
            public static OtherThreadLockHelper Lock(object requester, string testRepositoryPath, string multiprocessLockName, int timeout)
            {
                OtherThreadLockHelper helper = new OtherThreadLockHelper(requester, testRepositoryPath, multiprocessLockName, timeout);
                if (helper.GetMultiprocessLock())
                    return helper;

                helper.Dispose();
                return null;
            }

            private bool GetMultiprocessLock()
            {
                Thread helperThread = new Thread(HelperThreadStart);
                helperThread.TrySetApartmentState(ApartmentState.MTA);
                helperThread.Name = "Lock test helper";
                helperThread.IsBackground = true;
                lock (m_Lock)
                {
                    helperThread.Start();

                    System.Threading.Monitor.PulseAll(m_Lock);
                    while (m_RepositoryLock == null && m_Exited == false)
                    {
                        System.Threading.Monitor.Wait(m_Lock);
                    }

                    if (m_RepositoryLock != null && m_RepositoryLock.IsDisposed == false)
                        return true;
                    else
                        return false;
                }
            }

            private void HelperThreadStart()
            {
                lock (m_Lock)
                {
                    m_RepositoryLock = InterprocessLockManager.Lock(this, m_Path, m_Name, m_Timeout);

                    if (m_RepositoryLock != null)
                    {
                        System.Threading.Monitor.PulseAll(m_Lock);

                        while (m_Exiting == false)
                        {
                            System.Threading.Monitor.Wait(m_Lock); // Thread waits until we're told to exit.
                        }

                        m_RepositoryLock.Dispose(); // We're exiting, so it's time to release the lock!
                        m_RepositoryLock = null;
                    }
                    // Otherwise, we couldn't get the lock.

                    m_Exited = true; // Lock is released and thread is exiting.
                    System.Threading.Monitor.PulseAll(m_Lock);
                }
            }

            public void Dispose()
            {
                if (m_Exited)
                    return;

                lock (m_Lock)
                {
                    m_Exiting = true; // Signal the other thread it needs to release the lock and exit.

                    System.Threading.Monitor.PulseAll(m_Lock); // Pulse that we changed the status.
                    while (m_Exited == false)
                    {
                        System.Threading.Monitor.Wait(m_Lock);
                    }

                    // We're now released, so we can return from the Dispose() call.
                }
            }

        } // private class OtherThreadLockHelper

        #endregion OtherThreadLockHelper
    }
}