namespace StateForge.Core.Execution;

/// <summary>Fan-out observer that forwards each transition observation to multiple child observers.</summary>
/// <remarks>
///     Child observers are invoked sequentially in the order supplied to the constructor. Exceptions and cancellation
///     exceptions from one child observer are suppressed so remaining child observers can still receive the notification.
/// </remarks>
public sealed class CompositeTransitionObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    public CompositeTransitionObserver(params ITransitionObserver<TState, TEvent>[] observers)
        : this((IEnumerable<ITransitionObserver<TState, TEvent>>)observers)
    {
    }

    public CompositeTransitionObserver(IEnumerable<ITransitionObserver<TState, TEvent>> observers)
    {
        ArgumentNullException.ThrowIfNull(observers);
        Observers = observers.Select(observer => observer ?? throw new ArgumentNullException(nameof(observers)))
            .ToArray();
    }

    /// <summary>Child observers invoked for each notification.</summary>
    public IReadOnlyList<ITransitionObserver<TState, TEvent>> Observers { get; }

    public async ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default)
    {
        foreach (var observer in Observers)
            try
            {
                await observer.ObserveAsync(observation, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Observation fan-out is best-effort; one child observer must not block others.
            }
    }
}