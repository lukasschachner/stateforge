using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Resolves complete per-region restore shapes for history-enabled parallel composites.</summary>
internal sealed class ParallelHistoryRestorePlanner<TState>
{
    public ParallelHistoryRestorePlan<TState> Plan<TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TState compositeState,
        ParallelHistoryStore<TState> store,
        ActiveStateShape<TState>? preRestoreShape = null)
    {
        if (!definition.TryGetHistoryDefinition(compositeState, out var historyState) ||
            historyState.HistoryMode == HistoryMode.None)
        {
            var shape = ParallelRegionInitialResolver.Enter(definition, compositeState,
                (preRestoreShape?.Sequence ?? 0) + 1);
            return new ParallelHistoryRestorePlan<TState>(compositeState, HistoryMode.None,
                Array.Empty<ParallelRegionRestoreEntry<TState>>(), Array.Empty<ValidationFinding>(), preRestoreShape,
                shape);
        }

        var findings = new List<ValidationFinding>();
        var recorded = store.GetEntries(compositeState).ToDictionary(entry => entry.RegionId, StringComparer.Ordinal);
        var restoreEntries = new List<ParallelRegionRestoreEntry<TState>>();

        foreach (var region in definition.GetParallelRegions(compositeState).OrderBy(r => r.Order))
        {
            if (recorded.TryGetValue(region.RegionId, out var entry))
            {
                var leaf = historyState.HistoryMode == HistoryMode.Deep
                    ? entry.LastActiveLeafState
                    : ResolveShallowLeaf(definition, compositeState, entry);
                var path = definition.GetActiveStatePath(leaf);
                if (!IsInRegion(definition, region, path, leaf))
                {
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        ParallelValidationCodes.InvalidRestorePath,
                        $"Recorded parallel history for region '{region.Name}' does not belong to the region.",
                        $"region:{region.RegionId}", null, leaf));
                    continue;
                }

                restoreEntries.Add(new ParallelRegionRestoreEntry<TState>(region.RegionId, region.Name, region.Order,
                    ParallelHistoryRestoreKind.Recorded, leaf, path, entry.LastUpdatedSequence));
                continue;
            }

            if (!region.HasInitialState || !definition.ContainsState(region.InitialState!))
            {
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.MissingFallback,
                    $"Parallel history fallback for region '{region.Name}' is missing or invalid.",
                    $"region:{region.RegionId}", null, compositeState));
                continue;
            }

            var fallbackLeaf = InitialChildResolver.ResolveTargetLeaf(definition, region.InitialState!);
            var fallbackPath = definition.GetActiveStatePath(fallbackLeaf);
            if (!IsInRegion(definition, region, fallbackPath, fallbackLeaf))
            {
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.InvalidFallback,
                    $"Parallel history fallback for region '{region.Name}' does not resolve inside the region.",
                    $"region:{region.RegionId}", null, fallbackLeaf));
                continue;
            }

            restoreEntries.Add(new ParallelRegionRestoreEntry<TState>(region.RegionId, region.Name, region.Order,
                ParallelHistoryRestoreKind.Fallback, fallbackLeaf, fallbackPath, null));
        }

        var activeEntries = restoreEntries
            .OrderBy(entry => entry.RegionOrder)
            .Select(entry =>
            {
                var region = definition.GetParallelRegions(compositeState).First(r => r.RegionId == entry.RegionId);
                return new ActiveRegionEntry<TState>(entry.RegionId, entry.RegionName, entry.ResolvedLeafState,
                    entry.ResolvedPath,
                    region.TerminalStates.Contains(entry.ResolvedLeafState, EqualityComparer<TState>.Default));
            })
            .ToArray();
        var plannedShape =
            ActiveStateShape<TState>.Parallel(compositeState, activeEntries, (preRestoreShape?.Sequence ?? 0) + 1);
        return new ParallelHistoryRestorePlan<TState>(compositeState, historyState.HistoryMode,
            restoreEntries.OrderBy(entry => entry.RegionOrder).ToArray(), findings, preRestoreShape, plannedShape);
    }

    private static TState ResolveShallowLeaf<TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState compositeState, ParallelRegionHistoryEntry<TState> entry)
    {
        var path = entry.LastActivePath.StatesRootToLeaf;
        var comparer = EqualityComparer<TState>.Default;
        var ownerIndex = path.ToList().FindIndex(state => comparer.Equals(state, compositeState));
        var shallowTarget = ownerIndex >= 0 && ownerIndex + 1 < path.Count
            ? path[ownerIndex + 1]
            : entry.LastActiveLeafState;
        return InitialChildResolver.ResolveTargetLeaf(definition, shallowTarget);
    }

    private static bool IsInRegion<TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ParallelRegionDefinition<TState> region, ActiveStatePath<TState> path, TState leaf)
    {
        if (!definition.TryGetRegionMembership(leaf, out var membership) ||
            !string.Equals(membership.RegionId, region.RegionId, StringComparison.Ordinal)) return false;

        return path.StatesRootToLeaf.Contains(region.OwnerCompositeState, EqualityComparer<TState>.Default);
    }
}