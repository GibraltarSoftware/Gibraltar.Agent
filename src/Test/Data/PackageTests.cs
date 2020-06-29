using System.IO;
using Gibraltar.Data;
using Gibraltar.Monitor;
using NUnit.Framework;

namespace Gibraltar.Test.Data
{
    [TestFixture]
    public class PackageTests
    {

        [Test]
        public void SimplePackageImport()
        {
            string tempPackageFileNamePath = Path.GetTempFileName();
            File.Delete(tempPackageFileNamePath); //get temp file name creates the file as part of allocating the name.

            tempPackageFileNamePath = Path.GetTempFileName() + "." + Log.PackageExtension;

            //create an all sessions simple package
            using (Packager newPackager = new Packager())
            {
                newPackager.SendToFile(SessionCriteria.AllSessions, false, tempPackageFileNamePath, false);               
            }

            //open the original package to check its statistics
            int sessions;
            int problemSessions;
            int files;
            long fileBytes;
            using (SimplePackage newPackage = new SimplePackage(tempPackageFileNamePath))
            {
                newPackage.GetStats(out sessions, out problemSessions, out files, out fileBytes);
            }
            Assert.Greater(sessions, 0, "No sessions were found in the session that was created, the test is invalid.");
            Assert.Greater(files, 0, "No session files were found in the session that was created, the test is invalid.");
            Assert.Greater(fileBytes, 0, "No session file bytes were found in the session that was created, the test is invalid.");


            int compareProblemSessions;
            int compareSessions;
            int compareFiles;
            long compareFileBytes;
            using (Package newPackage = new Package(tempPackageFileNamePath))
            {
                newPackage.GetStats(out compareSessions, out compareProblemSessions, out compareFiles, out compareFileBytes);
            }

            //now check that they are generally the same.  Bytes may not be.
            Assert.AreEqual(sessions, compareSessions);
            Assert.AreEqual(problemSessions, compareProblemSessions);
            Assert.AreEqual(files, compareFiles);

            File.Delete(tempPackageFileNamePath);
        }
    }
}
