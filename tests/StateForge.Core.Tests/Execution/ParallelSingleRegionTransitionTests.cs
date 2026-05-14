using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelSingleRegionTransitionTests
{
    [Fact]
    public async Task Event_advances_matching_region_and_preserves_unaffected_region()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinition().CreateRuntime(ParallelState.Operational);

        var outcome = await runtime.ApplyAsync(ParallelEvent.PickStarted);

        Assert.True(outcome.IsSuccess);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);
    }
}