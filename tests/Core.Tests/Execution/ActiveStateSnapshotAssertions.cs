using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

internal static class ActiveStateSnapshotAssertions
{
    public static ActiveRegionSnapshot<SnapshotState> AssertRegion(
        ActiveStateSnapshot<SnapshotState> snapshot,
        string name,
        SnapshotState activeLeaf)
    {
        var region = Assert.Single(snapshot.RegionSnapshots, r => r.RegionName == name);
        Assert.Equal(activeLeaf, region.ActiveLeafState);
        Assert.Equal(activeLeaf, region.ActivePath.ActiveLeafState);
        return region;
    }

    public static void AssertPath(ActiveStatePath<SnapshotState>? path, params SnapshotState[] expected)
    {
        Assert.NotNull(path);
        Assert.Equal(expected, path.StatesRootToLeaf);
    }

    public static void AssertValid(ActiveStateSnapshot<SnapshotState> snapshot)
    {
        Assert.DoesNotContain(snapshot.RegionSnapshots, region => string.IsNullOrWhiteSpace(region.RegionId));
    }
}
