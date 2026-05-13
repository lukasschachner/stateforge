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

    [Fact]
    public void Region_scoped_initial_and_terminal_declarations_match_old_style_introspection()
    {
        var oldStyle = ParallelGraphTestData.CreateTwoRegionDefinitionOldStyle().Introspect();
        var newStyle = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle().Introspect();

        Assert.Equal(oldStyle.ParallelRegions.Select(r => (r.Name, r.InitialState)),
            newStyle.ParallelRegions.Select(r => (r.Name, r.InitialState)));
        Assert.Equal(oldStyle.ParallelRegions.Select(r => (r.Name, Terminals: string.Join(",", r.TerminalStates))),
            newStyle.ParallelRegions.Select(r => (r.Name, Terminals: string.Join(",", r.TerminalStates))));
        Assert.True(newStyle.TryGetRegionMembership(ParallelState.FulfillmentDone, out var membership));
        Assert.Contains(newStyle.ParallelRegions, r => r.RegionId == membership.RegionId && r.Name == "Fulfillment");
    }
}