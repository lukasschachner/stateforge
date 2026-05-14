using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class RuntimeReuseTests
{
    [Fact]
    public async Task OneDefinitionSupportsTenIndependentStateOwningRuntimes()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid).On<Ship>().GoTo(OrderState.Shipped);
            builder.State(OrderState.Shipped);
        });
        var runtimes = Enumerable.Range(0, 10).Select(_ => definition.CreateRuntime(OrderState.Created)).ToArray();

        await runtimes[0].ApplyAsync(new Pay());
        await runtimes[1].ApplyAsync(new Pay());
        await runtimes[1].ApplyAsync(new Ship());

        Assert.Equal(OrderState.Paid, runtimes[0].CurrentState);
        Assert.Equal(OrderState.Shipped, runtimes[1].CurrentState);
        Assert.All(runtimes.Skip(2), r => Assert.Equal(OrderState.Created, r.CurrentState));
    }
}