using Gibraltar.Agent;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Log.Initializing += Log_Initializing;

            Log.StartSession("Start HTTP Module Test Suite");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Log.EndSession("Test suite complete.");
        }

        void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            var publisherConfig = e.Configuration.Publisher;
            publisherConfig.ApplicationDescription = "HTTp Module Unit Tests";
            publisherConfig.ApplicationName = "Unit Tests";
            publisherConfig.ProductName = "Loupe.Agent.Web.Module";
            publisherConfig.EnvironmentName = "Development";
            
        }
    }
}