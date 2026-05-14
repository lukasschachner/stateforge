using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public class HistoryRestoreTests
{
    [Fact]
    public async Task ReEnteringHistoryCompositeRestoresLastActiveChild()
    {
        var runtime = HistoryTestDomain.CreateOperationalMachine().CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        await runtime.ApplyAsync(new Start());
        await runtime.ApplyAsync(new Pause());
        var resumed = await runtime.ApplyAsync(new Resume());

        Assert.True(resumed.IsSuccess);
        Assert.Equal(HistoryState.Processing, runtime.CurrentState);
        HistoryAssertions.ActivePathIs(resumed.ActiveStatePath, HistoryState.Operational, HistoryState.Processing);
        HistoryAssertions.SnapshotRecorded(runtime.HistorySnapshots, HistoryState.Operational, HistoryState.Processing);
    }
}