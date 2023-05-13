using ExpectedObjects;
using ExpectedObjects.Strategies;
using System;

namespace Loupe.Agent.Web.Module.Tests
{
    public class DateTimeOffSetComparisonStrategy:IComparisonStrategy
    {
        public bool CanCompare(Type type)
        {
            return type == typeof(DateTimeOffset);
        }

        public bool AreEqual(object expected, object actual, IComparisonContext comparisonContext)
        {
            var expectedOffset = (DateTimeOffset) expected;
            var actualOffset = (DateTimeOffset) actual;

            if (expectedOffset.Date != actualOffset.Date)
            {
                return false;
            }

            var expectedTimeSpan = new TimeSpan(expectedOffset.Day, expectedOffset.Hour, expectedOffset.Second, expectedOffset.Millisecond);
            var actualTimeSpan = new TimeSpan(actualOffset.Day, actualOffset.Hour, actualOffset.Second, actualOffset.Millisecond);

            return expectedTimeSpan == actualTimeSpan;

        }
    }
}