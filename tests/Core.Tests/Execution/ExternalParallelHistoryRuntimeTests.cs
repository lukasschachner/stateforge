using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Tests.History;

namespace Core.Tests.Execution;

public sealed class ExternalParallelHistoryRuntimeTests
{
    [Fact]
    public async Task External_runtime_records_parallel_history_snapshots()
    {
        var state = ParallelHistoryState.Operational;
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);
        var runtime = definition.CreateRuntime(StateAccessor.Create(() => state, next => state = next));

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);

        var snapshot = runtime.ParallelHistorySnapshots.AssertParallelHistorySnapshot(ParallelHistoryState.Operational);
        var entry = Assert.Single(snapshot.RegionEntries);
        Assert.Equal("Fulfillment", entry.RegionName);
        Assert.Equal(ParallelHistoryState.Packing, entry.LastActiveLeafState);
    }

    [Fact]
    public async Task External_runtime_restores_parallel_history_shape_on_reentry()
    {
        var state = ParallelHistoryState.Operational;
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);
        var runtime = definition.CreateRuntime(StateAccessor.Create(() => state, next => state = next));

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        var shape = await runtime.GetActiveStateShapeAsync();
        shape.AssertRegion("Fulfillment", ParallelHistoryState.Packing);
        shape.AssertRegion("Billing", ParallelHistoryState.WaitingForPayment);
    }
}