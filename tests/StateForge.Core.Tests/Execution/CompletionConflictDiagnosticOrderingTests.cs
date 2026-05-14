using StateForge.Core.Definitions;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Execution;

public sealed class CompletionConflictDiagnosticOrderingTests
{
    [Fact]
    public async Task Completion_conflict_participant_order_is_stable()
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

        string[]? expected = null;
        for (var i = 0; i < 10; i++)
        {
            var outcome = await definition.CreateRuntime(CompletionState.Reviewing).ApplyAsync(CompletionEvent.Approve);
            var actual = outcome.Diagnostics.ConflictDiagnostics.Single().Participants
                .Select(participant => $"{participant.TransitionId}:{participant.TargetState}")
                .ToArray();
            expected ??= actual;
            Assert.Equal(expected, actual);
        }
    }
}
