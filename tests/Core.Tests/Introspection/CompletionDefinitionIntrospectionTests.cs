using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class CompletionDefinitionIntrospectionTests
{
    [Fact]
    public void Introspection_exposes_completion_transitions_distinctly()
    {
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();

        var completion = Assert.Single(definition.Introspect().DeclaredCompletionTransitions);

        Assert.Equal(CompletionState.Reviewing, completion.SourceState);
        Assert.DoesNotContain(definition.Introspect().DeclaredEvents, e => e.Identity == "completion");
    }
}
