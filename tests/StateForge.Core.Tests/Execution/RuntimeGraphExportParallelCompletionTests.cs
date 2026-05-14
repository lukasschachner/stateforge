using StateForge.Core.Tests.Introspection;

namespace StateForge.Core.Tests.Execution;

public sealed class RuntimeGraphExportParallelCompletionTests
{
    [Fact]
    public async Task ExportGraph_reports_completed_and_unfinished_parallel_regions_independently()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateParallelDefinition()
            .CreateRuntime(RuntimeGraphState.Operational);

        await runtime.ApplyAsync(RuntimeGraphEvent.PickStarted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PickCompleted);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.False(overlay.IsComplete);
        Assert.Equal(["region-000"], overlay.CompletedRegionIds);
        Assert.True(overlay.Regions.Single(region => region.RegionName == "Fulfillment").IsComplete);
        Assert.False(overlay.Regions.Single(region => region.RegionName == "Billing").IsComplete);
    }

    [Fact]
    public async Task ExportGraph_reports_parallel_overlay_complete_when_all_regions_are_terminal()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateParallelDefinition()
            .CreateRuntime(RuntimeGraphState.Operational);

        await runtime.ApplyAsync(RuntimeGraphEvent.PickStarted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PickCompleted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PaymentStarted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PaymentCaptured);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.True(overlay.IsComplete);
        Assert.Equal(["region-000", "region-001"], overlay.CompletedRegionIds);
    }
}
