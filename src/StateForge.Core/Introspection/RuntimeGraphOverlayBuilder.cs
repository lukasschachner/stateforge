using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Core.Validation;

namespace StateForge.Core.Introspection;

internal static class RuntimeGraphOverlayBuilder
{
    public static GraphActiveStateOverlay<TState> Build<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        DefinitionGraph<TState, TEvent> graph,
        ActiveStateShape<TState> activeShape,
        RuntimeGraphExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(activeShape);
        ArgumentNullException.ThrowIfNull(options);

        if (options.ValidateActiveShape)
        {
            var snapshot = activeShape.ToActiveStateSnapshot(definition);
            var validation = ActiveStateSnapshotValidator.Validate(definition, snapshot);
            if (!validation.IsValid) throw new ActiveStateSnapshotValidationException<TState>(validation);
        }

        return activeShape.IsParallel
            ? BuildParallelOverlay(definition, graph, activeShape)
            : BuildSingleOverlay(definition, graph, activeShape);
    }

    private static GraphActiveStateOverlay<TState> BuildSingleOverlay<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        DefinitionGraph<TState, TEvent> graph,
        ActiveStateShape<TState> activeShape)
    {
        var activeLeaf = activeShape.ActiveLeafState!;
        var path = definition.GetActiveStatePath(activeLeaf).StatesRootToLeaf;
        var includePath = path.Count > 1;
        var completion = HierarchyCompletionEvaluator.Evaluate(definition, activeShape);
        var isTerminal = definition.FindState(activeLeaf)?.IsTerminal == true;

        return new GraphActiveStateOverlay<TState>(
            includePath ? GraphActiveStateOverlayKind.Hierarchical : GraphActiveStateOverlayKind.SingleLeaf,
            activeShape.Sequence,
            activeLeaf,
            ResolveNodeId(graph, activeLeaf),
            includePath ? path : [],
            includePath ? ResolveNodeIds(graph, path) : [],
            isTerminal: isTerminal,
            isComplete: completion is not null,
            completionScopeState: completion is null ? default : completion.CompletionScopeState);
    }

    private static GraphActiveStateOverlay<TState> BuildParallelOverlay<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        DefinitionGraph<TState, TEvent> graph,
        ActiveStateShape<TState> activeShape)
    {
        var owner = activeShape.OwningCompositeState!;
        var entries = activeShape.ActiveRegions.ToDictionary(region => region.RegionId, StringComparer.Ordinal);
        var regions = definition.GetParallelRegions(owner)
            .OrderBy(region => region.Order)
            .Where(region => entries.ContainsKey(region.RegionId))
            .Select(region =>
            {
                var entry = entries[region.RegionId];
                var path = entry.ActivePath.StatesRootToLeaf;
                return new GraphActiveRegionOverlay<TState>(
                    entry.RegionId,
                    entry.RegionName,
                    region.Order,
                    entry.ActiveLeafState,
                    ResolveNodeId(graph, entry.ActiveLeafState),
                    path,
                    ResolveNodeIds(graph, path),
                    entry.IsTerminal,
                    entry.IsTerminal,
                    activeShape.Sequence);
            })
            .ToArray();
        var isComplete = ParallelCompletionEvaluator.IsComplete(definition, activeShape);
        var completedRegionIds = regions.Where(region => region.IsComplete).Select(region => region.RegionId).ToArray();

        return new GraphActiveStateOverlay<TState>(
            GraphActiveStateOverlayKind.Parallel,
            activeShape.Sequence,
            owningCompositeState: owner,
            owningCompositeNodeId: ResolveNodeId(graph, owner),
            isTerminal: isComplete,
            isComplete: isComplete,
            completionScopeState: isComplete ? owner : default,
            completedRegionIds: completedRegionIds,
            regions: regions);
    }

    private static IReadOnlyList<string> ResolveNodeIds<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        IEnumerable<TState> states)
    {
        return states.Select(state => ResolveNodeId(graph, state)).Where(id => id is not null).Cast<string>().ToArray();
    }

    private static string? ResolveNodeId<TState, TEvent>(DefinitionGraph<TState, TEvent> graph, TState state)
    {
        var comparer = EqualityComparer<TState>.Default;
        foreach (var node in graph.Nodes)
            if (comparer.Equals(node.State, state))
                return node.Id;

        return null;
    }
}
