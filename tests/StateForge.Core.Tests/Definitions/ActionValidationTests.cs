using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Definitions;

public class ActionValidationTests
{
    [Fact]
    public void BuilderRejectsNullStateAction()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
                builder.State(ActionState.Created).OnExit(null!)));

        Assert.Equal("action", ex.ParamName);
    }

    [Fact]
    public void BuilderRejectsNullTransitionAction()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
                builder.State(ActionState.Created).On<Actions.Pay>().Execute(null!)));

        Assert.Equal("behavior", ex.ParamName);
    }

    [Fact]
    public void ValidActionsPassValidation()
    {
        var definition = ActionTestDomain.CreateWithOrderedActions(new List<string>());

        Assert.True(definition.Validate().IsValid);
    }
}