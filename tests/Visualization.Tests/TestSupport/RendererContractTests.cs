using StateMachineLibrary.Core.Introspection;

namespace Visualization.Tests.TestSupport;

internal static class RendererContractTests
{
    public static void AssertIncludesAllNodesAndEdges<TState, TEvent>(DefinitionGraph<TState, TEvent> graph,
        string rendered)
    {
        foreach (var node in graph.Nodes) Assert.Contains(node.Label, rendered, StringComparison.Ordinal);

        foreach (var edge in graph.Edges) Assert.Contains(edge.Label, rendered, StringComparison.Ordinal);
    }

    public static void AssertFailsOnUnknownNodeReference(Action render)
    {
        var exception = Assert.ThrowsAny<InvalidOperationException>(render);
        Assert.Contains("references unknown", exception.Message, StringComparison.Ordinal);
    }
}