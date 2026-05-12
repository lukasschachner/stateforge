using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PublicApiSnapshotGeneratorTests
{
    [Fact]
    public void SnapshotGenerationIsDeterministic()
    {
        var assembly = PackableProject.All.Single(p => p.Name == "Core").MarkerType.Assembly;
        Assert.Equal(PublicApiSnapshotGenerator.Generate(assembly), PublicApiSnapshotGenerator.Generate(assembly));
    }

    [Fact]
    public void SnapshotExcludesInternalImplementationDetails()
    {
        var snapshot =
            PublicApiSnapshotGenerator.Generate(PackableProject.All.Single(p => p.Name == "Core").MarkerType.Assembly);
        Assert.DoesNotContain("private", snapshot, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StateMachineLibrary.Core.Execution.TransitionExecutor", snapshot,
            StringComparison.Ordinal);
    }
}