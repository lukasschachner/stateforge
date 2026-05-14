using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests;

public sealed class RuntimeOverlayIgnoreTests
{
    [Fact]
    public void Renderers_ignore_runtime_overlay_by_default()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateParallelRuntimeOverlayGraph();

        Assert.DoesNotContain("runtime-overlay", MermaidGraphRenderer.Render(graph), StringComparison.Ordinal);
        Assert.DoesNotContain("runtime-overlay", GraphvizDotRenderer.Render(graph), StringComparison.Ordinal);
        Assert.DoesNotContain("runtime-overlay", PlantUmlGraphRenderer.Render(graph), StringComparison.Ordinal);
    }
}
