using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests.Mermaid;

public sealed class MermaidRuntimeOverlayRenderingTests
{
    [Fact]
    public void MermaidRenderer_emits_runtime_overlay_comments_and_classes_when_enabled()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateHierarchyRuntimeOverlayGraph();

        var rendered = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { RenderRuntimeOverlay = true });

        Assert.Contains("%% runtime-overlay: shape=Hierarchical", rendered, StringComparison.Ordinal);
        Assert.Contains("classDef runtimeActive", rendered, StringComparison.Ordinal);
        Assert.Contains("classDef runtimePath", rendered, StringComparison.Ordinal);
        Assert.Contains("class n_", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void MermaidRenderer_emits_parallel_region_overlay_comments_in_order()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateParallelRuntimeOverlayGraph();

        var rendered = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { RenderRuntimeOverlay = true });

        var fulfillment = rendered.IndexOf("runtime-overlay-region: order=0", StringComparison.Ordinal);
        var billing = rendered.IndexOf("runtime-overlay-region: order=1", StringComparison.Ordinal);
        Assert.True(fulfillment >= 0);
        Assert.True(billing > fulfillment);
    }
}
