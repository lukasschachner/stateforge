using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelHistoryNestedHistoryTests
{
    [Fact]
    public async Task Ordinary_nested_history_still_restores_independently_of_parallel_history()
    {
        var runtime = HistoryTestDomain.CreateOperationalMachine().CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        await runtime.ApplyAsync(new Start());
        await runtime.ApplyAsync(new Pause());
        var resumed = await runtime.ApplyAsync(new Resume());

        Assert.True(resumed.IsSuccess);
        Assert.Equal(HistoryState.Processing, runtime.CurrentState);
    }

    [Fact]
    public async Task Parallel_history_snapshot_records_region_scope_only()
    {
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Deep)
            .CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        var snapshot = runtime.ParallelHistorySnapshots.AssertParallelHistorySnapshot(ParallelHistoryState.Operational);

        var entry = Assert.Single(snapshot.RegionEntries);
        Assert.Equal("Fulfillment", entry.RegionName);
        Assert.Equal([ParallelHistoryState.Operational, ParallelHistoryState.Packing],
            entry.LastActivePath.StatesRootToLeaf);
    }
}