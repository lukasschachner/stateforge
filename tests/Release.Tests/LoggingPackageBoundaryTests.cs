using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class LoggingPackageBoundaryTests
{
    [Fact]
    public void LoggingPackageBoundaryIsDeclared()
    {
        var rules = PackageBoundaryRules.Load();
        Assert.Contains("StateForge.Logging", rules.Keys);
    }
}
