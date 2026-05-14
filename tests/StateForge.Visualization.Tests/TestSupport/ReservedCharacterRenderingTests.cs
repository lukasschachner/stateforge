using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class ReservedCharacterRenderingTests
{
    [Fact]
    public void ReservedCharactersAreEscapedDeterministically()
    {
        var graph = GraphRenderingFixtureFactory.CreateReservedCharacterGraph();

        var mermaid = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { IncludeMetadata = true });
        var graphviz = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { IncludeMetadata = true });
        var plantUml = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { IncludeMetadata = true });

        Assert.Contains("\\\"A\\\"", mermaid, StringComparison.Ordinal);
        Assert.Contains("\\n", graphviz, StringComparison.Ordinal);
        Assert.Contains("\\[", plantUml, StringComparison.Ordinal);
    }
}