namespace StateMachineLibrary.Core.Execution;

/// <summary>
///     Receives dependency-free transition lifecycle observations from Core execution.
/// </summary>
/// <remarks>
///     Observers are optional and are supplied explicitly to definition application or runtime creation APIs.
///     Core suppresses observer exceptions and cancellation exceptions so observation never changes transition results.
///     Consumers that need fan-out, logging, telemetry, or durable delivery can implement those policies outside Core.
/// </remarks>
public interface ITransitionObserver<TState, TEvent>
{
    /// <summary>Observes one transition lifecycle notification.</summary>
    ValueTask ObserveAsync(
        TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default);
}