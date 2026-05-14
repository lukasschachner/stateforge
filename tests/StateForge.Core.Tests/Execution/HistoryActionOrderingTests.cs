using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public class HistoryActionOrderingTests
{
    [Fact]
    public async Task HistoryReEntryUsesNormalExitTransitionEntryOrdering()
    {
        var log = new List<string>();
        var runtime = HistoryTestDomain.CreateOperationalMachine(log).CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        await runtime.ApplyAsync(new Start());
        log.Clear();

        await runtime.ApplyAsync(new Pause());
        await runtime.ApplyAsync(new Resume());

        Assert.Equal(["exit Processing", "entry Processing"], log);
    }
}