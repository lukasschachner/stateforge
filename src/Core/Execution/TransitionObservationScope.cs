using System.Diagnostics;
using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class TransitionObservationScope<TState, TEvent>
{
    private readonly ITransitionObserver<TState, TEvent> _observer;
    private readonly DateTimeOffset _startedAt;
    private readonly Stopwatch _stopwatch;
    private bool _outcomeEmitted;

    public TransitionObservationScope(
        ITransitionObserver<TState, TEvent> observer,
        StateMachineDefinition<TState, TEvent> definition,
        TState sourceState,
        TEvent @event,
        TransitionTriggerKind triggerKind = TransitionTriggerKind.Event)
    {
        _observer = observer;
        MachineName = TryGetMachineName(definition);
        SourceState = sourceState;
        Event = @event;
        EventType = @event?.GetType();
        TriggerKind = triggerKind;
        AttemptId = Guid.NewGuid();
        _startedAt = DateTimeOffset.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    public Guid AttemptId { get; }
    public string? MachineName { get; }
    public TState SourceState { get; }
    public TEvent Event { get; }
    public Type? EventType { get; }
    public TransitionTriggerKind TriggerKind { get; }

    public ValueTask StartedAsync(CancellationToken cancellationToken)
    {
        return ObserveAsync(TransitionObservationKind.Started, TransitionLifecyclePhase.Matching, null, null, false,
            TransitionDiagnostics.None, default, cancellationToken);
    }

    public ValueTask ObserveAsync(
        TransitionObservationKind kind,
        TransitionLifecyclePhase phase,
        TransitionDefinition<TState, TEvent>? transition,
        TransitionOutcome<TState, TEvent>? outcome,
        bool committed,
        TransitionDiagnostics diagnostics,
        TState? resultingState,
        CancellationToken cancellationToken)
    {
        if (kind == TransitionObservationKind.Outcome)
        {
            if (_outcomeEmitted) return ValueTask.CompletedTask;

            _outcomeEmitted = true;
        }

        var observation = new TransitionObservation<TState, TEvent>(
            AttemptId,
            kind,
            phase,
            MachineName,
            SourceState,
            transition is null ? default : transition.TargetState,
            outcome is null ? resultingState : outcome.ResultingState,
            Event,
            EventType,
            transition?.Kind,
            transition?.TriggerKind ?? outcome?.Transition?.TriggerKind ?? TriggerKind,
            outcome?.Category,
            committed || outcome?.Committed == true,
            diagnostics,
            kind == TransitionObservationKind.Started ? _startedAt : DateTimeOffset.UtcNow,
            _stopwatch.Elapsed,
            outcome);

        return InvokeObserverAsync(observation, cancellationToken);
    }

    public ValueTask OutcomeAsync(TransitionOutcome<TState, TEvent> outcome, CancellationToken cancellationToken)
    {
        return ObserveAsync(
            TransitionObservationKind.Outcome,
            outcome.Diagnostics.Phase,
            outcome.Transition,
            outcome,
            outcome.Committed,
            outcome.Diagnostics,
            outcome.ResultingState,
            cancellationToken);
    }

    private static string? TryGetMachineName(StateMachineDefinition<TState, TEvent> definition)
    {
        if (definition.Metadata.TryGetValue(StateMachineMetadataKeys.Name, out var value) && value is not null)
            return value.ToString();

        return null;
    }

    private async ValueTask InvokeObserverAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken)
    {
        try
        {
            await _observer.ObserveAsync(observation, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Observation is a side channel. Observer failures, including cancellation exceptions,
            // are intentionally isolated from transition outcomes and diagnostics.
        }
    }
}