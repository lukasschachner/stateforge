using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelCompletionTransitionTests
{
    [Fact]
    public async Task Parallel_completion_waits_until_all_regions_are_terminal()
    {
        var definition = ParallelCompletionTestFixtures.CreateOperationalDefinition();
        var runtime = definition.CreateRuntime(CompletionState.Operational);

        await runtime.ApplyAsync(CompletionEvent.Pick);
        Assert.True(runtime.ActiveStateShape.IsParallel);

        var outcome = await runtime.ApplyAsync(CompletionEvent.Pay);

        Assert.True(outcome.IsSuccess);
        Assert.True(outcome.Transition?.IsCompletionTriggered);
        Assert.False(runtime.ActiveStateShape.IsParallel);
        Assert.Equal(CompletionState.ReadyToClose, runtime.CurrentState);
    }
}
