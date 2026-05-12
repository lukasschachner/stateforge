using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal enum HistoryRestoreKind
{
    None,
    Recorded,
    Fallback
}

internal sealed record HistoryResolution<TState>(
    TState ResolvedTargetLeafState,
    TState DirectChildState,
    HistoryRestoreKind RestoreKind,
    bool UsedHistory,
    ParallelHistoryRestorePlan<TState>? ParallelRestorePlan = null);

internal static class HistoryTargetResolver
{
    public static HistoryResolution<TState> Resolve<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TState compositeState,
        IDictionary<TState, CompositeHistoryRecord<TState>>? records,
        ParallelHistoryStore<TState>? parallelHistoryStore = null,
        ActiveStateShape<TState>? preRestoreShape = null)
    {
        if (!definition.TryGetHistoryDefinition(compositeState, out var historyState))
        {
            var initialLeaf = InitialChildResolver.ResolveTargetLeaf(definition, compositeState);
            return new HistoryResolution<TState>(initialLeaf, initialLeaf, HistoryRestoreKind.None, false);
        }

        if (definition.IsParallelComposite(compositeState) && historyState.HistoryMode != HistoryMode.None)
        {
            if (parallelHistoryStore is not null)
            {
                var plan = new ParallelHistoryRestorePlanner<TState>().Plan(definition, compositeState,
                    parallelHistoryStore, preRestoreShape);
                if (plan.IsValid && plan.RestoreEntries.Count > 0)
                {
                    var first = plan.RestoreEntries.OrderBy(e => e.RegionOrder).First();
                    var kind = first.RestoreKind == ParallelHistoryRestoreKind.Recorded
                        ? HistoryRestoreKind.Recorded
                        : HistoryRestoreKind.Fallback;
                    return new HistoryResolution<TState>(first.ResolvedLeafState, first.ResolvedLeafState, kind, true,
                        plan);
                }
            }

            var fallbackLeaf = ParallelRegionInitialResolver.FirstLeaf(definition, compositeState);
            return new HistoryResolution<TState>(fallbackLeaf, fallbackLeaf, HistoryRestoreKind.Fallback, true);
        }

        if (records is not null && records.TryGetValue(compositeState, out var record))
        {
            var target = historyState.HistoryMode == HistoryMode.Deep &&
                         record.LastActiveDescendantLeafState is not null
                ? record.LastActiveDescendantLeafState
                : record.LastActiveDirectChildState;
            var resolved = InitialChildResolver.ResolveTargetLeaf(definition, target!);
            return new HistoryResolution<TState>(resolved, record.LastActiveDirectChildState,
                HistoryRestoreKind.Recorded, true);
        }

        if (definition.TryGetEffectiveHistoryFallback(compositeState, out var fallback))
        {
            var resolved = InitialChildResolver.ResolveTargetLeaf(definition, fallback);
            return new HistoryResolution<TState>(resolved, fallback, HistoryRestoreKind.Fallback, true);
        }

        var leaf = InitialChildResolver.ResolveTargetLeaf(definition, compositeState);
        return new HistoryResolution<TState>(leaf, leaf, HistoryRestoreKind.Fallback, true);
    }
}