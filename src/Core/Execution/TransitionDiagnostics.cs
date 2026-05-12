using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Diagnostics explaining non-success transition outcomes.</summary>
public sealed class TransitionDiagnostics
{
    public TransitionDiagnostics(
        string summary,
        TransitionLifecyclePhase phase = TransitionLifecyclePhase.None,
        Exception? exception = null,
        IReadOnlyList<ValidationFinding>? validationFindings = null,
        string? affectedElement = null,
        object? hierarchyMetadata = null,
        object? parallelMetadata = null,
        IReadOnlyList<TransitionConflictDiagnostic>? conflictDiagnostics = null)
    {
        Summary = summary;
        Phase = phase;
        Exception = exception;
        ValidationFindings = validationFindings ?? Array.Empty<ValidationFinding>();
        AffectedElement = affectedElement;
        HierarchyMetadata = hierarchyMetadata;
        ParallelMetadata = parallelMetadata;
        ConflictDiagnostics = conflictDiagnostics ?? Array.Empty<TransitionConflictDiagnostic>();
    }

    public static TransitionDiagnostics None { get; } = new("Transition completed.");

    public string Summary { get; }
    public TransitionLifecyclePhase Phase { get; }
    public Exception? Exception { get; }
    public IReadOnlyList<ValidationFinding> ValidationFindings { get; }
    public string? AffectedElement { get; }

    /// <summary>
    ///     Optional hierarchy resolution metadata for outcomes that selected a transition.
    ///     Consumers should treat this as additive, non-executable diagnostics payload.
    /// </summary>
    public object? HierarchyMetadata { get; }

    /// <summary>Optional parallel-region conflict or dispatch metadata.</summary>
    public object? ParallelMetadata { get; }

    /// <summary>Structured machine-readable transition conflict diagnostics.</summary>
    public IReadOnlyList<TransitionConflictDiagnostic> ConflictDiagnostics { get; }
}

internal sealed record HierarchySelectionDiagnostics(
    object? ActiveLeafState,
    object? SelectedSourceState,
    object? DeclaredTargetState,
    object? ResolvedTargetLeafState,
    int SourcePathDepth,
    int TargetPathDepth,
    bool SourceMatchedAncestor,
    bool TargetResolvedThroughInitialChild,
    string? HistoryRestoreSource = null);