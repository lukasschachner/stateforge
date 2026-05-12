using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class DefinitionIntrospectionTests
{
    [Fact]
    public void ConditionsAreVisibleOnDeclaredTransitions()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>().When(_ => true, "visible condition").GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var transition = Assert.Single(definition.Introspect().DeclaredTransitions);
        var condition = Assert.Single(transition.Conditions);
        Assert.Equal("visible condition", condition.DisplayName);
    }
}