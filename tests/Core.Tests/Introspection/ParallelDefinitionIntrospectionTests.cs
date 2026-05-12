using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelDefinitionIntrospectionTests
{
    [Fact]
    public void Introspection_exposes_parallel_regions_and_membership()
    {
        var introspection = ParallelGraphTestData.CreateTwoRegionDefinition().Introspect();
        Assert.True(introspection.HasParallelRegions);
        Assert.Equal(2, introspection.RegionsOf(ParallelState.Operational).Count);
        Assert.True(introspection.TryGetRegionMembership(ParallelState.WaitingForPick, out var membership));
        Assert.Contains("Fulfillment",
            introspection.ParallelRegions.Where(r => r.RegionId == membership.RegionId).Select(r => r.Name));
    }
}