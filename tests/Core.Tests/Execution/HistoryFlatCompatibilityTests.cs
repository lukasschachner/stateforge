using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Execution;

public class HistoryFlatCompatibilityTests
{
    [Fact]
    public async Task MachinesWithoutHistoryKeepExistingRuntimeBehavior()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(OrderState.Paid, outcome.ResultingState);
        Assert.False(definition.HasHistory);
    }
}