using System;
using System.IO;
using Gibraltar.Agent.Configuration;
using NUnit.Framework;

namespace Gibraltar.Agent.Test.Configuration
{
    [TestFixture]
    public class AgentConfigurationFile
    {
        [Test]
        public void CreateConfigurationFile()
        {
            var newConfiguration = new AgentConfiguration();

            newConfiguration.Publisher.ProductName = "Unit Test Product Name";
            newConfiguration.Publisher.ApplicationName = "Unit Test Application Name";
            newConfiguration.Publisher.ApplicationVersion = new Version(3, 4, 5, 6);
            newConfiguration.Server.AutoSendSessions = true;
            newConfiguration.Server.Repository = "ConfigurationTest";
            newConfiguration.Server.SendAllApplications = true;
            newConfiguration.Server.Server = "loupe.gibraltarsoftware.com";

            var testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AgentConfigurationFile_CreateConfigurationFile.config");
            try
            {
                File.Delete(testFile);

                newConfiguration.Save(testFile);
                Assert.IsTrue(File.Exists(testFile));

                var persistedConfiguration = new AgentConfiguration(testFile);
                Assert.AreEqual(newConfiguration.Publisher.ProductName, persistedConfiguration.Publisher.ProductName);
                Assert.AreEqual(newConfiguration.Publisher.ApplicationName,
                    persistedConfiguration.Publisher.ApplicationName);
                Assert.AreEqual(newConfiguration.Publisher.ApplicationVersion,
                    persistedConfiguration.Publisher.ApplicationVersion);
                Assert.AreEqual(newConfiguration.Server.AutoSendSessions,
                    persistedConfiguration.Server.AutoSendSessions);
                Assert.AreEqual(newConfiguration.Server.Repository, persistedConfiguration.Server.Repository);
                Assert.AreEqual(newConfiguration.Server.SendAllApplications,
                    persistedConfiguration.Server.SendAllApplications);
                Assert.AreEqual(newConfiguration.Server.Server, persistedConfiguration.Server.Server);
            }
            finally
            {
                File.Delete(testFile);
            }
        }

        [Test]
        public void ConfigFileMustExist()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                var wontLoadConfiguration = new AgentConfiguration("Non-existent file name.config");
            });
        }
    }
}
