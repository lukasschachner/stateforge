using StateForge.Core.Definitions;

namespace StateForge.Core.Diagnostics;

/// <summary>Transition candidate considered during preview matching or denial diagnostics.</summary>
public sealed class TransitionPreviewCandidate
{
    public TransitionPreviewCandidate(
        string transitionId,
        object? sourceState,
        string eventIdentity,
        object? targetState,
        TransitionTriggerKind triggerKind,
        string selectionRole,
        string? regionId = null,
        string? regionName = null,
        IReadOnlyList<TransitionPreviewGuardDiagnostic>? guardDiagnostics = null)
    {
        TransitionId = transitionId;
        SourceState = sourceState;
        EventIdentity = eventIdentity;
        TargetState = targetState;
        TriggerKind = triggerKind;
        SelectionRole = selectionRole;
        RegionId = regionId;
        RegionName = regionName;
        GuardDiagnostics = guardDiagnostics ?? Array.Empty<TransitionPreviewGuardDiagnostic>();
    }

    /// <summary>Stable transition identifier aligned with Core transition display conventions.</summary>
    public string TransitionId { get; }

    /// <summary>Declared source state.</summary>
    public object? SourceState { get; }

    /// <summary>Declared event identity.</summary>
    public string EventIdentity { get; }

    /// <summary>Declared direct target state.</summary>
    public object? TargetState { get; }

    /// <summary>Trigger kind for the candidate.</summary>
    public TransitionTriggerKind TriggerKind { get; }

    /// <summary>Deterministic role assigned by matching or guard evaluation.</summary>
    public string SelectionRole { get; }

    /// <summary>Parallel-region identifier when the candidate belongs to a regional dispatch.</summary>
    public string? RegionId { get; }

    /// <summary>Parallel-region name when the candidate belongs to a regional dispatch.</summary>
    public string? RegionName { get; }

    /// <summary>Guard diagnostics associated with this candidate.</summary>
    public IReadOnlyList<TransitionPreviewGuardDiagnostic> GuardDiagnostics { get; }
}
