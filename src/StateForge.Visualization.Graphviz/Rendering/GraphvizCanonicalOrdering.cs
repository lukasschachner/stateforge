using StateForge.Core.Introspection;

namespace StateForge.Visualization.Graphviz.Rendering;

internal readonly record struct GraphvizCanonicalNode<TState>(GraphNode<TState> Node, string Identifier);

internal readonly record struct GraphvizCanonicalEdge<TState, TEvent>(
    GraphEdge<TState, TEvent> Edge,
    string SourceIdentifier,
    string TargetIdentifier);

internal static class GraphvizCanonicalOrdering
{
    public static IReadOnlyList<GraphvizCanonicalNode<TState>> Nodes<TState>(IEnumerable<GraphNode<TState>> nodes)
    {
        return nodes.OrderBy(n => n.Id, StringComparer.Ordinal)
            .Select(n => new GraphvizCanonicalNode<TState>(n, GraphvizEscaper.EncodeIdentifier(n.Id)))
            .ToArray();
    }

    public static IReadOnlyList<GraphvizCanonicalEdge<TState, TEvent>> Edges<TState, TEvent>(
        IEnumerable<GraphEdge<TState, TEvent>> edges,
        IReadOnlyDictionary<string, string> nodeIdentifiers)
    {
        return edges.OrderBy(e => e.SourceNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.TargetNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.Id, StringComparer.Ordinal)
            .ThenBy(e => e.Label, StringComparer.Ordinal)
            .Select(e => new GraphvizCanonicalEdge<TState, TEvent>(
                e,
                nodeIdentifiers.TryGetValue(e.SourceNodeId, out var source)
                    ? source
                    : throw new InvalidOperationException(
                        $"Graph edge '{e.Id}' references unknown source node '{e.SourceNodeId}'."),
                nodeIdentifiers.TryGetValue(e.TargetNodeId, out var target)
                    ? target
                    : throw new InvalidOperationException(
                        $"Graph edge '{e.Id}' references unknown target node '{e.TargetNodeId}'.")))
            .ToArray();
    }
}