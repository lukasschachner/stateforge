namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Stable machine-readable category for transition conflict diagnostics.</summary>
public enum TransitionConflictKind
{
    DuplicateSourceScope,
    ParentRegionalConflict,
    CrossRegionBoundary,
    InvalidPostShape,
    AmbiguousGuardOutcome,
    CompletionConflict
}
