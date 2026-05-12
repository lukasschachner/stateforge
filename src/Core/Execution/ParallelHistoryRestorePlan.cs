using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Precomputed complete active-region shape for restoring a history-enabled parallel composite.</summary>
public sealed record ParallelHistoryRestorePlan<TState>(
    TState CompositeState,
    HistoryMode HistoryMode,
    IReadOnlyList<ParallelRegionRestoreEntry<TState>> RestoreEntries,
    IReadOnlyList<ValidationFinding> ValidationFindings,
    ActiveStateShape<TState>? PreRestoreShape,
    ActiveStateShape<TState> PlannedPostRestoreShape)
{
    public bool UsesFallback => RestoreEntries.Any(entry => entry.RestoreKind == ParallelHistoryRestoreKind.Fallback);
    public bool IsValid => ValidationFindings.Count == 0;
}