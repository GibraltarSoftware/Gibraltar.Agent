using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Gibraltar.Analyst.Data;
using Gibraltar.Data;
using Gibraltar.Monitor;
using NUnit.Framework;

namespace Gibraltar.Test.Data
{
    [TestFixture]
    public class RepositoryOperationTests
    {
        private const string DistribDirectoryPath = @"C:\Development\Gibraltar\Gibraltar\Trunk\Distrib\";
        private const string StockPackageLocation = DistribDirectoryPath + "StockTestPackage.glp";
        private const string StockPackageLocation2 = DistribDirectoryPath + "StockTestPackage2.glp";
        private const string NewStockPackageLocation = DistribDirectoryPath + "StockTestPackageNew.glp";
        private const string NewStockPackageLocation2 = DistribDirectoryPath + "StockTestPackageNew2.glp";
        private string m_OutputFileNamePath;
        private ProgressMonitorStack m_ProgressMonitorStack;
        private ProgressMonitor m_TopProgressMonitor;

        [OneTimeSetUp]
        public void Init()
        {
            Cleanup(); // In case we're reinitializing?  Wipe any leftovers we still see.

            m_OutputFileNamePath = Path.GetTempFileName();
            File.Delete(m_OutputFileNamePath); //get temp file name creates the file as part of allocating the name.

            m_ProgressMonitorStack = new ProgressMonitorStack("Repository Operations Unit Tests");
            m_TopProgressMonitor = m_ProgressMonitorStack.NewMonitor(this, "Running tests", 0);
        }

