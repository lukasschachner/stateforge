using System.Text;
using StateForge.Core.Introspection;

namespace StateForge.Visualization.PlantUML.Rendering;

/// <summary>Renders Core definition graphs as deterministic PlantUML state diagram text.</summary>
public static class PlantUmlGraphRenderer
{
    /// <summary>Render a Core definition graph as PlantUML text.</summary>
    public static string Render<TState, TEvent>(DefinitionGraph<TState, TEvent> graph,
        PlantUmlRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var resolvedOptions = options ?? new PlantUmlRenderOptions();
        resolvedOptions.Validate();

        var orderedNodes = PlantUmlCanonicalOrdering.Nodes(graph.Nodes);
        var idMap = orderedNodes.ToDictionary(n => n.Node.Id, n => n.Identifier, StringComparer.Ordinal);
        var orderedEdges = PlantUmlCanonicalOrdering.Edges(graph.Edges, idMap);

        var sb = new StringBuilder();
        var nl = resolvedOptions.NewLine;
        var indent = resolvedOptions.Indentation;

        static string EdgeLabel(GraphEdge<TState, TEvent> edge)
        {
            if (edge.Conditions.Kind == GraphConditionSummaryKind.All && edge.Conditions.Conditions.Count > 0)
                return $"{edge.Label} [{edge.Conditions.DisplayText}]";

            return edge.Label;
        }

        var title = string.IsNullOrWhiteSpace(resolvedOptions.DiagramTitle)
            ? graph.Label
            : resolvedOptions.DiagramTitle!;
        sb.Append("@startuml").Append(nl);
        sb.Append("title ").Append(PlantUmlEscaper.EscapeLabel(title)).Append(nl);
        sb.Append("skinparam state<<terminal>> {").Append(nl);
        sb.Append(indent).Append("BackgroundColor #E8F5E9").Append(nl);
        sb.Append(indent).Append("BorderColor #2E7D32").Append(nl);
        sb.Append('}').Append(nl);

        foreach (var node in orderedNodes)
        {
            sb.Append("state \"")
                .Append(PlantUmlEscaper.EscapeLabel(node.Node.Label))
                .Append("\" as ")
                .Append(node.Identifier);
            if (node.Node.IsTerminal) sb.Append(" <<terminal>>");

            sb.Append(nl);
        }

        foreach (var edge in orderedEdges)
            sb.Append(edge.SourceIdentifier)
                .Append(" --> ")
                .Append(edge.TargetIdentifier)
                .Append(" : ")
                .Append(PlantUmlEscaper.EscapeLabel(EdgeLabel(edge.Edge)))
                .Append(nl);

        AppendHierarchyLines(graph, orderedNodes, sb, nl);

        if (resolvedOptions.RenderRuntimeOverlay)
            AppendRuntimeOverlayLines(graph, sb, nl);

        if (resolvedOptions.IncludeMetadata) AppendMetadataLines(graph, orderedNodes, orderedEdges, sb, nl);

        sb.Append("@enduml").Append(nl);
        return sb.ToString();
    }

