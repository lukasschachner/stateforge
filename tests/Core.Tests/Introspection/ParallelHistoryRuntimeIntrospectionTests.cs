using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelHistoryRuntimeIntrospectionTests
{
    [Fact]
    public async Task Runtime_exposes_recorded_parallel_history_separately_from_active_shape()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        var snapshot = runtime.ParallelHistorySnapshots.AssertParallelHistorySnapshot(ParallelHistoryState.Operational);

        Assert.Equal(HistoryMode.Shallow, snapshot.HistoryMode);
        var entry = Assert.Single(snapshot.RegionEntries);
        Assert.Equal("Fulfillment", entry.RegionName);
        Assert.Equal(ParallelHistoryState.Packing, entry.LastActiveLeafState);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelHistoryState.WaitingForPayment);
    }
}