using StateForge.Core.Tests.History;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HistoryFallbackTests
{
    [Fact]
    public async Task FirstEntryUsesInitialChildFallbackWhenNoRecordExists()
    {
        var runtime = HistoryTestDomain.CreateOperationalMachine().CreateRuntime(HistoryState.Offline);

        var outcome = await runtime.ApplyAsync(new Resume());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HistoryState.Idle, runtime.CurrentState);
        HistoryAssertions.ActivePathIs(outcome.ActiveStatePath, HistoryState.Operational, HistoryState.Idle);
    }

    [Fact]
    public async Task FirstEntryUsesExplicitFallbackWhenConfigured()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Offline).On<Resume>().GoTo(HistoryState.Operational);
            builder.State(HistoryState.Operational).InitialChild(HistoryState.Idle)
                .WithShallowHistory(HistoryState.Processing);
            builder.State(HistoryState.Processing).ChildOf(HistoryState.Operational);
        });
        var runtime = definition.CreateRuntime(HistoryState.Offline);

        var outcome = await runtime.ApplyAsync(new Resume());

        Assert.Equal(HistoryState.Processing, outcome.ResultingState);
    }
}