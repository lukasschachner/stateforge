using StateMachineLibrary.Visualization.Graphviz.Rendering;
using StateMachineLibrary.Visualization.Mermaid.Rendering;
using StateMachineLibrary.Visualization.PlantUML.Rendering;

namespace Visualization.Tests.TestSupport;

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