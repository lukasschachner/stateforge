using StateForge.Core.Definitions;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Execution;

public sealed class CompletionEpisodeTests
{
    [Fact]
    public async Task No_eligible_completion_episode_does_not_re_evaluate_guards_on_unrelated_events()
    {
        var guardCount = 0;
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing).InitialChild(CompletionState.AuthorReview)
                .OnCompletion()
                .When(_ =>
                {
                    guardCount++;
                    return false;
                }, "never")
                .GoTo(CompletionState.Approved);
            builder.State(CompletionState.AuthorReview).ChildOf(CompletionState.Reviewing)
                .On(CompletionEvent.Approve).GoTo(CompletionState.ReviewDone);
            builder.State(CompletionState.ReviewDone).ChildOf(CompletionState.Reviewing).Terminal();
            builder.State(CompletionState.Approved);
        });
        var runtime = definition.CreateRuntime(CompletionState.Reviewing);

        await runtime.ApplyAsync(CompletionEvent.Approve);
        await runtime.ApplyAsync(CompletionEvent.Other);

        Assert.Equal(1, guardCount);
        Assert.Equal(CompletionState.ReviewDone, runtime.CurrentState);
    }
}
