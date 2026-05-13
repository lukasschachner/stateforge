namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Structured detail for a guard considered during transition preview or denial diagnostics.</summary>
public sealed class TransitionPreviewGuardDiagnostic
{
    public TransitionPreviewGuardDiagnostic(
        string? transitionId,
        int guardIndex,
        string displayName,
        TransitionPreviewGuardStatus status,
        string? message = null)
    {
        TransitionId = transitionId;
        GuardIndex = guardIndex;
        DisplayName = displayName;
        Status = status;
        Message = message;
    }

    /// <summary>Stable transition identifier where available.</summary>
    public string? TransitionId { get; }

    /// <summary>Zero-based guard declaration order within the transition.</summary>
    public int GuardIndex { get; }

    /// <summary>Safe display name supplied for the guard.</summary>
    public string DisplayName { get; }

    /// <summary>Evaluation status.</summary>
    public TransitionPreviewGuardStatus Status { get; }

    /// <summary>Safe cancellation or error summary. Stack traces and callback internals are intentionally omitted.</summary>
    public string? Message { get; }
}
