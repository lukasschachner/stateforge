using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Introspection;

public class DefinitionIntrospectionActionTests
{
    [Fact]
    public void IntrospectionExposesActionSummariesWithoutExecutingActions()
    {
        var calls = 0;
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => calls++, "exit one")
                .On<Actions.Pay>()
                .Execute(_ => calls++, "transition one")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ => calls++, "entry one");
        });

        var introspection = definition.Introspect();

        Assert.Equal(0, calls);
        Assert.Equal("exit one",
            introspection.DeclaredStates.Single(s => s.Value == ActionState.Created).ExitActions.Single().Summary
                .DisplayName);
        Assert.Equal("entry one",
            introspection.DeclaredStates.Single(s => s.Value == ActionState.Paid).EntryActions.Single().Summary
                .DisplayName);
        Assert.Equal("transition one",
            introspection.DeclaredTransitions.Single().TransitionActions.Single().Summary.DisplayName);
    }
}