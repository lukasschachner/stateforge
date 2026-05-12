using StateMachineLibrary.Visualization.Graphviz.Rendering;
using StateMachineLibrary.Visualization.Mermaid.Rendering;
using StateMachineLibrary.Visualization.PlantUML.Rendering;

namespace Visualization.Tests.TestSupport;

public sealed class HistoryRenderingTests
{
    [Fact]
    public void RenderersConsumeHistoryMetadataFromGraphExport()
    {
        var graph = GraphRenderingFixtureFactory.CreateHistoryGraph();

        var mermaid = MermaidGraphRenderer.Render(graph);
        var graphviz = GraphvizDotRenderer.Render(graph);
        var plantUml = PlantUmlGraphRenderer.Render(graph);

        Assert.Contains("hierarchy-history", mermaid, StringComparison.Ordinal);
        Assert.Contains("mode=Shallow", mermaid, StringComparison.Ordinal);
        Assert.Contains("fallback=Legal", mermaid, StringComparison.Ordinal);

        Assert.Contains("hierarchy-history", graphviz, StringComparison.Ordinal);
        Assert.Contains("mode=Shallow", graphviz, StringComparison.Ordinal);
        Assert.Contains("fallback=Legal", graphviz, StringComparison.Ordinal);

        Assert.Contains("hierarchy-history", plantUml, StringComparison.Ordinal);
        Assert.Contains("mode=Shallow", plantUml, StringComparison.Ordinal);
        Assert.Contains("fallback=Legal", plantUml, StringComparison.Ordinal);
    }
}