using Core.Tests.History;

namespace Core.Tests.Execution;

public class HistoryTerminalInteractionTests
{
    [Fact]
    public async Task ReEnteringHistoryCompositeCanRestoreTerminalChild()
    {
        var runtime = HistoryTestDomain.CreateOperationalMachine().CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        await runtime.ApplyAsync(new Start());
        await runtime.ApplyAsync(new Finish());
        await runtime.ApplyAsync(new Pause());
        var resumed = await runtime.ApplyAsync(new Resume());

        Assert.True(resumed.IsSuccess);
        Assert.Equal(HistoryState.Done, runtime.CurrentState);
        HistoryAssertions.ActivePathIs(resumed.ActiveStatePath, HistoryState.Operational, HistoryState.Done);
    }
}