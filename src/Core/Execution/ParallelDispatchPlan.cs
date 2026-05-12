using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Pre-commit regional dispatch plan for a parallel active-state shape.</summary>
internal sealed record ParallelDispatchPlan<TState, TEvent>(
    TEvent Event,
    ActiveStateShape<TState> PreDispatchShape,
    IReadOnlyList<TransitionDefinition<TState, TEvent>> SelectedRegionalTransitions,
    TransitionDefinition<TState, TEvent>? ParentTransitionCandidate,
    IReadOnlyList<TransitionDiagnostics> ConflictFindings,
    ActiveStateShape<TState>? PostDispatchShape);