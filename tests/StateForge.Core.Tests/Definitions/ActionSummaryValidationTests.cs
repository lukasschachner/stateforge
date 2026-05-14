using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Definitions;

public class ActionSummaryValidationTests
{
    [Fact]
    public void EmptyActionDisplayNameIsNormalizedDeterministically()
    {
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => { }, "   ")
                .On<Actions.Pay>()
                .Execute(_ => { }, "   ")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid).OnEntry(_ => { }, "   ");
        });

        Assert.Equal("Exit action 1", definition.FindState(ActionState.Created)!.ExitActions.Single().DisplayName);
        Assert.Equal("Entry action 1", definition.FindState(ActionState.Paid)!.EntryActions.Single().DisplayName);
        Assert.Equal("Transition action 1", definition.Transitions.Single().TransitionActions.Single().DisplayName);
        Assert.True(definition.Validate().IsValid);
    }
}