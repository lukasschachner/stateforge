using StateForge.Core.Definitions;

namespace StateForge.Core.Execution;

public enum ActiveStateShapeKind
{
    SingleLeaf,
    Parallel
}

/// <summary>Runtime active-state shape: either a single leaf or a parallel composite with one leaf per region.</summary>
public sealed class ActiveStateShape<TState>
{
    private ActiveStateShape(ActiveStateShapeKind kind, TState? activeLeafState, TState? owningCompositeState,
        IEnumerable<ActiveRegionEntry<TState>> activeRegions, long sequence)
    {
        Kind = kind;
        ActiveLeafState = activeLeafState;
        OwningCompositeState = owningCompositeState;
        ActiveRegions = activeRegions.ToArray();
        Sequence = sequence;
    }

    public ActiveStateShapeKind Kind { get; }
    public bool IsParallel => Kind == ActiveStateShapeKind.Parallel;
    public TState? ActiveLeafState { get; }
    public TState? OwningCompositeState { get; }
    public IReadOnlyList<ActiveRegionEntry<TState>> ActiveRegions { get; }
    public long Sequence { get; }

    public static ActiveStateShape<TState> Single(TState activeLeafState, long sequence = 0)
    {
        return new ActiveStateShape<TState>(ActiveStateShapeKind.SingleLeaf, activeLeafState, default, [], sequence);
    }

    public static ActiveStateShape<TState> Parallel(TState owningCompositeState,
        IEnumerable<ActiveRegionEntry<TState>> activeRegions, long sequence = 0)
    {
        return new ActiveStateShape<TState>(ActiveStateShapeKind.Parallel, default, owningCompositeState, activeRegions,
            sequence);
    }

    public TState RequireSingleLeaf()
    {
        return Kind == ActiveStateShapeKind.SingleLeaf
            ? ActiveLeafState!
            : throw new InvalidOperationException(
                "The active state shape contains parallel regions, not a single leaf.");
    }

    /// <summary>Converts the runtime active shape into the public active-state snapshot abstraction.</summary>
    public ActiveStateSnapshot<TState> ToActiveStateSnapshot<TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        string? definitionFingerprint = null)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (IsParallel)
            return ActiveStateSnapshot<TState>.Parallel(
                OwningCompositeState!,
                ToOrderedRegionSnapshots(definition),
                Sequence,
                definitionFingerprint);

        var activeLeaf = ActiveLeafState!;
        var path = definition.GetActiveStatePath(activeLeaf);
        return path.Depth > 1
            ? ActiveStateSnapshot<TState>.Hierarchical(activeLeaf, path, Sequence, definitionFingerprint)
            : ActiveStateSnapshot<TState>.SingleLeaf(activeLeaf, Sequence, definitionFingerprint);
    }

    /// <summary>Creates declaration-ordered public region snapshots for a parallel active shape.</summary>
    public IReadOnlyList<ActiveRegionSnapshot<TState>> ToOrderedRegionSnapshots<TEvent>(
        StateMachineDefinition<TState, TEvent> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!IsParallel) return Array.Empty<ActiveRegionSnapshot<TState>>();

        var entries = ActiveRegions.ToDictionary(region => region.RegionId, StringComparer.Ordinal);
        return definition.GetParallelRegions(OwningCompositeState!)
            .OrderBy(region => region.Order)
            .Where(region => entries.ContainsKey(region.RegionId))
            .Select(region =>
            {
                var entry = entries[region.RegionId];
                return new ActiveRegionSnapshot<TState>(
                    entry.RegionId,
                    entry.RegionName,
                    entry.ActiveLeafState,
                    entry.ActivePath,
                    entry.IsTerminal);
            })
            .ToArray();
    }
}