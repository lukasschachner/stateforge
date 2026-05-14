namespace StateForge.Core.Execution;

/// <summary>
///     Provider-neutral immutable snapshot of a runtime active-state shape.
/// </summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed class ActiveStateSnapshot<TState>
{
    /// <summary>Creates an active-state snapshot with explicit shape metadata.</summary>
    public ActiveStateSnapshot(
        ActiveStateSnapshotKind kind,
        TState? activeLeafState = default,
        ActiveStatePath<TState>? activePath = null,
        TState? owningCompositeState = default,
        IEnumerable<ActiveRegionSnapshot<TState>>? regionSnapshots = null,
        long sequence = 0,
        string? definitionFingerprint = null)
    {
        Kind = kind;
        ActiveLeafState = activeLeafState;
        ActivePath = activePath;
        OwningCompositeState = owningCompositeState;
        RegionSnapshots = (regionSnapshots ?? []).ToArray();
        Sequence = sequence;
        DefinitionFingerprint = definitionFingerprint;
    }

    /// <summary>Shape category represented by this snapshot.</summary>
    public ActiveStateSnapshotKind Kind { get; }

    /// <summary>Active leaf state for single-leaf and hierarchical snapshots.</summary>
    public TState? ActiveLeafState { get; }

    /// <summary>Ordered active path for hierarchical snapshots.</summary>
    public ActiveStatePath<TState>? ActivePath { get; }

    /// <summary>Owning composite state for parallel snapshots.</summary>
    public TState? OwningCompositeState { get; }

    /// <summary>Ordered region snapshots for parallel snapshots.</summary>
    public IReadOnlyList<ActiveRegionSnapshot<TState>> RegionSnapshots { get; }

    /// <summary>Runtime sequence point captured with the active shape.</summary>
    public long Sequence { get; }

    /// <summary>Optional application/definition identity marker used for compatibility checks.</summary>
    public string? DefinitionFingerprint { get; }

    /// <summary>Creates a flat single-leaf snapshot.</summary>
    public static ActiveStateSnapshot<TState> SingleLeaf(
        TState activeLeafState,
        long sequence = 0,
        string? definitionFingerprint = null)
    {
        return new ActiveStateSnapshot<TState>(ActiveStateSnapshotKind.SingleLeaf, activeLeafState,
            sequence: sequence, definitionFingerprint: definitionFingerprint);
    }

    /// <summary>Creates a hierarchical path snapshot.</summary>
    public static ActiveStateSnapshot<TState> Hierarchical(
        TState activeLeafState,
        ActiveStatePath<TState> activePath,
        long sequence = 0,
        string? definitionFingerprint = null)
    {
        return new ActiveStateSnapshot<TState>(ActiveStateSnapshotKind.Hierarchical, activeLeafState, activePath,
            sequence: sequence, definitionFingerprint: definitionFingerprint);
    }

    /// <summary>Creates a parallel active-region snapshot.</summary>
    public static ActiveStateSnapshot<TState> Parallel(
        TState owningCompositeState,
        IEnumerable<ActiveRegionSnapshot<TState>> regionSnapshots,
        long sequence = 0,
        string? definitionFingerprint = null)
    {
        return new ActiveStateSnapshot<TState>(ActiveStateSnapshotKind.Parallel,
            owningCompositeState: owningCompositeState, regionSnapshots: regionSnapshots, sequence: sequence,
            definitionFingerprint: definitionFingerprint);
    }
}
