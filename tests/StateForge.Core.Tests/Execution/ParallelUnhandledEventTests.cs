using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelUnhandledEventTests
{
    [Fact]
    public async Task Unhandled_event_does_not_change_parallel_shape()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinition().CreateRuntime(ParallelState.Operational);
        var outcome = await runtime.ApplyAsync(ParallelEvent.CompleteBilling);
        Assert.False(outcome.Committed);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.WaitingForPick);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);
    }
}