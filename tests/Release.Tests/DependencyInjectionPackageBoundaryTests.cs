using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class DependencyInjectionPackageBoundaryTests
{
    [Fact]
    public void DependencyInjectionPackageBoundaryIsDeclared()
    {
        var rules = PackageBoundaryRules.Load();
        Assert.Contains("StateForge.DependencyInjection", rules.Keys);
    }
}
