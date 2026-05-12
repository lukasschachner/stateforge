using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CoreHierarchySampleTests
{
    [Fact]
    public void CoreHierarchySampleRuns()
    {
        var output = CommandRunner.Run("dotnet",
            "run --project samples/Core.HierarchySample/Core.HierarchySample.csproj --configuration Release");

        Assert.Contains("Active path: Reviewing -> AuthorReview", output);
        Assert.Contains("History restored path: Reviewing -> LegalReview", output);
        Assert.Contains("Active snapshot:", output);
        Assert.Contains("Snapshot restored leaf:", output);
        Assert.Contains("Parallel regions:", output);
        Assert.Contains("Parallel active snapshot:", output);
        Assert.Contains("Parallel history restored regions:", output);
        Assert.Contains("Parallel history snapshots:", output);
    }
}