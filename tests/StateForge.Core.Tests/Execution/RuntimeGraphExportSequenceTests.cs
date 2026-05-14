using StateForge.Core.Tests.Introspection;

namespace StateForge.Core.Tests.Execution;

public sealed class RuntimeGraphExportSequenceTests
{
    [Fact]
    public async Task ExportGraph_reports_later_sequence_after_committed_transition()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);
        var before = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());

        await runtime.ApplyAsync(RuntimeGraphEvent.Pay);

        var after = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.Equal(RuntimeGraphState.Paid, after.ActiveLeafState);
        Assert.True(after.Sequence > before.Sequence);
        Assert.True(after.IsTerminal);
        Assert.True(after.IsComplete);
    }

    [Fact]
    public void Repeated_exports_without_dispatch_keep_same_sequence()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Created);

        var first = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        var second = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());

        Assert.Equal(first.Sequence, second.Sequence);
        Assert.Equal(first.ActiveLeafState, second.ActiveLeafState);
    }
}
