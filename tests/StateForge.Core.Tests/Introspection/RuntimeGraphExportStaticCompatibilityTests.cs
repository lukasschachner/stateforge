namespace StateForge.Core.Tests.Introspection;

public sealed class RuntimeGraphExportStaticCompatibilityTests
{
    [Fact]
    public void Definition_export_keeps_runtime_overlay_null()
    {
        var definition = RuntimeGraphExportTestDomain.CreateParallelDefinition();

        var graph = RuntimeGraphExportAssertions.SucceededGraph(definition.ExportGraph());

        Assert.Null(graph.RuntimeOverlay);
        Assert.Equal(2, graph.Regions.Count);
        Assert.Equal(8, graph.Nodes.Count);
    }

    [Fact]
    public void Runtime_export_with_overlay_disabled_matches_static_graph_shape()
    {
        var definition = RuntimeGraphExportTestDomain.CreateParallelDefinition();
        var staticGraph = RuntimeGraphExportAssertions.SucceededGraph(definition.ExportGraph());
        var runtimeGraph = RuntimeGraphExportAssertions.SucceededGraph(
            definition.CreateRuntime(RuntimeGraphState.Operational).ExportGraph(new()
            {
                OverlayMode = StateForge.Core.Introspection.RuntimeGraphOverlayMode.None
            }));

        Assert.Null(runtimeGraph.RuntimeOverlay);
        Assert.Equal(staticGraph.Nodes.Select(node => node.Id), runtimeGraph.Nodes.Select(node => node.Id));
        Assert.Equal(staticGraph.Edges.Select(edge => edge.Id), runtimeGraph.Edges.Select(edge => edge.Id));
        Assert.Equal(staticGraph.Regions.Select(region => region.RegionId), runtimeGraph.Regions.Select(region => region.RegionId));
    }
}
