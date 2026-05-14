namespace StateForge.Core.Execution;

/// <summary>Runtime snapshot of the active leaf inside one parallel region.</summary>
public sealed record ActiveRegionEntry<TState>(
    string RegionId,
    string RegionName,
    TState ActiveLeafState,
    ActiveStatePath<TState> ActivePath,
    bool IsTerminal);