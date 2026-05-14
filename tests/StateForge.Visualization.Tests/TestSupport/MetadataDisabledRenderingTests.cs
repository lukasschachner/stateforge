using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class MetadataDisabledRenderingTests
{
    [Fact]
    public void MetadataIsHiddenByDefault()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();

        var mermaid = MermaidGraphRenderer.Render(graph);
        var graphviz = GraphvizDotRenderer.Render(graph);
        var plantUml = PlantUmlGraphRenderer.Render(graph);

        Assert.DoesNotContain("graph-metadata", mermaid, StringComparison.Ordinal);
        Assert.DoesNotContain("graph-metadata", graphviz, StringComparison.Ordinal);
        Assert.DoesNotContain("graph-metadata", plantUml, StringComparison.Ordinal);
    }
}