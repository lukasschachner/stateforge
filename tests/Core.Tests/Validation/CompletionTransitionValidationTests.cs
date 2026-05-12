using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Tests.Completion;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class CompletionTransitionValidationTests
{
    [Fact]
    public void Completion_from_leaf_state_is_invalid()
    {
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Leaf).OnCompletion().GoTo(CompletionState.Approved);
            builder.State(CompletionState.Approved);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == CompletionTransitionValidationCodes.InvalidSource);
    }

    [Fact]
    public void Multiple_unguarded_completion_transitions_from_same_scope_are_invalid()
    {
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing).InitialChild(CompletionState.AuthorReview)
                .OnCompletion().GoTo(CompletionState.Approved)
                .OnCompletion().GoTo(CompletionState.Escalated);
            builder.State(CompletionState.AuthorReview).ChildOf(CompletionState.Reviewing).Terminal();
            builder.State(CompletionState.Approved);
            builder.State(CompletionState.Escalated);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == CompletionTransitionValidationCodes.AmbiguousUnguarded);
        var conflict = Assert.Single(validation.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.CompletionConflict);
        Assert.Equal(CompletionState.Reviewing, conflict.CompletionScope);
        Assert.Equal(2, conflict.Participants.Count);
    }
}
