namespace StateForge.Core.Execution;

/// <summary>Recorded runtime history for one owned region of a history-enabled parallel composite.</summary>
public sealed record ParallelRegionHistoryEntry<TState>(
    string RegionId,
    string RegionName,
    int RegionOrder,
    TState LastActiveLeafState,
    ActiveStatePath<TState> LastActivePath,
    long LastUpdatedSequence);