using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelHistoryActiveShapeDistinctionTests
{
    [Fact]
    public async Task Recorded_history_remains_distinct_from_current_active_shape_after_region_changes()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        var snapshot = runtime.ParallelHistorySnapshots.AssertParallelHistorySnapshot(ParallelHistoryState.Operational);

        Assert.NotSame(runtime.ActiveStateShape.ActiveRegions, snapshot.RegionEntries);
        Assert.Equal(ParallelHistoryState.Packing, snapshot.RegionEntries.Single().LastActiveLeafState);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelHistoryState.WaitingForPayment);
    }
}