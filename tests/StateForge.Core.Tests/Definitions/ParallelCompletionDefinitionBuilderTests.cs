using StateForge.Core.Definitions;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Definitions;

public sealed class ParallelCompletionDefinitionBuilderTests
{
    [Fact]
    public void Parallel_composite_OnCompletion_declares_completion_transition_and_remains_chainable()
    {
        var definition = ParallelCompletionTestFixtures.CreateOperationalDefinition();

        var transition = Assert.Single(definition.CompletionTransitions);
        Assert.Equal(CompletionState.Operational, transition.SourceState);
        Assert.Equal(CompletionState.ReadyToClose, transition.TargetState);
        Assert.True(definition.IsParallelComposite(CompletionState.Operational));
    }
}
