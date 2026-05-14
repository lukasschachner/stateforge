namespace StateForge.Core.Tests.Introspection;

public sealed class RuntimeGraphExportParallelOrderingTests
{
    [Fact]
    public void Parallel_region_overlays_follow_declaration_order()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateParallelDefinition()
            .CreateRuntime(RuntimeGraphState.Operational);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());

        Assert.Equal([0, 1], overlay.Regions.Select(region => region.RegionOrder));
        Assert.Equal(["region-000", "region-001"],
            overlay.Regions.Select(region => region.RegionId));
    }

    [Fact]
    public async Task Completed_region_ids_follow_declaration_order()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateParallelDefinition()
            .CreateRuntime(RuntimeGraphState.Operational);

        await runtime.ApplyAsync(RuntimeGraphEvent.PaymentStarted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PaymentCaptured);
        await runtime.ApplyAsync(RuntimeGraphEvent.PickStarted);
        await runtime.ApplyAsync(RuntimeGraphEvent.PickCompleted);

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(runtime.ExportGraph());
        Assert.Equal(["region-000", "region-001"], overlay.CompletedRegionIds);
    }
}
