using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelHistoryFallbackTests
{
    [Fact]
    public async Task Missing_region_history_falls_back_to_region_initial_state()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelHistoryState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelHistoryState.WaitingForPayment);
    }
}