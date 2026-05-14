using StateForge.Core.Definitions;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Execution;

public sealed class CompletionGuardSelectionTests
{
    [Fact]
    public async Task Guarded_completion_transitions_are_selected_in_declaration_order_with_default()
    {
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing).InitialChild(CompletionState.AuthorReview)
                .OnCompletion().When(_ => false, "skip").GoTo(CompletionState.Escalated)
                .OnCompletion().GoTo(CompletionState.Approved);
            builder.State(CompletionState.AuthorReview).ChildOf(CompletionState.Reviewing)
                .On(CompletionEvent.Approve).GoTo(CompletionState.ReviewDone);
            builder.State(CompletionState.ReviewDone).ChildOf(CompletionState.Reviewing).Terminal();
            builder.State(CompletionState.Approved);
            builder.State(CompletionState.Escalated);
        });

        await definition.CreateRuntime(CompletionState.Reviewing).ApplyAsync(CompletionEvent.Approve);

        var runtime = definition.CreateRuntime(CompletionState.Reviewing);
        await runtime.ApplyAsync(CompletionEvent.Approve);
        Assert.Equal(CompletionState.Approved, runtime.CurrentState);
    }
}