    private static void AppendHierarchyLines<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        IReadOnlyList<PlantUmlCanonicalNode<TState>> nodes,
        StringBuilder sb,
        string nl)
    {
        if (!graph.Hierarchy.HasHierarchy && graph.ParentChildRelationships.Count == 0 &&
            graph.InitialChildMarkers.Count == 0 && graph.HistoryMarkers.Count == 0 && graph.Regions.Count == 0) return;

        var comparer = EqualityComparer<TState>.Default;

        string StateLabel(TState state)
        {
            foreach (var node in nodes)
                if (comparer.Equals(node.Node.State, state))
                    return node.Node.Label;

            return state?.ToString() ?? "<null>";
        }

        sb.Append("' hierarchy: hasHierarchy=")
            .Append(graph.Hierarchy.HasHierarchy ? "true" : "false")
            .Append(" relationships=")
            .Append(graph.ParentChildRelationships.Count)
            .Append(" initialChildren=")
            .Append(graph.InitialChildMarkers.Count)
            .Append(" historyMarkers=")
            .Append(graph.HistoryMarkers.Count)
            .Append(" parallelRegions=")
            .Append(graph.Regions.Count)
            .Append(nl);

        foreach (var relationship in graph.ParentChildRelationships
                     .OrderBy(r => StateLabel(r.ParentState), StringComparer.Ordinal)
                     .ThenBy(r => StateLabel(r.ChildState), StringComparer.Ordinal)
                     .ThenBy(r => r.Depth)
                     .ThenBy(r => r.SiblingOrder))
            sb.Append("' hierarchy-parent: ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(relationship.ParentState)))
                .Append(" -> ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(relationship.ChildState)))
                .Append(nl);

        foreach (var marker in graph.InitialChildMarkers
                     .OrderBy(m => StateLabel(m.CompositeState), StringComparer.Ordinal)
                     .ThenBy(m => StateLabel(m.InitialChildState), StringComparer.Ordinal))
            sb.Append("' hierarchy-initial: ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(marker.CompositeState)))
                .Append(" => ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(marker.InitialChildState)))
                .Append(" (leaf: ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(marker.ResolvedInitialLeafState)))
                .Append(")")
                .Append(nl);

        foreach (var marker in graph.HistoryMarkers
                     .OrderBy(m => StateLabel(m.CompositeState), StringComparer.Ordinal))
            sb.Append("' hierarchy-history: ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(marker.CompositeState)))
                .Append(" mode=")
                .Append(PlantUmlEscaper.EscapeComment(marker.HistoryMode))
                .Append(" fallback=")
                .Append(PlantUmlEscaper.EscapeComment(marker.FallbackTargetState is null
                    ? "<none>"
                    : StateLabel(marker.FallbackTargetState!)))
                .Append(nl);

        foreach (var region in graph.Regions.OrderBy(r => StateLabel(r.CompositeState), StringComparer.Ordinal)
                     .ThenBy(r => r.RegionOrder))
            sb.Append("' parallel-region: ")
                .Append(PlantUmlEscaper.EscapeComment(StateLabel(region.CompositeState)))
                .Append("/")
                .Append(PlantUmlEscaper.EscapeComment(region.RegionName))
                .Append(" initial=")
                .Append(PlantUmlEscaper.EscapeComment(region.InitialState is null
                    ? "<none>"
                    : StateLabel(region.InitialState)))
                .Append(" members=")
                .Append(region.MemberStates.Count)
                .Append(" history=")
                .Append(PlantUmlEscaper.EscapeComment(region.ParallelHistoryMode ?? "None"))
                .Append(" fallback=")
                .Append(PlantUmlEscaper.EscapeComment(region.ParallelHistoryFallbackState is null
                    ? "<none>"
                    : StateLabel(region.ParallelHistoryFallbackState)))
                .Append(nl);
    }

    private static void AppendRuntimeOverlayLines<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        StringBuilder sb,
        string nl)
    {
        var overlay = graph.RuntimeOverlay;
        if (overlay is null) return;

        sb.Append("' runtime-overlay: shape=")
            .Append(overlay.ShapeKind)
            .Append(" sequence=")
            .Append(overlay.Sequence)
            .Append(" activeLeafNodeId=")
            .Append(PlantUmlEscaper.EscapeComment(overlay.ActiveLeafNodeId ?? ""))
            .Append(" activePath=")
            .Append(PlantUmlEscaper.EscapeComment(string.Join(",", overlay.ActivePathNodeIds)))
            .Append(" complete=")
            .Append(overlay.IsComplete ? "true" : "false")
            .Append(nl);

        foreach (var region in overlay.Regions.OrderBy(region => region.RegionOrder))
            sb.Append("' runtime-overlay-region: order=")
                .Append(region.RegionOrder)
                .Append(" id=")
                .Append(PlantUmlEscaper.EscapeComment(region.RegionId))
                .Append(" name=")
                .Append(PlantUmlEscaper.EscapeComment(region.RegionName ?? ""))
                .Append(" activeLeafNodeId=")
                .Append(PlantUmlEscaper.EscapeComment(region.ActiveLeafNodeId ?? ""))
                .Append(" activePath=")
                .Append(PlantUmlEscaper.EscapeComment(string.Join(",", region.ActivePathNodeIds)))
                .Append(" terminal=")
                .Append(region.IsTerminal ? "true" : "false")
                .Append(" complete=")
                .Append(region.IsComplete ? "true" : "false")
                .Append(nl);
    }

    private static void AppendMetadataLines<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        IEnumerable<PlantUmlCanonicalNode<TState>> nodes,
        IEnumerable<PlantUmlCanonicalEdge<TState, TEvent>> edges,
        StringBuilder sb,
        string nl)
    {
        var graphMetadata = PlantUmlMetadataFormatter.Format(graph.Metadata);
        if (graphMetadata.Length > 0)
            sb.Append("' graph-metadata: ").Append(PlantUmlEscaper.EscapeComment(graphMetadata)).Append(nl);

        foreach (var node in nodes)
        {
            var metadata = PlantUmlMetadataFormatter.Format(node.Node.Metadata);
            var actions = PlantUmlMetadataFormatter.FormatActions(node.Node.ExitActions.Concat(node.Node.EntryActions));
            if (metadata.Length > 0 || actions.Length > 0)
            {
                var parts = new List<string>();
                if (metadata.Length > 0) parts.Add(metadata);
                if (actions.Length > 0) parts.Add("actions=" + actions);
                sb.Append("' node-metadata(")
                    .Append(node.Identifier)
                    .Append("): ")
                    .Append(PlantUmlEscaper.EscapeComment(string.Join(" | ", parts)))
                    .Append(nl);
            }
        }

        foreach (var edge in edges)
        {
            var parts = new List<string>();
            var edgeMetadata = PlantUmlMetadataFormatter.Format(edge.Edge.Metadata);
            if (edgeMetadata.Length > 0) parts.Add("edge=" + edgeMetadata);

            var eventMetadata = PlantUmlMetadataFormatter.Format(edge.Edge.Event.Metadata);
            if (eventMetadata.Length > 0) parts.Add("event=" + eventMetadata);

            var conditionMetadata = PlantUmlMetadataFormatter.Format(edge.Edge.Conditions.Metadata);
            if (conditionMetadata.Length > 0) parts.Add("conditions=" + conditionMetadata);

            var actionMetadata = PlantUmlMetadataFormatter.FormatActions(edge.Edge.TransitionActions);
            if (actionMetadata.Length > 0) parts.Add("actions=" + actionMetadata);

            if (parts.Count > 0)
                sb.Append("' edge-metadata(")
                    .Append(edge.Edge.Id)
                    .Append("): ")
                    .Append(PlantUmlEscaper.EscapeComment(string.Join(" | ", parts)))
                    .Append(nl);
        }
    }
}