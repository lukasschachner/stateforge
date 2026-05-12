using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class CompletionConflictDiagnosticTests
{
    [Fact]
    public async Task Runtime_guarded_completion_ambiguity_returns_completion_conflict_before_commit()
    {
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing).InitialChild(CompletionState.AuthorReview)
                .OnCompletion().When(_ => true, "ready").GoTo(CompletionState.Approved)
                .OnCompletion().When(_ => true, "also ready").GoTo(CompletionState.Escalated);
            builder.State(CompletionState.AuthorReview).ChildOf(CompletionState.Reviewing)
                .On(CompletionEvent.Approve).GoTo(CompletionState.ReviewDone);
            builder.State(CompletionState.ReviewDone).ChildOf(CompletionState.Reviewing).Terminal();
            builder.State(CompletionState.Approved);
            builder.State(CompletionState.Escalated);
        });
        var runtime = definition.CreateRuntime(CompletionState.Reviewing);

        var outcome = await runtime.ApplyAsync(CompletionEvent.Approve);

        Assert.False(outcome.Committed);
        Assert.Equal(CompletionState.ReviewDone, runtime.CurrentState);
        var conflict = Assert.Single(outcome.Diagnostics.ConflictDiagnostics);
        Assert.Equal(TransitionConflictKind.CompletionConflict, conflict.Kind);
        Assert.Equal(CompletionState.Reviewing, conflict.CompletionScope);
    }
}
