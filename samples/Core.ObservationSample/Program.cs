using StateForge.Core.Definitions;
using StateForge.Core.Execution;

var observer = new ConsoleObserver<OrderState, OrderEvent>();
var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Draft)
        .On<Submit>().GoTo(OrderState.Submitted)
        .On<Reject>().When(_ => false, "demo denial").GoTo(OrderState.Submitted)
        .On<Fail>().Execute(_ => throw new InvalidOperationException("sample failure")).GoTo(OrderState.Submitted);
    builder.State(OrderState.Submitted);
});

foreach (var @event in new OrderEvent[] { new Submit(), new Reject(), new Fail(), new Unknown() })
{
    var outcome = await definition.ApplyAsync(OrderState.Draft, @event, observer: observer);
    Console.WriteLine($"Outcome: {outcome.Category} committed={outcome.Committed} resulting={outcome.ResultingState}");
}

Console.WriteLine("Core observation sample completed");

internal enum OrderState
{
    Draft,
    Submitted
}

internal abstract record OrderEvent;

internal sealed record Submit : OrderEvent;

internal sealed record Reject : OrderEvent;

internal sealed record Fail : OrderEvent;

internal sealed record Unknown : OrderEvent;

internal sealed class ConsoleObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"  {observation.Kind} attempt={observation.AttemptId} phase={observation.Phase} outcome={observation.OutcomeCategory}");
        return ValueTask.CompletedTask;
    }
}