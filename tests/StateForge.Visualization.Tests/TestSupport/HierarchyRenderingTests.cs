using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class HierarchyRenderingTests
{
    [Fact]
    public void RenderersConsumeHierarchyMetadataFromGraphExport()
    {
        var graph = GraphRenderingFixtureFactory.CreateHierarchyGraph();

        var mermaid = MermaidGraphRenderer.Render(graph);
        var graphviz = GraphvizDotRenderer.Render(graph);
        var plantUml = PlantUmlGraphRenderer.Render(graph);

        Assert.Contains("hierarchy-parent", mermaid, StringComparison.Ordinal);
        Assert.Contains("hierarchy-initial", mermaid, StringComparison.Ordinal);

        Assert.Contains("hierarchy-parent", graphviz, StringComparison.Ordinal);
        Assert.Contains("hierarchy-initial", graphviz, StringComparison.Ordinal);

        Assert.Contains("hierarchy-parent", plantUml, StringComparison.Ordinal);
        Assert.Contains("hierarchy-initial", plantUml, StringComparison.Ordinal);
    }
}