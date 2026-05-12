using StateMachineLibrary.Visualization.Graphviz.Rendering;
using StateMachineLibrary.Visualization.Mermaid.Rendering;
using StateMachineLibrary.Visualization.PlantUML.Rendering;

namespace Visualization.Tests.TestSupport;

public sealed class MetadataEnabledRenderingTests
{
    [Fact]
    public void MetadataIsEmittedWhenEnabled()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();

        var mermaid = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { IncludeMetadata = true });
        var graphviz = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { IncludeMetadata = true });
        var plantUml = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { IncludeMetadata = true });

        Assert.Contains("graph-metadata", mermaid, StringComparison.Ordinal);
        Assert.Contains("graph-metadata", graphviz, StringComparison.Ordinal);
        Assert.Contains("graph-metadata", plantUml, StringComparison.Ordinal);
    }
}