using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class TransitionLifecycleTests
{
    [Fact]
    public async Task ExternalTransitionRunsPhasesInDocumentedOrder()
    {
        var order = new List<string>();
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>()
                .When(_ =>
                {
                    order.Add("condition");
                    return true;
                })
                .OnExit(_ => order.Add("exit"))
                .Execute(_ => order.Add("transition"))
                .OnEntry(_ => order.Add("entry"))
                .GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(["condition", "exit", "transition", "entry"], order);
    }
}