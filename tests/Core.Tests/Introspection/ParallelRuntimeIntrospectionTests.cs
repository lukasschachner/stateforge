using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelRuntimeIntrospectionTests
{
    [Fact]
    public void Runtime_active_state_shape_exposes_region_entries()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinition().CreateRuntime(ParallelState.Operational);
        Assert.True(runtime.ActiveStateShape.IsParallel);
        Assert.Equal(2, runtime.ActiveStateShape.ActiveRegions.Count);
    }
}