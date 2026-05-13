using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class RuntimeGraphExportFlatTests
{
    [Fact]
    public void ExportGraph_marks_current_flat_active_leaf()
    {
        var definition = RuntimeGraphExportTestDomain.CreateFlatDefinition();
        var runtime = definition.CreateRuntime(RuntimeGraphState.Created);

        var graph = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph());
        var overlay = Assert.IsType<GraphActiveStateOverlay<RuntimeGraphState>>(graph.RuntimeOverlay);

        Assert.Equal(GraphActiveStateOverlayKind.SingleLeaf, overlay.ShapeKind);
        Assert.Equal(RuntimeGraphState.Created, overlay.ActiveLeafState);
        Assert.Equal(RuntimeGraphExportAssertions.NodeIdFor(graph, RuntimeGraphState.Created), overlay.ActiveLeafNodeId);
        Assert.Empty(overlay.ActivePath);
        Assert.Empty(overlay.Regions);
        Assert.False(overlay.IsTerminal);
    }

    [Fact]
    public void Runtime_export_preserves_static_nodes_and_edges()
    {
        var definition = RuntimeGraphExportTestDomain.CreateFlatDefinition();
        var staticGraph = RuntimeGraphExportAssertions.SucceededGraph(definition.ExportGraph());
        var runtimeGraph = RuntimeGraphExportAssertions.SucceededGraph(
            definition.CreateRuntime(RuntimeGraphState.Created).ExportGraph());

        Assert.Equal(staticGraph.Nodes.Select(node => node.Id), runtimeGraph.Nodes.Select(node => node.Id));
        Assert.Equal(staticGraph.Edges.Select(edge => edge.Id), runtimeGraph.Edges.Select(edge => edge.Id));
        Assert.Null(staticGraph.RuntimeOverlay);
        Assert.NotNull(runtimeGraph.RuntimeOverlay);
    }
}
