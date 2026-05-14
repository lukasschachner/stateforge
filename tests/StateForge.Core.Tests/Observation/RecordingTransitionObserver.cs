using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

internal sealed class RecordingTransitionObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    private readonly Func<TransitionObservation<TState, TEvent>, ValueTask>? _onObserve;

    public RecordingTransitionObserver(Func<TransitionObservation<TState, TEvent>, ValueTask>? onObserve = null)
    {
        _onObserve = onObserve;
    }

    public List<TransitionObservation<TState, TEvent>> Observations { get; } = [];

    public async ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default)
    {
        Observations.Add(observation);
        if (_onObserve is not null) await _onObserve(observation);
    }
}