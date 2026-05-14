namespace StateForge.Core.Execution;

public enum ParallelHistoryRestoreKind
{
    Recorded,
    Fallback
}

/// <summary>Effective restore target for one region after recorded history and fallback are applied.</summary>
public sealed record ParallelRegionRestoreEntry<TState>(
    string RegionId,
    string RegionName,
    int RegionOrder,
    ParallelHistoryRestoreKind RestoreKind,
    TState ResolvedLeafState,
    ActiveStatePath<TState> ResolvedPath,
    long? SourceHistoryEntrySequence);