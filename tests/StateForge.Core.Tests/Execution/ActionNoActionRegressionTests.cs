using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class ActionNoActionRegressionTests
{
    [Fact]
    public async Task SuppliedStateNoActionTransitionStillSucceeds()
    {
        var definition = CreateNoActionDefinition();

        var outcome = await definition.ApplyAsync(ActionState.Created, new Actions.Pay());

        Assert.True(outcome.IsSuccess);
        Assert.True(outcome.Committed);
        Assert.Equal(ActionState.Paid, outcome.ResultingState);
    }

    [Fact]
    public async Task RuntimeNoActionTransitionStillUpdatesCurrentState()
    {
        var runtime = CreateNoActionDefinition().CreateRuntime(ActionState.Created);

        var outcome = await runtime.ApplyAsync(new Actions.Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(ActionState.Paid, runtime.CurrentState);
    }

    private static StateMachineDefinition<ActionState, ActionEvent> CreateNoActionDefinition()
    {
        return StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created).On<Actions.Pay>().GoTo(ActionState.Paid);
            builder.State(ActionState.Paid);
        });
    }
}