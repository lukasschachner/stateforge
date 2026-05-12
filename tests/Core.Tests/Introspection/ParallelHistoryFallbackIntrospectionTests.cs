using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelHistoryFallbackIntrospectionTests
{
    [Fact]
    public async Task Snapshot_reports_incomplete_shape_when_region_history_is_missing()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        var snapshot = runtime.ParallelHistorySnapshots.AssertParallelHistorySnapshot(ParallelHistoryState.Operational);

        Assert.False(snapshot.HasCompleteRecordedShape);
        Assert.Equal("Fulfillment", Assert.Single(snapshot.RegionEntries).RegionName);
    }
}