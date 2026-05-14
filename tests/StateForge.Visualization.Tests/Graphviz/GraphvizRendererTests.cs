using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests.Graphviz;

public sealed class GraphvizRendererTests
{
    [Fact]
    public void GraphvizRenderer_EmitsExpectedStructure()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = GraphvizDotRenderer.Render(graph);

        Assert.StartsWith("digraph", rendered, StringComparison.Ordinal);
        RendererContractTests.AssertIncludesAllNodesAndEdges(graph, rendered);
        Assert.Contains("shape=doublecircle", rendered, StringComparison.Ordinal);
        Assert.Contains(" -> ", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphvizRenderer_ThrowsForInvalidGraphReferences()
    {
        var graph = GraphRenderingFixtureFactory.CreateInvalidEdgeGraph();
        RendererContractTests.AssertFailsOnUnknownNodeReference(() => GraphvizDotRenderer.Render(graph));
    }
}