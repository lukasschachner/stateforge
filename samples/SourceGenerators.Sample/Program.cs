using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.SourceGeneration;

internal static class Program
{
    private static async Task Main()
    {
        var definition = GeneratedOrderMachine.Definition;
        var outcome = await definition.ApplyAsync(OrderState.Created, OrderEvent.Pay);
        if (outcome.Category != TransitionOutcomeCategory.Success)
            throw new InvalidOperationException(outcome.Diagnostics.Summary);

        Console.WriteLine($"Source generator sample completed: {outcome.ResultingState}");
    }
}

public enum OrderState
{
    Created,
    Paid,
    Shipped,
    Cancelled
}

public enum OrderEvent
{
    Pay,
    Ship,
    Cancel
}

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Paid)]
[State(OrderState.Shipped, IsTerminal = true)]
[State(OrderState.Cancelled, IsTerminal = true)]
[Event(OrderEvent.Pay)]
[Event(OrderEvent.Ship)]
[Event(OrderEvent.Cancel)]
[Transition(OrderState.Created, OrderEvent.Pay, OrderState.Paid)]
[Transition(OrderState.Paid, OrderEvent.Ship, OrderState.Shipped)]
public static partial class GeneratedOrderMachine
{
}