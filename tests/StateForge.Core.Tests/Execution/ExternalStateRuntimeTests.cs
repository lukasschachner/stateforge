using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public class ExternalStateRuntimeTests
{
    [Fact]
    public async Task ExternalStateRuntimeUsesApplicationOwnedAccessor()
    {
        var state = OrderState.Created;
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });
        var accessor = StateAccessor.Create(() => state, next => state = next);
        var runtime = definition.CreateRuntime(accessor, ConcurrencyMode.Serialized);

        var outcome = await runtime.ApplyAsync(new Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(OrderState.Paid, state);
        Assert.Equal(OrderState.Paid, await runtime.GetCurrentStateAsync());
    }
}