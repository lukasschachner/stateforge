using System.Text;
using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Visualization.Mermaid.Rendering;

/// <summary>Renders Core definition graphs as deterministic Mermaid state diagram text.</summary>
public static class MermaidGraphRenderer
{
    /// <summary>Render a Core definition graph as Mermaid text.</summary>
    public static string Render<TState, TEvent>(DefinitionGraph<TState, TEvent> graph,
        MermaidRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var resolvedOptions = options ?? new MermaidRenderOptions();
        resolvedOptions.Validate();

        var orderedNodes = MermaidCanonicalOrdering.Nodes(graph.Nodes);
        var idMap = orderedNodes.ToDictionary(n => n.Node.Id, n => n.Identifier, StringComparer.Ordinal);
        var orderedEdges = MermaidCanonicalOrdering.Edges(graph.Edges, idMap);

        var sb = new StringBuilder();
        var nl = resolvedOptions.NewLine;
        var indent = resolvedOptions.Indentation;

        static string EdgeLabel(GraphEdge<TState, TEvent> edge)
        {
            if (edge.Conditions.Kind == GraphConditionSummaryKind.All && edge.Conditions.Conditions.Count > 0)
                return $"{edge.Label} [{edge.Conditions.DisplayText}]";

            return edge.Label;
        }

        sb.Append("stateDiagram-v2").Append(nl);
        var title = string.IsNullOrWhiteSpace(resolvedOptions.DiagramTitle)
            ? graph.Label
            : resolvedOptions.DiagramTitle!;
        sb.Append(indent).Append("%% ").Append(MermaidEscaper.EscapeComment(title)).Append(nl);
        sb.Append(indent).Append("classDef terminal fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;").Append(nl);

        foreach (var node in orderedNodes)
            sb.Append(indent)
                .Append("state \"")
                .Append(MermaidEscaper.EscapeLabel(node.Node.Label))
                .Append("\" as ")
                .Append(node.Identifier)
                .Append(nl);

        foreach (var edge in orderedEdges)
            sb.Append(indent)
                .Append(edge.SourceIdentifier)
                .Append(" --> ")
                .Append(edge.TargetIdentifier)
                .Append(" : ")
                .Append(MermaidEscaper.EscapeLabel(EdgeLabel(edge.Edge)))
                .Append(nl);

        foreach (var terminal in orderedNodes.Where(n => n.Node.IsTerminal)
                     .OrderBy(n => n.Node.Id, StringComparer.Ordinal))
            sb.Append(indent).Append("class ").Append(terminal.Identifier).Append(" terminal").Append(nl);

        AppendHierarchyLines(graph, orderedNodes, sb, indent, nl);

        if (resolvedOptions.IncludeMetadata) AppendMetadataLines(graph, orderedNodes, orderedEdges, sb, indent, nl);

        return sb.ToString();
    }

    private static void AppendHierarchyLines<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        IReadOnlyList<MermaidCanonicalNode<TState>> nodes,
        StringBuilder sb,
        string indent,
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

