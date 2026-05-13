using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.SourceGeneration;

internal static class Program
{
    private static async Task Main()
    {
        var definition = GeneratedOrderMachine.Definition;
        var outcome = await GeneratedOrderMachine.ApplyOrderEvent_PayAsync(OrderState.Created);
        if (outcome.Category != TransitionOutcomeCategory.Success)
            throw new InvalidOperationException(outcome.Diagnostics.Summary);

        var advancedDefinition = GeneratedAdvancedOrderMachine.Definition;
        var advancedValidation = advancedDefinition.Validate();
        if (!advancedValidation.IsValid)
            throw new InvalidOperationException(string.Join("; ", advancedValidation.Errors.Select(e => e.Message)));

        var advancedRuntime = advancedDefinition.CreateRuntime(OrderState.Created);
        await advancedRuntime.ApplyAsync(OrderEvent.Start);
        Console.WriteLine("Advanced initial regions: " + FormatRegions(advancedRuntime));
        await advancedRuntime.ApplyAsync(OrderEvent.Picked);
        Console.WriteLine("After fulfillment: " + FormatRegions(advancedRuntime));
        await advancedRuntime.ApplyAsync(OrderEvent.Pay);
        Console.WriteLine("After billing: " + FormatRegions(advancedRuntime));

        Console.WriteLine($"Source generator sample completed: {outcome.ResultingState}; advanced states: {advancedDefinition.States.Count}; metadata entries: {GeneratedOrderMachine.GeneratedMetadata.Count}; graph entries: {GeneratedOrderMachine.GeneratedGraph.Count}");
    }

    private static string FormatRegions(StateMachineRuntime<OrderState, OrderEvent> runtime)
    {
        return string.Join(", ", runtime.ActiveStateShape.ActiveRegions.Select(r => $"{r.RegionName}={r.ActiveLeafState}"));
    }
}

public enum OrderState
{
    Created,
    Paid,
    Shipped,
    Cancelled,
    Operational,
    Pick,
    PickDone,
    PayPending,
    PayDone
}

public enum OrderEvent
{
    Pay,
    Ship,
    Cancel,
    Start,
    Picked
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

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Operational, IsParallelComposite = true)]
[Region(OrderState.Operational, "Fulfillment", OrderState.Pick, IsInitial = true)]
[Region(OrderState.Operational, "Fulfillment", OrderState.PickDone, IsTerminal = true)]
[Region(OrderState.Operational, "Billing", OrderState.PayPending, IsInitial = true)]
[Region(OrderState.Operational, "Billing", OrderState.PayDone, IsTerminal = true)]
[Event(OrderEvent.Start)]
[Event(OrderEvent.Picked)]
[Event(OrderEvent.Pay)]
[Transition(OrderState.Created, OrderEvent.Start, OrderState.Operational)]
[Transition(OrderState.Pick, OrderEvent.Picked, OrderState.PickDone)]
[Transition(OrderState.PayPending, OrderEvent.Pay, OrderState.PayDone)]
public static partial class GeneratedAdvancedOrderMachine
{
}