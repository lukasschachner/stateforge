using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Tests.Parallel;

internal static class ParallelAssertions
{
    public static ActiveRegionEntry<TState> Region<TState>(this ActiveStateShape<TState> shape, string name)
    {
        Assert.True(shape.IsParallel);
        return Assert.Single(shape.ActiveRegions, r => r.RegionName == name);
    }

    public static void AssertRegion<TState>(this ActiveStateShape<TState> shape, string name, TState expectedLeaf)
    {
        Assert.Equal(expectedLeaf, shape.Region(name).ActiveLeafState);
    }
}