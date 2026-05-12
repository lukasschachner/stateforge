using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Visualization.Mermaid.Rendering;

internal readonly record struct MermaidCanonicalNode<TState>(GraphNode<TState> Node, string Identifier);

internal readonly record struct MermaidCanonicalEdge<TState, TEvent>(
    GraphEdge<TState, TEvent> Edge,
    string SourceIdentifier,
    string TargetIdentifier);

internal static class MermaidCanonicalOrdering
{
    public static IReadOnlyList<MermaidCanonicalNode<TState>> Nodes<TState>(IEnumerable<GraphNode<TState>> nodes)
    {
        return nodes.OrderBy(n => n.Id, StringComparer.Ordinal)
            .Select(n => new MermaidCanonicalNode<TState>(n, MermaidEscaper.EncodeIdentifier(n.Id)))
            .ToArray();
    }

    public static IReadOnlyList<MermaidCanonicalEdge<TState, TEvent>> Edges<TState, TEvent>(
        IEnumerable<GraphEdge<TState, TEvent>> edges,
        IReadOnlyDictionary<string, string> nodeIdentifiers)
    {
        return edges.OrderBy(e => e.SourceNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.TargetNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.Id, StringComparer.Ordinal)
            .ThenBy(e => e.Label, StringComparer.Ordinal)
            .Select(e => new MermaidCanonicalEdge<TState, TEvent>(
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