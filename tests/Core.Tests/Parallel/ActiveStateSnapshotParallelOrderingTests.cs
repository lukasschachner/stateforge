using Core.Tests.Execution;

namespace StateMachineLibrary.Core.Tests.Parallel;

public sealed class ActiveStateSnapshotParallelOrderingTests
{
    [Fact]
    public void CaptureOrdersRegionsByDeclarationOrder()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var runtime = definition.CreateRuntime(SnapshotState.Operational);

        var snapshot = runtime.CaptureActiveStateSnapshot();

        Assert.Equal(["Fulfillment", "Billing"], snapshot.RegionSnapshots.Select(region => region.RegionName));
    }
}
