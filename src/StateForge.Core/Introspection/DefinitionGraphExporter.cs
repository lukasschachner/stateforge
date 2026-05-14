using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Core.Validation;

namespace StateForge.Core.Introspection;

/// <summary>Projects reusable machine definitions into dependency-light graph data.</summary>
public static class DefinitionGraphExporter
{
    /// <summary>Exports graph data for a definition when validation has no errors.</summary>
    public static GraphExportResult<TState, TEvent> ExportGraph<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var validation = definition.Validate();
        if (!validation.IsValid) return GraphExportResult<TState, TEvent>.Failure(validation);

        var nodeIds = new Dictionary<StateKey<TState>, string>();
        var nodes = definition.States
            .Select((state, index) =>
            {
                var id = CreateNodeId(index);
                nodeIds[new StateKey<TState>(state.Value)] = id;
                return CreateNode(definition, state, id);
            })
            .ToArray();

        var eventEdges = definition.Transitions
            .Select((transition, index) => CreateEdge(definition, transition, index, nodeIds));
        var completionEdges = definition.CompletionTransitions
            .Select((transition, index) => CreateCompletionEdge(definition, transition,
                definition.Transitions.Count + index, nodeIds));
        var edges = eventEdges.Concat(completionEdges).ToArray();

        var relationships = definition.States
            .Where(s => s.Hierarchy.HasParent && definition.ContainsState(s.Hierarchy.ParentState!))
            .Select(s => new GraphHierarchyRelationship<TState>(
                s.Hierarchy.ParentState!,
                s.Value,
                definition.GetActiveStatePath(s.Value).Depth - 1,
                definition.GetChildren(s.Hierarchy.ParentState!).Select((child, index) => (child, index))
                    .First(x => EqualityComparer<TState>.Default.Equals(x.child.Value, s.Value)).index))
            .ToArray();

        var initialMarkers = definition.States
            .Where(s => s.Hierarchy.HasInitialChild && definition.ContainsState(s.Hierarchy.InitialChildState!))
            .Select(s =>
            {
                var leaf = InitialChildResolver.ResolveTargetLeaf(definition, s.Value);
                var path = definition.GetActiveStatePath(leaf).StatesRootToLeaf
                    .SkipWhile(p => !EqualityComparer<TState>.Default.Equals(p, s.Value)).ToArray();
                return new GraphInitialChildMarker<TState>(s.Value, s.Hierarchy.InitialChildState!, leaf, path);
            })
            .ToArray();

        var historyMarkers = definition.HistoryEnabledStates
            .Select(s =>
            {
                definition.TryGetEffectiveHistoryFallback(s.Value, out var fallback);
                return new GraphHistoryMarker<TState>(s.Value, s.HistoryMode.ToString(), fallback,
                    s.HistoryMode != HistoryMode.Deep ||
                    !validation.Errors.Any(f => f.Code == HierarchyValidationCodes.AmbiguousDeepHistory));
            })
            .ToArray();

        var regions = definition.ParallelRegions
            .Select(r =>
            {
                var historyMode = definition.TryGetHistoryDefinition(r.OwnerCompositeState, out var historyState) &&
                                  definition.IsParallelComposite(r.OwnerCompositeState)
                    ? historyState.HistoryMode.ToString()
                    : HistoryMode.None.ToString();
                var supported = !string.Equals(historyMode, HistoryMode.None.ToString(), StringComparison.Ordinal);
                return new GraphRegionMetadata<TState>(r.OwnerCompositeState, r.RegionId, r.Name, r.Order,
                    r.InitialState, r.TerminalStates, r.MemberStates, ParallelHistoryMode: historyMode,
                    ParallelHistorySupported: supported, ParallelHistoryFallbackState: r.InitialState);
            })
            .ToArray();

        var graph = new DefinitionGraph<TState, TEvent>(
            "definition-graph",
            GetGraphLabel(definition.Metadata),
            nodes,
            edges,
            definition.Metadata,
            validation,
            relationships,
            initialMarkers,
            historyMarkers,
            new GraphHierarchyMetadata(definition.HasHierarchy, relationships.Length, initialMarkers.Length,
                historyMarkers.Length),
            regions);

