using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Read-only runtime snapshot of recorded per-region history for a history-enabled parallel composite.</summary>
public sealed record ParallelHistorySnapshot<TState>(
    TState CompositeState,
    HistoryMode HistoryMode,
    IReadOnlyList<ParallelRegionHistoryEntry<TState>> RegionEntries,
    bool HasCompleteRecordedShape,
    long LastUpdatedSequence);