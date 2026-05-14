namespace StateForge.Core.Tests.Execution;

public sealed class ActiveStateSnapshotParallelRestoreTests
{
    [Fact]
    public async Task CreateRuntimeFromParallelSnapshotRestoresEachRegionLeaf()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var source = definition.CreateRuntime(SnapshotState.Operational);
        await source.ApplyAsync(SnapshotEvent.Pack);
        await source.ApplyAsync(SnapshotEvent.Bill);

        var restored = definition.CreateRuntime(source.CaptureActiveStateSnapshot());
        var restoredSnapshot = restored.CaptureActiveStateSnapshot();

        ActiveStateSnapshotAssertions.AssertRegion(restoredSnapshot, "Fulfillment", SnapshotState.FulfillmentPacking);
        ActiveStateSnapshotAssertions.AssertRegion(restoredSnapshot, "Billing", SnapshotState.BillingCapturing);

        var outcome = await restored.ApplyAsync(SnapshotEvent.Finish);

        Assert.True(outcome.IsSuccess);
        ActiveStateSnapshotAssertions.AssertRegion(restored.CaptureActiveStateSnapshot(), "Fulfillment",
            SnapshotState.FulfillmentDone);
    }
}
