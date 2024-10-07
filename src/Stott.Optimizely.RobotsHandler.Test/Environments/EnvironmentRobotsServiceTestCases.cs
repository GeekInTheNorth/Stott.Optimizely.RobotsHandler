using System.Collections.Generic;

using NUnit.Framework;

namespace Stott.Optimizely.RobotsHandler.Test.Environments;

public static class EnvironmentRobotsServiceTestCases
{
   public static IEnumerable<TestCaseData> EnvironmentNameTestCases
    {
        get
        {
            yield return new TestCaseData("Test");
            yield return new TestCaseData("Development");
            yield return new TestCaseData("Integration");
            yield return new TestCaseData("Preproduction");
            yield return new TestCaseData("Production");
        }
    }
}