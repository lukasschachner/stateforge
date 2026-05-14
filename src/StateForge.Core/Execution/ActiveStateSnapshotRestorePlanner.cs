using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Execution;

internal sealed record ActiveStateSnapshotRestorePlan<TState>(
    bool IsValid,
    ActiveStateShape<TState>? ActiveStateShape,
    ActiveStateSnapshotValidationResult<TState> ValidationResult);

/// <summary>Plans runtime active-shape materialization from a validated snapshot.</summary>
internal static class ActiveStateSnapshotRestorePlanner
{
    public static ActiveStateSnapshotRestorePlan<TState> Plan<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(snapshot);

        var validation = ActiveStateSnapshotValidator.Validate(definition, snapshot);
        if (!validation.IsValid) return new ActiveStateSnapshotRestorePlan<TState>(false, null, validation);

        var shape = snapshot.Kind switch
        {
            ActiveStateSnapshotKind.SingleLeaf => ActiveStateShape<TState>.Single(snapshot.ActiveLeafState!,
                snapshot.Sequence),
            ActiveStateSnapshotKind.Hierarchical => ActiveStateShape<TState>.Single(snapshot.ActiveLeafState!,
                snapshot.Sequence),
            ActiveStateSnapshotKind.Parallel => RestoreParallel(definition, snapshot),
            _ => throw new InvalidOperationException("Unsupported active-state snapshot kind.")
        };

        return new ActiveStateSnapshotRestorePlan<TState>(true, shape, validation);
    }

    private static ActiveStateShape<TState> RestoreParallel<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot)
    {
        var owner = snapshot.OwningCompositeState!;
        var byId = snapshot.RegionSnapshots.ToDictionary(region => region.RegionId, StringComparer.Ordinal);
        var entries = definition.GetParallelRegions(owner)
            .OrderBy(region => region.Order)
            .Select(region =>
            {
                var snapshotRegion = byId[region.RegionId];
                return new ActiveRegionEntry<TState>(
                    region.RegionId,
                    region.Name,
                    snapshotRegion.ActiveLeafState,
                    snapshotRegion.ActivePath,
                    snapshotRegion.IsTerminal);
            })
            .ToArray();

        return ActiveStateShape<TState>.Parallel(owner, entries, snapshot.Sequence);
    }
}