        return GraphExportResult<TState, TEvent>.Success(graph, validation);
    }

    internal static GraphExportResult<TState, TEvent> ExportGraph<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeShape,
        RuntimeGraphExportOptions? options)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(activeShape);

        var resolvedOptions = options ?? new RuntimeGraphExportOptions();
        resolvedOptions.Validate();

        var export = ExportGraph(definition);
        if (!export.Succeeded || export.Graph is null) return export;

        if (resolvedOptions.OverlayMode == RuntimeGraphOverlayMode.None) return export;

        var overlay = RuntimeGraphOverlayBuilder.Build(definition, export.Graph, activeShape, resolvedOptions);
        var graph = CloneWithRuntimeOverlay(export.Graph, overlay);
        return GraphExportResult<TState, TEvent>.Success(graph, export.Validation);
    }

    private static DefinitionGraph<TState, TEvent> CloneWithRuntimeOverlay<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        GraphActiveStateOverlay<TState>? runtimeOverlay)
    {
        return new DefinitionGraph<TState, TEvent>(
            graph.Id,
            graph.Label,
            graph.Nodes,
            graph.Edges,
            graph.Metadata,
            graph.Validation,
            graph.ParentChildRelationships,
            graph.InitialChildMarkers,
            graph.HistoryMarkers,
            graph.Hierarchy,
            graph.Regions,
            runtimeOverlay);
    }

    internal static string CreateNodeId(int declarationIndex)
    {
        return $"state-{declarationIndex:000}";
    }

    internal static string CreateEdgeId(int declarationIndex)
    {
        return $"transition-{declarationIndex:000}";
    }

    internal static string CreateSafeLabel(object? value)
    {
        var text = value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? "<null>" : text;
    }

    private static GraphNode<TState> CreateNode<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        StateDefinition<TState> state, string id)
    {
        return new GraphNode<TState>(
            id,
            state.Value,
            CreateSafeLabel(state.Value),
            state.IsTerminal,
            state.Metadata,
            state.EntryActions.Select(a => CreateActionSummary(id, a.Summary)),
            state.ExitActions.Select(a => CreateActionSummary(id, a.Summary)),
            definition.IsCompositeState(state.Value),
            definition.IsLeafState(state.Value),
            state.Hierarchy.HasParent,
            state.Hierarchy.ParentState,
            definition.GetChildren(state.Value).Count,
            state.Hierarchy.HasInitialChild,
            state.Hierarchy.InitialChildState,
            state.HasHistory,
            state.HasHistory ? state.HistoryMode.ToString() : null,
            state.HasHistoryFallback,
            state.HistoryFallbackState,
            definition.IsParallelComposite(state.Value),
            definition.TryGetRegionMembership(state.Value, out var membership) ? membership.RegionId : null,
            definition.TryGetRegionMembership(state.Value, out var membershipName) &&
            definition.TryGetParallelRegion(membershipName.RegionId, out var region)
                ? region.Name
                : null,
            definition.TryGetRegionMembership(state.Value, out var membershipOrder) &&
            definition.TryGetParallelRegion(membershipOrder.RegionId, out var regionOrder)
                ? regionOrder.Order
                : null);
    }

    private static GraphEdge<TState, TEvent> CreateEdge<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TransitionDefinition<TState, TEvent> transition,
        int declarationIndex,
        IReadOnlyDictionary<StateKey<TState>, string> nodeIds)
    {
        var sourceNodeId = nodeIds[new StateKey<TState>(transition.SourceState)];
        var targetNodeId = nodeIds[new StateKey<TState>(transition.TargetState)];
        var targetIsComposite = definition.IsCompositeState(transition.TargetState);
        var resolvedTarget = InitialChildResolver.ResolveTargetLeaf(definition, transition.TargetState);
        var regionClassification = ClassifyRegionRelationship(definition, transition.SourceState,
            transition.TargetState, out var sourceRegionId, out var targetRegionId);

        return new GraphEdge<TState, TEvent>(
            CreateEdgeId(declarationIndex),
            sourceNodeId,
            targetNodeId,
            transition.SourceState,
            transition.TargetState,
            CreateSafeLabel(transition.Event.DisplayName),
            CreateEventDescriptor(transition.Event),
            transition.Kind,
            CreateConditionSummary(transition),
            transition.Metadata,
            transition.TransitionActions.Select(a => CreateActionSummary(CreateEdgeId(declarationIndex), a.Summary)),
            definition.IsCompositeState(transition.SourceState),
            targetIsComposite,
            targetIsComposite,
            resolvedTarget,
            ClassifyRelationship(definition, transition.SourceState, resolvedTarget),
            regionClassification,
            sourceRegionId,
            targetRegionId,
            GraphTriggerKind.Event,
            definition.IsParallelComposite(transition.SourceState));
    }

    private static GraphEdge<TState, TEvent> CreateCompletionEdge<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        CompletionTransitionDefinition<TState, TEvent> transition,
        int declarationIndex,
        IReadOnlyDictionary<StateKey<TState>, string> nodeIds)
    {
        var sourceNodeId = nodeIds[new StateKey<TState>(transition.SourceState)];
        var targetNodeId = nodeIds[new StateKey<TState>(transition.TargetState)];
        var targetIsComposite = definition.IsCompositeState(transition.TargetState);
        var resolvedTarget = InitialChildResolver.ResolveTargetLeaf(definition, transition.TargetState);
        var regionClassification = ClassifyRegionRelationship(definition, transition.SourceState,
            transition.TargetState, out var sourceRegionId, out var targetRegionId);

        return new GraphEdge<TState, TEvent>(
            CreateEdgeId(declarationIndex),
            sourceNodeId,
            targetNodeId,
            transition.SourceState,
            transition.TargetState,
            "completion",
            new GraphEventDescriptor<TEvent>("completion", "completion", MetadataCollection.Empty, "Completion"),
            transition.Kind,
            CreateConditionSummary(transition),
            transition.Metadata,
            transition.TransitionActions.Select(a => CreateActionSummary(CreateEdgeId(declarationIndex), a.Summary)),
            definition.IsCompositeState(transition.SourceState),
            targetIsComposite,
            targetIsComposite,
            resolvedTarget,
            ClassifyRelationship(definition, transition.SourceState, resolvedTarget),
            regionClassification,
            sourceRegionId,
            targetRegionId,
            GraphTriggerKind.Completion,
            definition.IsParallelComposite(transition.SourceState));
    }

    private static string ClassifyRegionRelationship<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState source, TState target, out string? sourceRegionId, out string? targetRegionId)
    {
        sourceRegionId = null;
        targetRegionId = null;
        if (!definition.HasParallelRegions) return "None";

        if (definition.TryGetCommonParallelOwner(source, target, out _, out sourceRegionId, out targetRegionId))
            return StringComparer.Ordinal.Equals(sourceRegionId, targetRegionId) ? "Regional" : "InvalidBoundary";

        if (definition.TryGetRegionMembership(source, out var sourceMembership))
        {
            sourceRegionId = sourceMembership.RegionId;
            return "ParentBoundary";
        }

        if (definition.TryGetRegionMembership(target, out var targetMembership))
        {
            targetRegionId = targetMembership.RegionId;
            return "ParentBoundary";
        }

        return "None";
    }

    private static string ClassifyRelationship<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState source, TState target)
    {
        if (!definition.HasHierarchy) return "Flat";

        var comparer = EqualityComparer<TState>.Default;
        var sourcePath = definition.GetActiveStatePath(source).StatesRootToLeaf;
        var targetPath = definition.GetActiveStatePath(target).StatesRootToLeaf;
        if (comparer.Equals(source, target)) return "SameState";
        if (sourcePath.Count > 1 && targetPath.Count > 1 && comparer.Equals(sourcePath[0], targetPath[0]))
            return "SameBranch";
        return "CrossBranch";
    }

    private static GraphActionSummary CreateActionSummary(string ownerId, ActionSummary summary)
    {
        return new GraphActionSummary(ownerId, summary.Kind, summary.DisplayName, summary.Order, summary.Metadata);
    }

    private static GraphEventDescriptor<TEvent> CreateEventDescriptor<TEvent>(EventDefinition<TEvent> eventDefinition)
    {
        return new GraphEventDescriptor<TEvent>(eventDefinition.Identity, eventDefinition.DisplayName,
            eventDefinition.Metadata,
            GetEventCategory(eventDefinition.Identity));
    }

    private static GraphConditionSummary<TState, TEvent> CreateConditionSummary<TState, TEvent>(
        TransitionDefinition<TState, TEvent> transition)
    {
        return CreateConditionSummary(transition.Conditions);
    }

    private static GraphConditionSummary<TState, TEvent> CreateConditionSummary<TState, TEvent>(
        CompletionTransitionDefinition<TState, TEvent> transition)
    {
        return CreateConditionSummary(transition.Conditions);
    }

    private static GraphConditionSummary<TState, TEvent> CreateConditionSummary<TState, TEvent>(
        IReadOnlyList<ConditionDefinition<TState, TEvent>> conditions)
    {
        if (conditions.Count == 0)
            return new GraphConditionSummary<TState, TEvent>(
                GraphConditionSummaryKind.None,
                "No conditions",
                [],
                MetadataCollection.Empty);

        var descriptors = conditions
            .Select((condition, index) =>
                new GraphConditionDescriptor(index, condition.DisplayName, condition.Metadata))
            .ToArray();
        var displayText = string.Join(" and ", descriptors.Select(d => d.DisplayName));

        return new GraphConditionSummary<TState, TEvent>(
            GraphConditionSummaryKind.All,
            displayText,
            descriptors,
            MetadataCollection.Empty);
    }

    private static string GetEventCategory(string identity)
    {
        if (identity.StartsWith("value:", StringComparison.Ordinal)) return "Value";

        if (identity.StartsWith("type:", StringComparison.Ordinal)) return "Type";

        return "Event";
    }

    private static string GetGraphLabel(MetadataCollection metadata)
    {
        return metadata.TryGetValue("title", out var title) && !string.IsNullOrWhiteSpace(title?.ToString())
            ? title!.ToString()!
            : "State machine definition";
    }

    private readonly record struct StateKey<T>(T Value);
}