        [Test]
        public void CreateNewPackage()
        {
            Package newPackage = new Package();
            RepositoryManager.Repositories.Add(newPackage);
            try
            {
                Assert.IsTrue(newPackage.IsDirty, "New package did not report itself as dirty (unsaved).");

                Exception exception = null;
                try
                {
                    newPackage.Save(m_ProgressMonitorStack); // Should throw an exception.
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Log.Write(LogMessageSeverity.Verbose, LogWriteMode.Queued, ex, "Unit tests.Repository.Operations", "Expected exception caught",
                              "Attempt to save a new package without specifying a filename threw an exception as expected.");
                }
                Assert.IsNotNull(exception, "Saving new package without a filename did not throw an exception as expected.");
                Assert.IsNotNull(exception as FileNotFoundException, "Saving new package without a filename did not throw expected FileNotFoundException.");
                Assert.IsTrue(newPackage.IsDirty, "New package did not report itself as dirty (unsaved) after failed save.");

                newPackage.Save(m_ProgressMonitorStack, m_OutputFileNamePath);
                Assert.IsFalse(newPackage.IsDirty, "Saved new package reports itself as still dirty after successful save.");

                int sessionCount, problemCount, fileCount;
                long fileBytes;
                newPackage.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);
                Assert.AreEqual(0, sessionCount, "Expected empty package has a non-zero session count.");

                // Now let's try merging in our stock package so we can play with some sessions.

                newPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation);
                Assert.IsTrue(newPackage.IsDirty, "Modified package did not report itself as dirty after a merge.");
                newPackage.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);
                Assert.AreEqual(9, sessionCount);
                Assert.AreEqual(0, problemCount);
                newPackage.Save(m_ProgressMonitorStack);
                Assert.IsFalse(newPackage.IsDirty, "Merged package still reported itself as dirty after a save.");
            }
            finally
            {
                RepositoryManager.Repositories.Remove(newPackage); // And remove it from the Repositories collection when done.
            }
        }

        [Test]
        public void CreateFolders() {
            Package newPackage = new Package();
            RepositoryManager.Repositories.Add(newPackage);
            try
            {
                // Make a folder

                RepositoryFolder basicFolder = newPackage.Folders.Add("Basic", RepositoryFolderType.Manual);
                // Apparently adding a folder does not set the dirty flag (by design?).
                //Assert.IsTrue(newPackage.IsDirty, "Modified package did not report itself as dirty after a folder add.");
                newPackage.Save(m_ProgressMonitorStack, m_OutputFileNamePath);
                //Assert.IsFalse(newPackage.IsDirty, "Package still reported itself as dirty after a save.");

                newPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation, basicFolder); // Same, now in subfolder

                int sessionCount, problemCount, fileCount;
                long fileBytes;
                newPackage.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);
                Assert.AreEqual(9, sessionCount);
                Assert.AreEqual(0, problemCount);
                //Assert.IsTrue(newPackage.IsDirty, "Modified package did not report itself as dirty after merge to a folder.");
                newPackage.Save(m_ProgressMonitorStack);
                Assert.IsFalse(newPackage.IsDirty, "Package still reported itself as dirty after a save.");

                DataSet dataSet = basicFolder.GetSessions();
                int folderCount = dataSet.Tables[0].Rows.Count;
                Assert.AreEqual(9, folderCount, "'Basic' folder does not contain the expected count of 9 sessions.");

                // Now merge in the second stock package, in a destination folder.

                RepositoryFolder interestingFolder;
                interestingFolder = newPackage.Folders.Add("Interesting", RepositoryFolderType.Manual);
                //Assert.IsTrue(newPackage.IsDirty, "Modified package did not report itself as dirty after a folder add.");
                //newPackage.Save(m_ProgressMonitorStack);
                //Assert.IsFalse(newPackage.IsDirty, "Package still reported itself as dirty after a save.");

                newPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation2, interestingFolder);
                newPackage.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);
                Assert.AreEqual(11, sessionCount);
                Assert.AreEqual(1, problemCount);
                //Assert.IsTrue(newPackage.IsDirty, "Modified package did not report itself as dirty after a second merge.");
                newPackage.Save(m_ProgressMonitorStack);
                Assert.IsFalse(newPackage.IsDirty, "Merged package still reported itself as dirty after a save.");

                dataSet = interestingFolder.GetSessions();
                folderCount = dataSet.Tables[0].Rows.Count;
                Assert.AreEqual(5, folderCount, "'Interesting' folder does not contain the expected count of 5 sessions.");

                dataSet = basicFolder.GetSessions();
                folderCount = dataSet.Tables[0].Rows.Count;
                Assert.AreEqual(9, folderCount, "'Basic' folder no longer contains the expected count of 9 sessions.");
            }
            finally
            {
                RepositoryManager.Repositories.Remove(newPackage); // And remove it from the Repositories collection when done.
            }
        }

        [Ignore("debugging test only")]
        [Test]
        public void PackageSessionCompaction()
        {
            Package stockPackage = null;
            try
            {
                stockPackage = new Package();
                RepositoryManager.Repositories.Add(stockPackage);

                stockPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation);
                DataSet dataSet = stockPackage.GetSessions();
                DataRowCollection rowCollection = dataSet.Tables[0].Rows;
                foreach (DataRow row in rowCollection)
                {
                    Guid sessionGuid = (Guid)row["Session_Id"];
                    Session session = stockPackage.GetSession(sessionGuid);
                    if (session.IsLoaded == false)
                        session.Load();
                }

                stockPackage.Save(m_ProgressMonitorStack, NewStockPackageLocation);

                RepositoryManager.Repositories.Remove(stockPackage);
                stockPackage = new Package();
                RepositoryManager.Repositories.Add(stockPackage);

                stockPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation2);
                dataSet = stockPackage.GetSessions();
                rowCollection = dataSet.Tables[0].Rows;
                foreach (DataRow row in rowCollection)
                {
                    Guid sessionGuid = (Guid)row["Session_Id"];
                    Session session = stockPackage.GetSession(sessionGuid);
                    if (session.IsLoaded == false)
                        session.Load();
                }

                stockPackage.Save(m_ProgressMonitorStack, NewStockPackageLocation2);
            }
            finally
            {
                if (stockPackage != null)
                    RepositoryManager.Repositories.Remove(stockPackage);

            }
        }

        [Test]
        public void SessionPackageShuffle()
        {
            string tempOne = Path.GetTempFileName();
            File.Delete(tempOne);
            string tempTwo = Path.GetTempFileName();
            File.Delete(tempTwo);
            Package startPackage = null;
            Package packageOne = null;
            Package packageTwo = null;
            Package packageNew = null; // Don't save this one to a file.
            Stream expectedStream = null;
            Stream expectedStream2 = null;
            try
            {
                startPackage = new Package();
                RepositoryManager.Repositories.Add(startPackage);

                startPackage.MergeFile(m_ProgressMonitorStack, StockPackageLocation);
                startPackage.Save(m_ProgressMonitorStack, m_OutputFileNamePath);

                DataSet dataSet = startPackage.GetSessions();
                DataRowCollection rowCollection = dataSet.Tables[0].Rows;
                Assert.Greater(rowCollection.Count, 0, "Package contains no sessions.");
                DataRow row = rowCollection[0];
                Guid sessionGuid = (Guid)row["Session_Id"];
                Guid[] sessionGuids = new[] {sessionGuid};
                expectedStream = startPackage.GetSessionStream(sessionGuid); // Get the stream to compare against.

                // Now try copying the session to another package.

                RepositoryCopyLink copyLink = startPackage.CreateCopyLink(null, sessionGuids); // Copy from startPackage.
                long expectedLength = GetSessionLength(startPackage, sessionGuid);
                Assert.Greater(expectedLength, 0, "Session stream had no bytes.");

                // From saved package to an empty saved package.
                packageOne = new Package();
                RepositoryManager.Repositories.Add(packageOne);
                packageOne.Save(m_ProgressMonitorStack, tempOne);
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageOne.
                int sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(1, sessionCount);
                long sessionLength = GetSessionLength(packageOne, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageOne, sessionGuid), "Session data mismatch after copy.");

                // From unsaved package to a new unsaved package.
                packageTwo = new Package();
                RepositoryManager.Repositories.Add(packageTwo);
                copyLink = packageOne.CreateCopyLink(null, sessionGuids); // Copy from packageOne.
                packageTwo.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageTwo.
                packageTwo.Save(m_ProgressMonitorStack, tempTwo);
                sessionCount = RepositorySessionCount(packageTwo);
                Assert.AreEqual(1, sessionCount);
                sessionLength = GetSessionLength(packageTwo, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageTwo, sessionGuid), "Session data mismatch after copy.");

                // From saved package to a new unsaved package.
                packageNew = new Package();
                RepositoryManager.Repositories.Add(packageNew);
                copyLink = packageTwo.CreateCopyLink(null, sessionGuids); // Copy from packageTwo.
                packageNew.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageNew.
                sessionCount = RepositorySessionCount(packageNew);
                Assert.AreEqual(1, sessionCount);
                sessionLength = GetSessionLength(packageNew, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageNew, sessionGuid), "Session data mismatch after copy.");

                // From unsaved package to a saved package which already has the session.
                copyLink = packageNew.CreateCopyLink(null, sessionGuids); // Copy from packageNew.
                RepositoryFolder roundTripFolder = packageOne.Folders.Add("Round trip", RepositoryFolderType.Manual);
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink, roundTripFolder);
                packageOne.Save(m_ProgressMonitorStack);
                sessionCount = FolderSessionCount(roundTripFolder);
                Assert.AreEqual(1, sessionCount); // Make sure it's counted in the folder.
                sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(1, sessionCount); // Still only 1 session total.
                sessionLength = GetSessionLength(packageOne, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageOne, sessionGuid), "Session data mismatch after copy.");

                // Find a second session in the startPackage.
                Assert.Greater(rowCollection.Count, 1, "Package only contains 1 session.");
                row = rowCollection[1];
                Guid sessionGuid2 = (Guid)row["Session_Id"];
                Guid[] sessionGuids2 = new[] {sessionGuid2};
                long expectedLength2 = GetSessionLength(startPackage, sessionGuid2);
                expectedStream2 = startPackage.GetSessionStream(sessionGuid2); // Get the second stream to compare against.

                // From saved package to a non-empty saved package.
                copyLink = startPackage.CreateCopyLink(null, sessionGuids2); // Copy from startPackage.
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageOne.
                sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageOne, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageOne, sessionGuid2), "Session data mismatch after copy.");

                // From unsaved package to a non-empty saved package.
                copyLink = packageOne.CreateCopyLink(null, sessionGuids2); // Copy from packageOne.
                packageTwo.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageTwo.
                sessionCount = RepositorySessionCount(packageTwo);
                Assert.AreEqual(2, sessionCount);
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageTwo, sessionGuid2), "Session data mismatch after copy.");

                // Save and close out packageTwo.  Then reopen it.
                packageTwo.Save(m_ProgressMonitorStack);
                RepositoryManager.Repositories.Remove(packageTwo);
                packageTwo = new Package(tempTwo);
                RepositoryManager.Repositories.Add(packageTwo);
                Assert.IsFalse(packageTwo.IsDirty, "Reopened package reported itself as dirty.");
                sessionCount = RepositorySessionCount(packageTwo);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageTwo, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageTwo, sessionGuid2), "Session data mismatch after copy.");

                // From reopened package to a non-empty unsaved package.
                copyLink = packageTwo.CreateCopyLink(null, sessionGuids2); // Copy from packageTwo.
                packageNew.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageNew.
                sessionCount = RepositorySessionCount(packageNew);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageNew, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageNew, sessionGuid2), "Session data mismatch after copy.");

                // From (same) reopened package to a non-empty saved package which already has the session.
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink, roundTripFolder); // Copy into packageOne.
                sessionCount = FolderSessionCount(roundTripFolder);
                Assert.AreEqual(2, sessionCount);
                sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageOne, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageOne, sessionGuid2), "Session data mismatch after copy.");

                // Reset packageNew to an empty package.
                RepositoryManager.Repositories.Remove(packageNew); // Discard packageNew.
                packageNew = new Package();                        // And make a new one.
                RepositoryManager.Repositories.Add(packageNew);
                sessionCount = RepositorySessionCount(packageNew);
                Assert.AreEqual(0, sessionCount);

                // Copy folder into unsaved empty package.
                Guid[] folderGuids = new[] {roundTripFolder.Id};
                copyLink = packageOne.CreateCopyLink(folderGuids, null); // Copy from folder in packageOne.
                packageNew.PerformCopy(m_ProgressMonitorStack, copyLink);
                sessionCount = RepositorySessionCount(packageNew);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageNew, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                sessionLength = GetSessionLength(packageNew, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageNew, sessionGuid), "Session data mismatch after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageNew, sessionGuid2), "Session data mismatch after copy.");

                // Copy folder into reopened package which already contains the sessions.
                packageTwo.PerformCopy(m_ProgressMonitorStack, copyLink); // Copy into packageTwo.
                sessionCount = RepositorySessionCount(packageTwo);
                Assert.AreEqual(2, sessionCount);
                sessionLength = GetSessionLength(packageTwo, sessionGuid);
                Assert.AreEqual(expectedLength, sessionLength, "Session was not expected length after copy.");
                sessionLength = GetSessionLength(packageTwo, sessionGuid2);
                Assert.AreEqual(expectedLength2, sessionLength, "Session was not expected length after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream, packageTwo, sessionGuid), "Session data mismatch after copy.");
                Assert.IsTrue(IsSessionDataEqual(expectedStream2, packageTwo, sessionGuid2), "Session data mismatch after copy.");
                packageTwo.Save(m_ProgressMonitorStack);

                Guid[] allSessions = GetSessionIds(rowCollection);
                RepositoryFolder recentFolder = packageOne.Folders.Add("Recent", RepositoryFolderType.Manual);
                copyLink = startPackage.CreateCopyLink(null, allSessions);
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink, recentFolder);
                sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(9, sessionCount);

                packageNew.MergeFile(m_ProgressMonitorStack, StockPackageLocation2);
                sessionCount = RepositorySessionCount(packageNew);
                Assert.AreEqual(5, sessionCount); // 2 + 5 but - 2 in common. (used to be 1 in common for 6)

                DataSet dataSet2 = packageNew.GetSessions();
                DataRowCollection rowCollection2 = dataSet2.Tables[0].Rows;
                Assert.Greater(rowCollection2.Count, 0, "Package contains no sessions.");
                Guid[] allSessions2 = GetSessionIds(rowCollection2);
                RepositoryFolder interestingFolder = packageOne.Folders.Add("Interesting", RepositoryFolderType.Manual);
                copyLink = packageNew.CreateCopyLink(null, allSessions2);
                packageOne.PerformCopy(m_ProgressMonitorStack, copyLink, interestingFolder);
                sessionCount = RepositorySessionCount(packageOne);
                Assert.AreEqual(11, sessionCount);

                // TODO: more checking?
            }
            finally
            {
                if (expectedStream != null)
                    expectedStream.Dispose();
                
                if (expectedStream2 != null)
                    expectedStream2.Dispose();
                
                if (startPackage != null)
                    RepositoryManager.Repositories.Remove(startPackage);

                if (packageOne != null)
                    RepositoryManager.Repositories.Remove(packageOne);
                
                if (packageTwo != null)
                    RepositoryManager.Repositories.Remove(packageTwo);

                if (packageNew != null)
                    RepositoryManager.Repositories.Remove(packageNew);

                File.Delete(tempOne);
                File.Delete(tempTwo);
            }
        }

        //[Test]
        //[ExpectedException(typeof(KeyNotFoundException))]
        //public void SessionDoesNotExist()
        //{
        //    Guid bogusSessionId = Guid.NewGuid();

        //    Repository localRepository = RepositoryManager.Collection;
        //    DateTimeOffset startDateTime, endDateTime, addedDateTime, updatedDateTime;

        //    bool foundSession = localRepository.Index.SessionTimesGet(bogusSessionId, out startDateTime, out endDateTime, out addedDateTime, out updatedDateTime);

        //    Assert.IsFalse(foundSession);

        //    Session badSession = localRepository.GetSession(bogusSessionId);
        //}

        [OneTimeTearDown]
        public void Cleanup()
        {
            if ((string.IsNullOrEmpty(m_OutputFileNamePath) == false)
                && (File.Exists(m_OutputFileNamePath)))
            {
                File.Delete(m_OutputFileNamePath);
            }

            if (m_TopProgressMonitor != null)
            {
                m_TopProgressMonitor.Dispose();
                m_TopProgressMonitor = null;
            }
            if (m_ProgressMonitorStack != null)
            {
                m_ProgressMonitorStack.Dispose();
                m_ProgressMonitorStack = null;
            }
        }

        private static int RepositorySessionCount(IPersistentRepository repository)
        {
            int sessionCount, problemCount, fileCount;
            long fileBytes;
            repository.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);

            return sessionCount;
        }

        private static int FolderSessionCount(RepositoryFolder folder)
        {
            int sessionCount, problemCount, fileCount;
            long fileBytes;
            folder.GetStats(out sessionCount, out problemCount, out fileCount, out fileBytes);

            return sessionCount;
        }

        private static long GetSessionLength(IPersistentRepository repository, Guid sessionId)
        {
            using (Stream sessionStream = repository.GetSessionStream(sessionId))
            {
                long length = sessionStream.Length;
                return length;
            }
        }

        private static bool IsSessionDataEqual(Stream expectedStream, IPersistentRepository repository, Guid sessionId)
        {
            long originalPosition = expectedStream.Position;
            bool equal = true;
            try
            {
                expectedStream.Position = 0;
                using (Stream comparisonStream = repository.GetSessionStream(sessionId))
                {
                    comparisonStream.Position = 0; // Just to make sure.

                    const int BufferSize = 4096;
                    byte[] sourceBytes = new byte[BufferSize];
                    byte[] targetBytes = new byte[BufferSize];
                    int sourceBytesRead, targetBytesRead;
                    while ((sourceBytesRead = expectedStream.Read(sourceBytes, 0, BufferSize)) != 0)
                    {
                        targetBytesRead = comparisonStream.Read(targetBytes, 0, BufferSize);

                        //Assert.AreEqual(sourceBytesRead, targetBytesRead, "The Source and Target bytes read don't have the same count, so the buffers can't possibly be the same");
                        //Assert.IsTrue(ArrayCompare(sourceBytes, targetBytes), "The source and target buffers are not the same.");
                        if (sourceBytesRead != targetBytesRead)
                            equal = false;
                        else if (ArrayCompare(sourceBytes, targetBytes) == false)
                            equal = false;

                        if (equal == false)
                            break;
                    }
                }
            }
            finally
            {
                expectedStream.Position = originalPosition;
            }

            return equal;
        }

        private static bool ArrayCompare<T>(T[] original, T[] target)
        {
            if ((original == null) && (target == null))
                return true;
            if ((original == null) || (target == null))
                return false;
            if (original.Length != target.Length)
                return false;

            for (int curIndex = 0; curIndex < original.Length; curIndex++)
            {
                if (original[curIndex].Equals(target[curIndex]) == false)
                    return false;
            }

            return true;
        }

        private static Guid[] GetSessionIds(DataRowCollection rowCollection)
        {
            List<Guid> rowList = new List<Guid>();
            foreach (DataRow row in rowCollection)
            {
                Guid sessionId = (Guid)row["Session_Id"];
                rowList.Add(sessionId);
            }

            return rowList.ToArray();
        }
    }
}
