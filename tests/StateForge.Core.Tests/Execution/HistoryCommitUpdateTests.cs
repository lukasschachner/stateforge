using StateForge.Core.Tests.History;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HistoryCommitUpdateTests
{
    [Fact]
    public async Task DeniedTransitionDoesNotMutateRecordedHistory()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Offline).On<Resume>().GoTo(HistoryState.Operational);
            builder.State(HistoryState.Operational).InitialChild(HistoryState.Idle).WithShallowHistory().On<Pause>()
                .GoTo(HistoryState.Suspended);
            builder.State(HistoryState.Idle).On<Start>().When(_ => false, "blocked").GoTo(HistoryState.Processing);
            builder.State(HistoryState.Processing).ChildOf(HistoryState.Operational);
            builder.State(HistoryState.Suspended).On<Resume>().GoTo(HistoryState.Operational);
        });
        var runtime = definition.CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        var denied = await runtime.ApplyAsync(new Start());
        await runtime.ApplyAsync(new Pause());
        var resumed = await runtime.ApplyAsync(new Resume());

        Assert.False(denied.Committed);
        Assert.Equal(HistoryState.Idle, resumed.ResultingState);
    }
}