using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Definitions;

public class MachineDefinitionTests
{
    [Fact]
    public void ValidLifecycleCanBeBuiltValidatedAndInspected()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>().WhenAsync(async (ctx, ct) =>
                {
                    await Task.Yield();
                    ct.ThrowIfCancellationRequested();
                    return ((Pay)ctx.Event).Amount > 0;
                }, "positive amount").GoTo(OrderState.Paid)
                .On<Cancel>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid)
                .On<Ship>().GoTo(OrderState.Shipped)
                .On<Cancel>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Shipped).Terminal();
            builder.State(OrderState.Cancelled).Terminal();
        });

        var validation = definition.Validate();

        Assert.True(validation.IsValid);
        Assert.Equal(4, definition.States.Count);
        Assert.Equal(4, definition.Transitions.Count);
        Assert.Equal(3, definition.Events.Count);
        Assert.Equal(2, definition.TerminalStates.Count);
        Assert.Contains(definition.Transitions, t => t.Conditions.Count == 1);
    }
}