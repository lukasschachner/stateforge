using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Definitions;

public sealed class ParallelRegionDefinitionBuilderTests
{
    [Fact]
    public void Builder_declares_parallel_composite_regions_and_membership()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinition();

        Assert.True(definition.IsParallelComposite(ParallelState.Operational));
        Assert.Equal(new[] { "Fulfillment", "Billing" },
            definition.GetParallelRegions(ParallelState.Operational).Select(r => r.Name));
        Assert.True(definition.TryGetRegionMembership(ParallelState.WaitingForPick, out var membership));
        Assert.Equal("Fulfillment",
            definition.GetParallelRegions(ParallelState.Operational).Single(r => r.RegionId == membership.RegionId)
                .Name);
    }
}