using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyFlatCompatibilityTests
{
    [Fact]
    public async Task FlatDefinitionsRemainSingleStateMachinesByDefault()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.False(definition.HasHierarchy);
        Assert.True(outcome.IsSuccess);
        Assert.Equal(OrderState.Paid, outcome.ActiveLeafState);
        Assert.Equal([OrderState.Paid], outcome.ActiveStatePath.StatesRootToLeaf);
    }
}