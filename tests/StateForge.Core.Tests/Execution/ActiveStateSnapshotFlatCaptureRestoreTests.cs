using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class ActiveStateSnapshotFlatCaptureRestoreTests
{
    [Fact]
    public async Task ExternalRuntimeCapturesCurrentAccessorBackedSnapshot()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var state = SnapshotState.Idle;
        var runtime = definition.CreateRuntime(StateAccessor.Create(() => state, next => state = next));

        await runtime.ApplyAsync(SnapshotEvent.Start);
        var snapshot = await runtime.CaptureActiveStateSnapshotAsync();

        Assert.Equal(ActiveStateSnapshotKind.SingleLeaf, snapshot.Kind);
        Assert.Equal(SnapshotState.Running, snapshot.ActiveLeafState);
    }

    [Fact]
    public async Task FlatSnapshotCapturesAndRestoresSingleLeafWithoutRegionMetadata()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var runtime = definition.CreateRuntime(SnapshotState.Idle);
        await runtime.ApplyAsync(SnapshotEvent.Start);

        var snapshot = runtime.CaptureActiveStateSnapshot();
        var restored = definition.CreateRuntime(snapshot);

        Assert.Equal(ActiveStateSnapshotKind.SingleLeaf, snapshot.Kind);
        Assert.Equal(SnapshotState.Running, snapshot.ActiveLeafState);
        Assert.Empty(snapshot.RegionSnapshots);
        Assert.Equal(SnapshotState.Running, restored.CurrentState);
    }
}
