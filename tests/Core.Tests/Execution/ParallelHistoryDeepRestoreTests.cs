using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelHistoryDeepRestoreTests
{
    [Fact]
    public async Task Reentering_deep_history_parallel_composite_restores_recorded_region_leaves()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Deep)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.PaymentStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelHistoryState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelHistoryState.CapturingPayment);
    }
}