using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class EquivalentGraphDeterminismTests
{
    [Fact]
    public void EquivalentGraphWithReorderedCollections_RendersSameTextInAllFormats()
    {
        var first = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var second = GraphRenderingFixtureFactory.CreateEquivalentOrderFlowGraphWithReorderedCollections();

        Assert.Equal(
            TextNormalization.NormalizeForSnapshot(MermaidGraphRenderer.Render(first)),
            TextNormalization.NormalizeForSnapshot(MermaidGraphRenderer.Render(second)));

        Assert.Equal(
            TextNormalization.NormalizeForSnapshot(GraphvizDotRenderer.Render(first)),
            TextNormalization.NormalizeForSnapshot(GraphvizDotRenderer.Render(second)));

        Assert.Equal(
            TextNormalization.NormalizeForSnapshot(PlantUmlGraphRenderer.Render(first)),
            TextNormalization.NormalizeForSnapshot(PlantUmlGraphRenderer.Render(second)));
    }
}