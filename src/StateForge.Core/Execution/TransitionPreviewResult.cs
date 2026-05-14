using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Validation;

namespace StateForge.Core.Execution;

/// <summary>Immutable side-effect-free preview of what an event would do for a supplied active shape.</summary>
public sealed class TransitionPreviewResult<TState, TEvent>
{
    public TransitionPreviewResult(
        TransitionPreviewStatus status,
        TEvent @event,
        ActiveStateShape<TState> currentActiveShape,
        TransitionDefinition<TState, TEvent>? selectedTransition = null,
        IReadOnlyList<TransitionDefinition<TState, TEvent>>? parallelTransitions = null,
        IReadOnlyList<TransitionPreviewCandidate>? candidateTransitions = null,
        IReadOnlyList<TransitionPreviewGuardDiagnostic>? guardDiagnostics = null,
        TState? expectedTargetState = default,
        ActiveStateShape<TState>? expectedActiveShape = null,
        TransitionPredictionCompleteness predictionCompleteness = TransitionPredictionCompleteness.Unknown,
        TransitionDenialDiagnostic? denialDiagnostic = null,
        IReadOnlyList<ValidationFinding>? validationFindings = null,
        IReadOnlyList<TransitionConflictDiagnostic>? conflictDiagnostics = null)
    {
        Status = status;
        Event = @event;
        CurrentActiveShape = currentActiveShape;
        SelectedTransition = selectedTransition;
        ParallelTransitions = parallelTransitions ?? Array.Empty<TransitionDefinition<TState, TEvent>>();
        CandidateTransitions = candidateTransitions ?? Array.Empty<TransitionPreviewCandidate>();
        GuardDiagnostics = guardDiagnostics ?? Array.Empty<TransitionPreviewGuardDiagnostic>();
        ExpectedTargetState = expectedTargetState;
        ExpectedActiveShape = expectedActiveShape;
        PredictionCompleteness = predictionCompleteness;
        DenialDiagnostic = denialDiagnostic;
        ValidationFindings = validationFindings ?? Array.Empty<ValidationFinding>();
        ConflictDiagnostics = conflictDiagnostics ?? Array.Empty<TransitionConflictDiagnostic>();
    }

    /// <summary>High-level preview status.</summary>
    public TransitionPreviewStatus Status { get; }

    /// <summary>True only when preview selected and allowed a transition.</summary>
    public bool IsPermitted => Status == TransitionPreviewStatus.Permitted;

    /// <summary>The event value supplied to preview.</summary>
    public TEvent Event { get; }

    /// <summary>The active shape used to perform the preview. The shape is not mutated by preview.</summary>
    public ActiveStateShape<TState> CurrentActiveShape { get; }

    /// <summary>Selected transition for non-parallel or primary regional previews, when known.</summary>
    public TransitionDefinition<TState, TEvent>? SelectedTransition { get; }

    /// <summary>Selected regional transitions for parallel dispatch previews.</summary>
    public IReadOnlyList<TransitionDefinition<TState, TEvent>> ParallelTransitions { get; }

    /// <summary>Considered transition candidates in deterministic order.</summary>
    public IReadOnlyList<TransitionPreviewCandidate> CandidateTransitions { get; }

    /// <summary>Guard diagnostics in deterministic evaluation order.</summary>
    public IReadOnlyList<TransitionPreviewGuardDiagnostic> GuardDiagnostics { get; }

    /// <summary>Direct target state when known.</summary>
    public TState? ExpectedTargetState { get; }

    /// <summary>Predicted direct post-transition active shape when knowable.</summary>
    public ActiveStateShape<TState>? ExpectedActiveShape { get; }

    /// <summary>How completely preview predicted the direct post-transition active shape.</summary>
    public TransitionPredictionCompleteness PredictionCompleteness { get; }

    /// <summary>Structured denial reason when preview is not permitted.</summary>
    public TransitionDenialDiagnostic? DenialDiagnostic { get; }

    /// <summary>Definition validation findings when preview cannot proceed authoritatively.</summary>
    public IReadOnlyList<ValidationFinding> ValidationFindings { get; }

    /// <summary>Transition conflict diagnostics when ambiguity or validation conflicts are involved.</summary>
    public IReadOnlyList<TransitionConflictDiagnostic> ConflictDiagnostics { get; }
}
