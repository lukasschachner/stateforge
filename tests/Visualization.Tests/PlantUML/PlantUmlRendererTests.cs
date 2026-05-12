using StateMachineLibrary.Visualization.PlantUML.Rendering;
using Visualization.Tests.TestSupport;

namespace Visualization.Tests.PlantUML;

public sealed class PlantUmlRendererTests
{
    [Fact]
    public void PlantUmlRenderer_EmitsExpectedStructure()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = PlantUmlGraphRenderer.Render(graph);

        Assert.StartsWith("@startuml", rendered, StringComparison.Ordinal);
        Assert.Contains("@enduml", rendered, StringComparison.Ordinal);
        RendererContractTests.AssertIncludesAllNodesAndEdges(graph, rendered);
        Assert.Contains("<<terminal>>", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void PlantUmlRenderer_ThrowsForInvalidGraphReferences()
    {
        var graph = GraphRenderingFixtureFactory.CreateInvalidEdgeGraph();
        RendererContractTests.AssertFailsOnUnknownNodeReference(() => PlantUmlGraphRenderer.Render(graph));
    }
}