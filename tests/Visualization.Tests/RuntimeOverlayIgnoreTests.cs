using StateMachineLibrary.Visualization.Graphviz.Rendering;
using StateMachineLibrary.Visualization.Mermaid.Rendering;
using StateMachineLibrary.Visualization.PlantUML.Rendering;
using Visualization.Tests.TestSupport;

namespace Visualization.Tests;

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
