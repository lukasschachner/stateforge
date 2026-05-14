using StateForge.Core.Tests.Introspection;

namespace StateForge.Core.Tests.Execution;

public sealed class RuntimeGraphExportHierarchySequenceTests
{
    [Fact]
    public async Task Hierarchical_export_sequence_advances_when_active_leaf_changes()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateHierarchicalDefinition()
            .CreateRuntime(RuntimeGraphState.Reviewing);
        var before = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());

        await runtime.ApplyAsync(RuntimeGraphEvent.AuthorApproved);

        var after = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.True(after.Sequence > before.Sequence);
        Assert.NotEqual(before.ActiveLeafState, after.ActiveLeafState);
    }
}
