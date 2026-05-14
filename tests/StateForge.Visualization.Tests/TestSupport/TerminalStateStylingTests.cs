using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class TerminalStateStylingTests
{
    [Fact]
    public void TerminalStatesAreStyledAcrossAllFormats()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();

        var mermaid = MermaidGraphRenderer.Render(graph);
        var graphviz = GraphvizDotRenderer.Render(graph);
        var plantUml = PlantUmlGraphRenderer.Render(graph);

        Assert.Contains("classDef terminal", mermaid, StringComparison.Ordinal);
        Assert.Contains("shape=doublecircle", graphviz, StringComparison.Ordinal);
        Assert.Contains("<<terminal>>", plantUml, StringComparison.Ordinal);
    }
}