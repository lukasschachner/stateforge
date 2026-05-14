namespace StateForge.Core.Execution;

/// <summary>Observer decorator that forwards only observations accepted by a predicate.</summary>
public sealed class FilteredTransitionObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    private readonly ITransitionObserver<TState, TEvent> _inner;
    private readonly Func<TransitionObservation<TState, TEvent>, bool> _predicate;

    public FilteredTransitionObserver(
        ITransitionObserver<TState, TEvent> inner,
        Func<TransitionObservation<TState, TEvent>, bool> predicate)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default)
    {
        return _predicate(observation) ? _inner.ObserveAsync(observation, cancellationToken) : ValueTask.CompletedTask;
    }
}