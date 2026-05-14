using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public sealed class MetadataOrderingTests
{
    [Fact]
    public void MetadataKeysAreOrderedDeterministically()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();

        var mermaid = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { IncludeMetadata = true });
        var graphviz = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { IncludeMetadata = true });
        var plantUml = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { IncludeMetadata = true });

        AssertOrder(mermaid, "alpha", "beta", "zeta");
        AssertOrder(graphviz, "alpha", "beta", "zeta");
        AssertOrder(plantUml, "alpha", "beta", "zeta");
    }

    private static void AssertOrder(string rendered, params string[] orderedKeys)
    {
        var previousIndex = -1;
        foreach (var key in orderedKeys)
        {
            var index = rendered.IndexOf(key, StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Expected metadata key '{key}' to appear in deterministic order.");
            previousIndex = index;
        }
    }
}