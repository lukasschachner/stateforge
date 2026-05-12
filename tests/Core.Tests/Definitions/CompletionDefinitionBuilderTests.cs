using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Definitions;

public sealed class CompletionDefinitionBuilderTests
{
    [Fact]
    public void State_scoped_OnCompletion_declares_completion_transition_without_declaring_event()
    {
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();

        var transition = Assert.Single(definition.CompletionTransitions);
        Assert.Equal(CompletionState.Reviewing, transition.SourceState);
        Assert.Equal(CompletionState.Approved, transition.TargetState);
        Assert.Equal(TransitionTriggerKind.Completion, transition.TriggerKind);
        Assert.DoesNotContain(definition.Events, e => e.Identity == "completion");
    }
}
