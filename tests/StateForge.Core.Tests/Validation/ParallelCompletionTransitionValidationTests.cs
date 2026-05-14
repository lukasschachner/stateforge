using StateForge.Core.Definitions;
using StateForge.Core.Tests.Completion;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

public sealed class ParallelCompletionTransitionValidationTests
{
    [Fact]
    public void Parallel_completion_source_without_regions_is_invalid()
    {
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.ParallelComposite(CompletionState.Operational)
                .OnCompletion()
                .GoTo(CompletionState.ReadyToClose);
            builder.State(CompletionState.ReadyToClose);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == CompletionTransitionValidationCodes.InvalidParallelScope);
    }
}
