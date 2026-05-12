using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelHistoryShallowRestoreTests
{
    [Fact]
    public async Task Reentering_shallow_history_parallel_composite_restores_recorded_regions()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.PaymentStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        var outcome = await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        Assert.True(outcome.IsSuccess);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelHistoryState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelHistoryState.CapturingPayment);
    }
}