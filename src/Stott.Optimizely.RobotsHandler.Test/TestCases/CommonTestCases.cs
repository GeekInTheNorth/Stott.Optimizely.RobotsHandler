using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Stott.Optimizely.RobotsHandler.Test.TestCases
{
    public static class CommonTestCases
    {
        public static IEnumerable<TestCaseData> InvalidGuidStrings
        {
            get
            {
                yield return new TestCaseData((string)null);
                yield return new TestCaseData(string.Empty);
                yield return new TestCaseData(" ");
                yield return new TestCaseData("Not-A-Guid");
                yield return new TestCaseData(Guid.Empty.ToString());
            }
        }
    }
}
