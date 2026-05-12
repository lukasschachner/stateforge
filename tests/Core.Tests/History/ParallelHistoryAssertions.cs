using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Tests.History;

internal static class ParallelHistoryAssertions
{
    public static ActiveRegionEntry<TState> Region<TState>(this ActiveStateShape<TState> shape, string regionName)
    {
        Assert.True(shape.IsParallel);
        return Assert.Single(shape.ActiveRegions, entry => entry.RegionName == regionName);
    }

    public static void AssertRegion<TState>(this ActiveStateShape<TState> shape, string regionName, TState expectedLeaf)
    {
        Assert.Equal(expectedLeaf, shape.Region(regionName).ActiveLeafState);
    }

    public static ParallelHistorySnapshot<TState> AssertParallelHistorySnapshot<TState>(
        this IReadOnlyList<ParallelHistorySnapshot<TState>> snapshots, TState composite)
    {
        return Assert.Single(snapshots,
            snapshot => EqualityComparer<TState>.Default.Equals(snapshot.CompositeState, composite));
    }
}