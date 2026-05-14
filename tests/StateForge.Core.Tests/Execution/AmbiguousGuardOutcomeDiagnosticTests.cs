using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Execution;

public sealed class AmbiguousGuardOutcomeDiagnosticTests
{
    [Fact]
    public async Task Multiple_enabled_guarded_completion_candidates_report_structured_completion_conflict()
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
        var conflict = Assert.Single(outcome.Diagnostics.ConflictDiagnostics);
        Assert.Equal(TransitionConflictKind.CompletionConflict, conflict.Kind);
        Assert.Equal(CompletionState.Reviewing, conflict.CompletionScope);
        Assert.Equal(2, conflict.Participants.Count);
    }
}
