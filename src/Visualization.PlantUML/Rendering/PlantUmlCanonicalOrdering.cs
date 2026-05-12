using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Visualization.PlantUML.Rendering;

internal readonly record struct PlantUmlCanonicalNode<TState>(GraphNode<TState> Node, string Identifier);

internal readonly record struct PlantUmlCanonicalEdge<TState, TEvent>(
    GraphEdge<TState, TEvent> Edge,
    string SourceIdentifier,
    string TargetIdentifier);

internal static class PlantUmlCanonicalOrdering
{
    public static IReadOnlyList<PlantUmlCanonicalNode<TState>> Nodes<TState>(IEnumerable<GraphNode<TState>> nodes)
    {
        return nodes.OrderBy(n => n.Id, StringComparer.Ordinal)
            .Select(n => new PlantUmlCanonicalNode<TState>(n, PlantUmlEscaper.EncodeIdentifier(n.Id)))
            .ToArray();
    }

    public static IReadOnlyList<PlantUmlCanonicalEdge<TState, TEvent>> Edges<TState, TEvent>(
        IEnumerable<GraphEdge<TState, TEvent>> edges,
        IReadOnlyDictionary<string, string> nodeIdentifiers)
    {
        return edges.OrderBy(e => e.SourceNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.TargetNodeId, StringComparer.Ordinal)
            .ThenBy(e => e.Id, StringComparer.Ordinal)
            .ThenBy(e => e.Label, StringComparer.Ordinal)
            .Select(e => new PlantUmlCanonicalEdge<TState, TEvent>(
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