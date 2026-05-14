using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Completion;

internal static class OrdinaryCompletionTestFixtures
{
    public static StateMachineDefinition<CompletionState, CompletionEvent> CreateReviewingDefinition(
        Action<StateMachineDefinitionBuilder<CompletionState, CompletionEvent>>? configure = null)
    {
        return StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing)
                .InitialChild(CompletionState.AuthorReview)
                .OnCompletion()
                .GoTo(CompletionState.Approved);

            builder.State(CompletionState.AuthorReview)
                .ChildOf(CompletionState.Reviewing)
                .On(CompletionEvent.Approve)
                .GoTo(CompletionState.ReviewDone);

            builder.State(CompletionState.ReviewDone)
                .ChildOf(CompletionState.Reviewing)
                .Terminal();

            builder.State(CompletionState.Approved);
            configure?.Invoke(builder);
        });
    }
}
