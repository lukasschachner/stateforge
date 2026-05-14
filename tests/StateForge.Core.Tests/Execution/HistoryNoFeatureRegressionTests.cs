using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HistoryNoFeatureRegressionTests
{
    [Fact]
    public async Task FlatMachinesDoNotCreateHistorySnapshots()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });
        var runtime = definition.CreateRuntime(OrderState.Created);

        var outcome = await runtime.ApplyAsync(new Pay());

        Assert.False(definition.HasHistory);
        Assert.Empty(runtime.HistorySnapshots);
        Assert.Empty(outcome.HistorySnapshots);
        Assert.Equal(OrderState.Paid, runtime.CurrentState);
    }
}