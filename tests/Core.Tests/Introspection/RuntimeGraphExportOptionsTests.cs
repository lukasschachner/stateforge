using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class RuntimeGraphExportOptionsTests
{
    [Fact]
    public void Unknown_overlay_mode_is_rejected()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        Assert.Throws<ArgumentOutOfRangeException>(() => runtime.ExportGraph(new RuntimeGraphExportOptions
        {
            OverlayMode = (RuntimeGraphOverlayMode)999
        }));
    }

    [Fact]
    public void Active_overlay_requires_active_shape_validation()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        Assert.Throws<ArgumentException>(() => runtime.ExportGraph(new RuntimeGraphExportOptions
        {
            OverlayMode = RuntimeGraphOverlayMode.ActiveState,
            ValidateActiveShape = false
        }));
    }

    [Fact]
    public void Overlay_none_allows_validation_flag_to_be_disabled()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        var graph = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph(new RuntimeGraphExportOptions
        {
            OverlayMode = RuntimeGraphOverlayMode.None,
            ValidateActiveShape = false
        }));

        Assert.Null(graph.RuntimeOverlay);
    }
}
