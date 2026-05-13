using StateMachineLibrary.Visualization.Graphviz.Rendering;
using Visualization.Tests.TestSupport;

namespace Visualization.Tests.Graphviz;

public sealed class GraphvizRuntimeOverlayRenderingTests
{
    [Fact]
    public void GraphvizRenderer_emits_runtime_overlay_comments_when_enabled()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateFlatRuntimeOverlayGraph();

        var rendered = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { RenderRuntimeOverlay = true });

        Assert.Contains("// runtime-overlay: shape=SingleLeaf", rendered, StringComparison.Ordinal);
        Assert.Contains("activeLeafNodeId=state-000", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphvizRenderer_emits_parallel_region_overlay_comments_in_order()
    {
        var graph = RuntimeOverlayGraphFixtureFactory.CreateParallelRuntimeOverlayGraph();

        var rendered = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { RenderRuntimeOverlay = true });

        var fulfillment = rendered.IndexOf("runtime-overlay-region: order=0", StringComparison.Ordinal);
        var billing = rendered.IndexOf("runtime-overlay-region: order=1", StringComparison.Ordinal);
        Assert.True(fulfillment >= 0);
        Assert.True(billing > fulfillment);
    }
}
