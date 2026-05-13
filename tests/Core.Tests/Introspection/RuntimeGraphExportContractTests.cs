using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class RuntimeGraphExportContractTests
{
    [Fact]
    public void State_owning_runtime_export_includes_overlay_by_default()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());

        Assert.Equal(RuntimeGraphState.Created, overlay.ActiveLeafState);
    }

    [Fact]
    public void Overlay_mode_none_returns_definition_graph_without_overlay()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        var graph = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph(new RuntimeGraphExportOptions
        {
            OverlayMode = RuntimeGraphOverlayMode.None
        }));

        Assert.Null(graph.RuntimeOverlay);
    }

    [Fact]
    public void Static_introspection_export_has_no_runtime_overlay()
    {
        var graph = RuntimeGraphExportAssertions.SucceededGraph(
            RuntimeGraphExportTestDomain.CreateFlatDefinition().Introspect().ExportGraph());

        Assert.Null(graph.RuntimeOverlay);
    }
}
