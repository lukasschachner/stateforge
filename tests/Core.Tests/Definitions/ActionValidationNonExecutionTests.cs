using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Definitions;

public class ActionValidationNonExecutionTests
{
    [Fact]
    public void ValidationDoesNotExecuteThrowingActions()
    {
        var calls = 0;
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ =>
                {
                    calls++;
                    throw new InvalidOperationException("exit");
                })
                .On<Actions.Pay>()
                .Execute(_ =>
                {
                    calls++;
                    throw new InvalidOperationException("transition");
                })
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ =>
                {
                    calls++;
                    throw new InvalidOperationException("entry");
                });
        });

        var validation = definition.Validate();

        Assert.True(validation.IsValid);
        Assert.Equal(0, calls);
    }
}