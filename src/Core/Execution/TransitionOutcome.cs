using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Structured result of applying an event to a state machine definition or runtime.</summary>
public sealed class TransitionOutcome<TState, TEvent>
{
    private TransitionOutcome(
        TransitionOutcomeCategory category,
        TState previousState,
        TState resultingState,
        TEvent @event,
        TransitionDefinition<TState, TEvent>? transition,
        TransitionDiagnostics diagnostics,
        bool committed,
        ActiveStatePath<TState>? activeStatePath = null,
        IEnumerable<CompositeHistorySnapshot<TState>>? historySnapshots = null,
        ActiveStateShape<TState>? activeStateShape = null,
        IEnumerable<TransitionDefinition<TState, TEvent>>? parallelTransitions = null)
    {
        Category = category;
        PreviousState = previousState;
        ResultingState = resultingState;
        Event = @event;
        Transition = transition;
        Diagnostics = diagnostics;
        Committed = committed;
        ActiveStatePath = activeStatePath ?? new ActiveStatePath<TState>([resultingState]);
        HistorySnapshots = (historySnapshots ?? []).ToArray();
        ActiveStateShape = activeStateShape ?? ActiveStateShape<TState>.Single(ActiveStatePath.ActiveLeafState);
        ParallelTransitions = (parallelTransitions ?? (transition is null ? [] : [transition])).ToArray();
    }

    public TransitionOutcomeCategory Category { get; }
    public TState PreviousState { get; }
    public TState ResultingState { get; }
    public TEvent Event { get; }
    public TransitionDefinition<TState, TEvent>? Transition { get; }
    public TransitionDiagnostics Diagnostics { get; }
    public bool IsSuccess => Category == TransitionOutcomeCategory.Success;
    public bool Committed { get; }
    public TState ActiveLeafState => ActiveStatePath.ActiveLeafState;
    public ActiveStatePath<TState> ActiveStatePath { get; }
    public ActiveStateShape<TState> ActiveStateShape { get; }
    public IReadOnlyList<TransitionDefinition<TState, TEvent>> ParallelTransitions { get; }
    public IReadOnlyList<CompositeHistorySnapshot<TState>> HistorySnapshots { get; }

    /// <summary>Structured machine-readable conflict diagnostics forwarded from <see cref="Diagnostics" />.</summary>
    public IReadOnlyList<TransitionConflictDiagnostic> ConflictDiagnostics => Diagnostics.ConflictDiagnostics;

    /// <summary>Structured machine-readable denial diagnostics forwarded from <see cref="Diagnostics" />.</summary>
    public IReadOnlyList<TransitionDenialDiagnostic> DenialDiagnostics => Diagnostics.DenialDiagnostics;

    public static TransitionOutcome<TState, TEvent> Success(TState previousState, TState resultingState, TEvent @event,
        TransitionDefinition<TState, TEvent> transition, ActiveStatePath<TState>? activeStatePath = null,
        IEnumerable<CompositeHistorySnapshot<TState>>? historySnapshots = null,
        ActiveStateShape<TState>? activeStateShape = null,
        IEnumerable<TransitionDefinition<TState, TEvent>>? parallelTransitions = null)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.Success, previousState, resultingState,
            @event, transition,
            TransitionDiagnostics.None, true, activeStatePath, historySnapshots, activeStateShape, parallelTransitions);
    }

    public static TransitionOutcome<TState, TEvent> Denied(TState state, TEvent @event,
        TransitionDefinition<TState, TEvent> transition, TransitionDiagnostics diagnostics)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.Denied, state, state, @event, transition,
            diagnostics, false);
    }

    public static TransitionOutcome<TState, TEvent> NotPermitted(TState state, TEvent @event,
        TransitionDiagnostics diagnostics)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.NotPermitted, state, state, @event, null,
            diagnostics, false);
    }

    public static TransitionOutcome<TState, TEvent> ValidationFailure(TState state, TEvent @event,
        TransitionDiagnostics diagnostics)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.ValidationFailure, state, state, @event,
            null, diagnostics, false);
    }

    public static TransitionOutcome<TState, TEvent> Cancelled(TState previousState, TState resultingState,
        TEvent @event, TransitionDefinition<TState, TEvent>? transition, TransitionDiagnostics diagnostics,
        bool committed)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.Cancelled, previousState, resultingState,
            @event, transition, diagnostics,
            committed);
    }

    public static TransitionOutcome<TState, TEvent> BehaviorFailure(TState previousState, TState resultingState,
        TEvent @event, TransitionDefinition<TState, TEvent>? transition, TransitionDiagnostics diagnostics,
        bool committed)
    {
        return new TransitionOutcome<TState, TEvent>(TransitionOutcomeCategory.BehaviorFailure, previousState,
            resultingState, @event, transition,
            diagnostics, committed);
    }
}