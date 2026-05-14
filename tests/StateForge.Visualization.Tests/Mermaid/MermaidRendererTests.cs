using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests.Mermaid;

public sealed class MermaidRendererTests
{
    [Fact]
    public void MermaidRenderer_EmitsExpectedStructure()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = MermaidGraphRenderer.Render(graph);

        Assert.StartsWith("stateDiagram-v2", rendered, StringComparison.Ordinal);
        RendererContractTests.AssertIncludesAllNodesAndEdges(graph, rendered);
        Assert.Contains("classDef terminal", rendered, StringComparison.Ordinal);
        Assert.Contains("class n_", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void MermaidRenderer_ThrowsForInvalidGraphReferences()
    {
        var graph = GraphRenderingFixtureFactory.CreateInvalidEdgeGraph();
        RendererContractTests.AssertFailsOnUnknownNodeReference(() => MermaidGraphRenderer.Render(graph));
    }
}