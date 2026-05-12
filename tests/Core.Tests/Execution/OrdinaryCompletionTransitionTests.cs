using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class OrdinaryCompletionTransitionTests
{
    [Fact]
    public async Task Ordinary_composite_completion_transition_runs_after_terminal_child_entry()
    {
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();
        var runtime = definition.CreateRuntime(CompletionState.Reviewing);

        var outcome = await runtime.ApplyAsync(CompletionEvent.Approve);

        Assert.True(outcome.IsSuccess);
        Assert.True(outcome.Transition?.IsCompletionTriggered);
        Assert.Equal(CompletionState.Approved, runtime.CurrentState);
        Assert.Equal(CompletionState.Approved, outcome.ResultingState);
    }
}
