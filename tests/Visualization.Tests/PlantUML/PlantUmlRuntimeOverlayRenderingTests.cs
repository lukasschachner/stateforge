using StateMachineLibrary.Visualization.PlantUML.Rendering;
using Visualization.Tests.TestSupport;

namespace Visualization.Tests.PlantUML;

public sealed class PlantUmlRuntimeOverlayRenderingTests
{
    [Fact]
    public void PlantUmlRenderer_emits_runtime_overlay_comments_when_enabled()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateHierarchyRuntimeOverlayGraph();

        var rendered = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { RenderRuntimeOverlay = true });

        Assert.Contains("' runtime-overlay: shape=Hierarchical", rendered, StringComparison.Ordinal);
        Assert.Contains("activePath=state-000,state-001", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void PlantUmlRenderer_emits_parallel_region_overlay_comments_in_order()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateParallelRuntimeOverlayGraph();

        var rendered = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { RenderRuntimeOverlay = true });

        var fulfillment = rendered.IndexOf("runtime-overlay-region: order=0", StringComparison.Ordinal);
        var billing = rendered.IndexOf("runtime-overlay-region: order=1", StringComparison.Ordinal);
        Assert.True(fulfillment >= 0);
        Assert.True(billing > fulfillment);
    }
}
