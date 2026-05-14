namespace StateForge.Core.Execution;

/// <summary>Immutable active-state snapshot for one region of a parallel composite.</summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed record ActiveRegionSnapshot<TState>(
    string RegionId,
    string? RegionName,
    TState ActiveLeafState,
    ActiveStatePath<TState> ActivePath,
    bool IsTerminal);
