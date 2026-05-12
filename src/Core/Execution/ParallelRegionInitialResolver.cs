using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal static class ParallelRegionInitialResolver
{
    public static ActiveStateShape<TState> Enter<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState compositeState, long sequence = 0)
    {
        var entries = definition.GetParallelRegions(compositeState)
            .OrderBy(r => r.Order)
            .Select(region =>
            {
                var initial = region.HasInitialState ? region.InitialState! : default!;
                var leaf = InitialChildResolver.ResolveTargetLeaf(definition, initial);
                return new ActiveRegionEntry<TState>(region.RegionId, region.Name, leaf,
                    definition.GetActiveStatePath(leaf),
                    region.TerminalStates.Contains(leaf, EqualityComparer<TState>.Default));
            })
            .ToArray();
        return ActiveStateShape<TState>.Parallel(compositeState, entries, sequence);
    }

    public static TState FirstLeaf<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState compositeState)
    {
        var first = definition.GetParallelRegions(compositeState).OrderBy(r => r.Order).FirstOrDefault();
        if (first is null || !first.HasInitialState) return compositeState;
        return InitialChildResolver.ResolveTargetLeaf(definition, first.InitialState!);
    }
}