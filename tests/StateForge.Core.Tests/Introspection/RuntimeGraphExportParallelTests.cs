using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public sealed class RuntimeGraphExportParallelTests
{
    [Fact]
    public void ExportGraph_marks_parallel_regions_in_overlay()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateParallelDefinition()
            .CreateRuntime(RuntimeGraphState.Operational);

        var graph = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph());
        var overlay = Assert.IsType<GraphActiveStateOverlay<RuntimeGraphState>>(graph.RuntimeOverlay);

        Assert.Equal(GraphActiveStateOverlayKind.Parallel, overlay.ShapeKind);
        Assert.Equal(RuntimeGraphState.Operational, overlay.OwningCompositeState);
        Assert.Equal(RuntimeGraphExportAssertions.NodeIdFor(graph, RuntimeGraphState.Operational),
            overlay.OwningCompositeNodeId);
        Assert.Equal(["Fulfillment", "Billing"], overlay.Regions.Select(region => region.RegionName));
        Assert.Equal([RuntimeGraphState.WaitingForPick, RuntimeGraphState.WaitingForPayment],
            overlay.Regions.Select(region => region.ActiveLeafState));
        Assert.All(overlay.Regions, region => Assert.Equal(overlay.Sequence, region.Sequence));
    }
}
