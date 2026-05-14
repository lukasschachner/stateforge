using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelInitialEntryTests
{
    [Fact]
    public void Runtime_enters_one_initial_leaf_per_region_in_order()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinition().CreateRuntime(ParallelState.Operational);

        Assert.True(runtime.ActiveStateShape.IsParallel);
        Assert.Equal(new[] { "Fulfillment", "Billing" },
            runtime.ActiveStateShape.ActiveRegions.Select(r => r.RegionName));
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.WaitingForPick);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);
    }
}