using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public class SuppliedStateExecutionTests
{
    [Fact]
    public async Task ApplyAsyncReturnsSuccessAndTargetState()
    {
        var definition = Definition();

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay(10));

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.PreviousState);
        Assert.Equal(OrderState.Paid, outcome.ResultingState);
    }

    private static StateMachineDefinition<OrderState, OrderEvent> Definition()
    {
        return StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });
    }
}