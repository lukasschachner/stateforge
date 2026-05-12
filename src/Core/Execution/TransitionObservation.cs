using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Immutable structured notification for one point in a transition attempt lifecycle.</summary>
/// <remarks>
///     <see cref="TransitionObservationKind.Started" /> is the first notification for observed attempts and
///     <see cref="TransitionObservationKind.Outcome" /> is emitted exactly once as the final notification.
///     Timing values are captured only for observed attempts.
/// </remarks>
public sealed class TransitionObservation<TState, TEvent>
{
    public TransitionObservation(
        Guid attemptId,
        TransitionObservationKind kind,
        TransitionLifecyclePhase phase,
        string? machineName,
        TState sourceState,
        TState? targetState,
        TState? resultingState,
        TEvent @event,
        Type? eventType,
        TransitionKind? transitionKind,
        TransitionTriggerKind triggerKind,
        TransitionOutcomeCategory? outcomeCategory,
        bool committed,
        TransitionDiagnostics diagnostics,
        DateTimeOffset occurredAt,
        TimeSpan? elapsed,
        TransitionOutcome<TState, TEvent>? outcome,
        ActiveStatePath<TState>? activeStatePath = null,
        ActiveStateShape<TState>? activeStateShape = null)
    {
        AttemptId = attemptId;
        Kind = kind;
        Phase = phase;
        MachineName = machineName;
        SourceState = sourceState;
        TargetState = targetState;
        ResultingState = resultingState;
        Event = @event;
        EventType = eventType;
        TransitionKind = transitionKind;
        TriggerKind = triggerKind;
        OutcomeCategory = outcomeCategory;
        Committed = committed;
        Diagnostics = diagnostics;
        OccurredAt = occurredAt;
        Elapsed = elapsed;
        Outcome = outcome;
        ActiveStatePath = activeStatePath ?? outcome?.ActiveStatePath;
        ActiveStateShape = activeStateShape ?? outcome?.ActiveStateShape;
    }

    public Guid AttemptId { get; }
    public TransitionObservationKind Kind { get; }
    public TransitionLifecyclePhase Phase { get; }
    public string? MachineName { get; }
    public TState SourceState { get; }
    public TState? TargetState { get; }
    public TState? ResultingState { get; }
    public TEvent Event { get; }
    public Type? EventType { get; }
    public TransitionKind? TransitionKind { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public bool IsCompletionTrigger => TriggerKind == TransitionTriggerKind.Completion;
    public TransitionOutcomeCategory? OutcomeCategory { get; }
    public bool Committed { get; }
    public TransitionDiagnostics Diagnostics { get; }
    public DateTimeOffset OccurredAt { get; }
    public TimeSpan? Elapsed { get; }
    public TransitionOutcome<TState, TEvent>? Outcome { get; }
    public ActiveStatePath<TState>? ActiveStatePath { get; }
    public ActiveStateShape<TState>? ActiveStateShape { get; }
}