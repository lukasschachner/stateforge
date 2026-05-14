using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public sealed class RuntimeGraphExportHierarchyTests
{
    [Fact]
    public void ExportGraph_marks_hierarchical_active_path()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateHierarchicalDefinition()
            .CreateRuntime(RuntimeGraphState.Reviewing);

        var graph = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph());
        var overlay = Assert.IsType<GraphActiveStateOverlay<RuntimeGraphState>>(graph.RuntimeOverlay);

        Assert.Equal(GraphActiveStateOverlayKind.Hierarchical, overlay.ShapeKind);
        Assert.Equal(RuntimeGraphState.AuthorReview, overlay.ActiveLeafState);
        Assert.Equal([RuntimeGraphState.Reviewing, RuntimeGraphState.AuthorReview], overlay.ActivePath);
        Assert.Equal(overlay.ActivePath.Select(state => RuntimeGraphExportAssertions.NodeIdFor(graph, state)),
            overlay.ActivePathNodeIds);
    }

    [Fact]
    public async Task ExportGraph_updates_active_path_after_hierarchical_transition()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateHierarchicalDefinition()
            .CreateRuntime(RuntimeGraphState.Reviewing);

        await runtime.ApplyAsync(RuntimeGraphEvent.AuthorApproved);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.Equal(RuntimeGraphState.LegalReview, overlay.ActiveLeafState);
        Assert.Equal([RuntimeGraphState.Reviewing, RuntimeGraphState.LegalReview], overlay.ActivePath);
    }
}