        sb.Append(indent)
            .Append("%% hierarchy: hasHierarchy=")
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
            sb.Append(indent)
                .Append("%% hierarchy-parent: ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(relationship.ParentState)))
                .Append(" -> ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(relationship.ChildState)))
                .Append(nl);

        foreach (var marker in graph.InitialChildMarkers
                     .OrderBy(m => StateLabel(m.CompositeState), StringComparer.Ordinal)
                     .ThenBy(m => StateLabel(m.InitialChildState), StringComparer.Ordinal))
            sb.Append(indent)
                .Append("%% hierarchy-initial: ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(marker.CompositeState)))
                .Append(" => ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(marker.InitialChildState)))
                .Append(" (leaf: ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(marker.ResolvedInitialLeafState)))
                .Append(")")
                .Append(nl);

        foreach (var marker in graph.HistoryMarkers
                     .OrderBy(m => StateLabel(m.CompositeState), StringComparer.Ordinal))
            sb.Append(indent)
                .Append("%% hierarchy-history: ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(marker.CompositeState)))
                .Append(" mode=")
                .Append(MermaidEscaper.EscapeComment(marker.HistoryMode))
                .Append(" fallback=")
                .Append(MermaidEscaper.EscapeComment(marker.FallbackTargetState is null
                    ? "<none>"
                    : StateLabel(marker.FallbackTargetState!)))
                .Append(nl);

        foreach (var region in graph.Regions.OrderBy(r => StateLabel(r.CompositeState), StringComparer.Ordinal)
                     .ThenBy(r => r.RegionOrder))
            sb.Append(indent)
                .Append("%% parallel-region: ")
                .Append(MermaidEscaper.EscapeComment(StateLabel(region.CompositeState)))
                .Append("/")
                .Append(MermaidEscaper.EscapeComment(region.RegionName))
                .Append(" initial=")
                .Append(MermaidEscaper.EscapeComment(region.InitialState is null
                    ? "<none>"
                    : StateLabel(region.InitialState)))
                .Append(" members=")
                .Append(region.MemberStates.Count)
                .Append(" history=")
                .Append(MermaidEscaper.EscapeComment(region.ParallelHistoryMode ?? "None"))
                .Append(" fallback=")
                .Append(MermaidEscaper.EscapeComment(region.ParallelHistoryFallbackState is null
                    ? "<none>"
                    : StateLabel(region.ParallelHistoryFallbackState)))
                .Append(nl);
    }

    private static void AppendMetadataLines<TState, TEvent>(
        DefinitionGraph<TState, TEvent> graph,
        IEnumerable<MermaidCanonicalNode<TState>> nodes,
        IEnumerable<MermaidCanonicalEdge<TState, TEvent>> edges,
        StringBuilder sb,
        string indent,
        string nl)
    {
        var graphMetadata = MermaidMetadataFormatter.Format(graph.Metadata);
        if (graphMetadata.Length > 0)
            sb.Append(indent).Append("%% graph-metadata: ").Append(MermaidEscaper.EscapeComment(graphMetadata))
                .Append(nl);

        foreach (var node in nodes)
        {
            var metadata = MermaidMetadataFormatter.Format(node.Node.Metadata);
            var actions = MermaidMetadataFormatter.FormatActions(node.Node.ExitActions.Concat(node.Node.EntryActions));
            if (metadata.Length > 0 || actions.Length > 0)
            {
                var parts = new List<string>();
                if (metadata.Length > 0) parts.Add(metadata);
                if (actions.Length > 0) parts.Add("actions=" + actions);
                sb.Append(indent)
                    .Append("%% node-metadata(")
                    .Append(node.Identifier)
                    .Append("): ")
                    .Append(MermaidEscaper.EscapeComment(string.Join(" | ", parts)))
                    .Append(nl);
            }
        }

        foreach (var edge in edges)
        {
            var parts = new List<string>();
            var edgeMetadata = MermaidMetadataFormatter.Format(edge.Edge.Metadata);
            if (edgeMetadata.Length > 0) parts.Add("edge=" + edgeMetadata);

            var eventMetadata = MermaidMetadataFormatter.Format(edge.Edge.Event.Metadata);
            if (eventMetadata.Length > 0) parts.Add("event=" + eventMetadata);

            var conditionMetadata = MermaidMetadataFormatter.Format(edge.Edge.Conditions.Metadata);
            if (conditionMetadata.Length > 0) parts.Add("conditions=" + conditionMetadata);

            var actionMetadata = MermaidMetadataFormatter.FormatActions(edge.Edge.TransitionActions);
            if (actionMetadata.Length > 0) parts.Add("actions=" + actionMetadata);

            if (parts.Count > 0)
                sb.Append(indent)
                    .Append("%% edge-metadata(")
                    .Append(edge.Edge.Id)
                    .Append("): ")
                    .Append(MermaidEscaper.EscapeComment(string.Join(" | ", parts)))
                    .Append(nl);
        }
    }
}