using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class ActiveStateSnapshotParallelCaptureTests
{
    [Fact]
    public async Task CaptureIncludesOneRegionEntryPerActiveParallelRegion()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var runtime = definition.CreateRuntime(SnapshotState.Operational);

        await runtime.ApplyAsync(SnapshotEvent.Pack);
        var snapshot = runtime.CaptureActiveStateSnapshot();

        Assert.Equal(ActiveStateSnapshotKind.Parallel, snapshot.Kind);
        Assert.Equal(SnapshotState.Operational, snapshot.OwningCompositeState);
        Assert.Equal(2, snapshot.RegionSnapshots.Count);
        ActiveStateSnapshotAssertions.AssertRegion(snapshot, "Fulfillment", SnapshotState.FulfillmentPacking);
        ActiveStateSnapshotAssertions.AssertRegion(snapshot, "Billing", SnapshotState.BillingWaiting);
    }
}
