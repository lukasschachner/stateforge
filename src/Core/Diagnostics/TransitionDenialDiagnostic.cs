using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Structured machine-readable explanation for a transition preview or actual transition denial.</summary>
public sealed class TransitionDenialDiagnostic
{
    public TransitionDenialDiagnostic(
        TransitionDenialReason reason,
        string message,
        TransitionLifecyclePhase phase = TransitionLifecyclePhase.None,
        object? state = null,
        string? eventIdentity = null,
        string? transitionId = null,
        IReadOnlyList<string>? candidateTransitionIds = null,
        IReadOnlyList<TransitionPreviewGuardDiagnostic>? guardDiagnostics = null,
        IReadOnlyList<string>? validationCodes = null,
        string? regionId = null,
        string? regionName = null,
        IReadOnlyList<TransitionConflictDiagnostic>? conflictDiagnostics = null)
    {
        Reason = reason;
        Message = message;
        Phase = phase;
        State = state;
        EventIdentity = eventIdentity;
        TransitionId = transitionId;
        CandidateTransitionIds = candidateTransitionIds ?? Array.Empty<string>();
        GuardDiagnostics = guardDiagnostics ?? Array.Empty<TransitionPreviewGuardDiagnostic>();
        ValidationCodes = validationCodes ?? Array.Empty<string>();
        RegionId = regionId;
        RegionName = regionName;
        ConflictDiagnostics = conflictDiagnostics ?? Array.Empty<TransitionConflictDiagnostic>();
    }

    /// <summary>Stable denial reason.</summary>
    public TransitionDenialReason Reason { get; }

    /// <summary>Safe human-readable summary. Automation should prefer <see cref="Reason" />.</summary>
    public string Message { get; }

    /// <summary>Lifecycle phase most closely associated with the denial.</summary>
    public TransitionLifecyclePhase Phase { get; }

    /// <summary>Current state or active leaf involved, when known.</summary>
    public object? State { get; }

    /// <summary>Stable event identity involved, when known.</summary>
    public string? EventIdentity { get; }

    /// <summary>Selected or conflicting transition identifier, when known.</summary>
    public string? TransitionId { get; }

    /// <summary>Candidate or conflicting transition identifiers in deterministic order.</summary>
    public IReadOnlyList<string> CandidateTransitionIds { get; }

    /// <summary>Guard diagnostics relevant to condition denials.</summary>
    public IReadOnlyList<TransitionPreviewGuardDiagnostic> GuardDiagnostics { get; }

    /// <summary>Validation codes relevant to invalid definitions or active shapes.</summary>
    public IReadOnlyList<string> ValidationCodes { get; }

    /// <summary>Parallel-region identifier associated with the denial, when applicable.</summary>
    public string? RegionId { get; }

    /// <summary>Parallel-region name associated with the denial, when applicable.</summary>
    public string? RegionName { get; }

    /// <summary>Existing conflict diagnostics associated with ambiguity or validation conflicts.</summary>
    public IReadOnlyList<TransitionConflictDiagnostic> ConflictDiagnostics { get; }

    internal static TransitionDenialDiagnostic ValidationConflicts(
        IEnumerable<ValidationFinding> findings,
        IReadOnlyList<TransitionConflictDiagnostic>? conflicts = null)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.ValidationConflicts,
            "Machine definition has validation errors.",
            validationCodes: findings.Select(f => f.Code).ToArray(),
            conflictDiagnostics: conflicts);
    }

    internal static TransitionDenialDiagnostic InvalidActiveShape<TState>(
        IEnumerable<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.InvalidActiveShape,
            "Active state shape is invalid for the machine definition.",
            TransitionLifecyclePhase.Matching,
            validationCodes: diagnostics.Select(d => d.Code).ToArray());
    }

    internal static TransitionDenialDiagnostic UnknownCurrentState(object? state)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.UnknownCurrentState,
            $"Current state '{state}' is not declared by the machine definition.",
            TransitionLifecyclePhase.Matching,
            state);
    }

    internal static TransitionDenialDiagnostic UnknownEvent(string? eventIdentity)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.UnknownEvent,
            "Event is not declared by the machine definition.",
            TransitionLifecyclePhase.Matching,
            eventIdentity: eventIdentity);
    }

    internal static TransitionDenialDiagnostic NoMatchingEvent(object? state, string? eventIdentity)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.NoMatchingEvent,
            $"Event '{eventIdentity}' is not permitted from state '{state}'.",
            TransitionLifecyclePhase.Matching,
            state,
            eventIdentity);
    }

    internal static TransitionDenialDiagnostic TerminalState(object? state, string? eventIdentity)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.TerminalState,
            $"State '{state}' is terminal and no transition is permitted.",
            TransitionLifecyclePhase.Matching,
            state,
            eventIdentity);
    }

    internal static TransitionDenialDiagnostic FailedGuards(
        string transitionId,
        IReadOnlyList<TransitionPreviewGuardDiagnostic> guards)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.FailedGuards,
            "One or more transition guards denied the transition.",
            TransitionLifecyclePhase.Condition,
            transitionId: transitionId,
            candidateTransitionIds: [transitionId],
            guardDiagnostics: guards);
    }

    internal static TransitionDenialDiagnostic GuardCancelled(
        string? transitionId,
        IReadOnlyList<TransitionPreviewGuardDiagnostic> guards)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.GuardEvaluationCancelled,
            "Guard evaluation was cancelled.",
            TransitionLifecyclePhase.Condition,
            transitionId: transitionId,
            candidateTransitionIds: transitionId is null ? [] : [transitionId],
            guardDiagnostics: guards);
    }

    internal static TransitionDenialDiagnostic GuardFailed(
        string? transitionId,
        IReadOnlyList<TransitionPreviewGuardDiagnostic> guards)
    {
        return new TransitionDenialDiagnostic(
            TransitionDenialReason.GuardEvaluationFailed,
            "Guard evaluation failed.",
            TransitionLifecyclePhase.Condition,
            transitionId: transitionId,
            candidateTransitionIds: transitionId is null ? [] : [transitionId],
            guardDiagnostics: guards);
    }
}
