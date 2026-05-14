namespace StateForge.Persistence.Hooks;

/// <summary>
///     Optional persistence observer invoked after transition execution and optional persistence work.
/// </summary>
public interface ITransitionPersistenceObserver<TState, TEvent>
{
    /// <summary>
    ///     Observes a transition persistence context. Throwing or returning failure/cancelled propagates to caller.
    /// </summary>
    ValueTask<ObservationResult> ObserveAsync(
        TransitionPersistenceContext<TState, TEvent> context,
        CancellationToken cancellationToken = default);
}