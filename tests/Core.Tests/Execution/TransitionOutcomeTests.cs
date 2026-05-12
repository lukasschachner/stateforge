using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public class TransitionOutcomeTests
{
    [Fact]
    public async Task DeniedConditionPreservesState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().When(_ => false, "no").GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.Equal(TransitionOutcomeCategory.Denied, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Contains("no", outcome.Diagnostics.Summary);
    }

    [Fact]
    public async Task NotPermittedEventPreservesState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Ship());

        Assert.Equal(TransitionOutcomeCategory.NotPermitted, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
    }
}