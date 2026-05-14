using Release.Tests.TestSupport;
using StateForge.Core.Definitions;

namespace Release.Tests;

public sealed class CorePublicApiSnapshotTests
{
    [Fact]
    public void CorePublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "Core"));
    }

    [Fact]
    public async Task HierarchyApisRemainOptInForFlatDefinitions()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("B");
            builder.State("B");
        });

        var runtime = definition.CreateRuntime("A");
        var outcome = await runtime.ApplyAsync("go");

        Assert.False(definition.HasHierarchy);
        Assert.Equal("B", outcome.ActiveLeafState);
        Assert.Equal(["B"], outcome.ActiveStatePath.StatesRootToLeaf);
    }
}