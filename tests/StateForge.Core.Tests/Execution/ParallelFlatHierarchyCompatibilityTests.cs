using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelFlatHierarchyCompatibilityTests
{
    [Fact]
    public async Task Flat_machine_active_shape_remains_single_leaf()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.State(ParallelState.Idle).On(ParallelEvent.Start).GoTo(ParallelState.Operational);
            builder.State(ParallelState.Operational);
        });
        var runtime = definition.CreateRuntime(ParallelState.Idle);

        var outcome = await runtime.ApplyAsync(ParallelEvent.Start);

        Assert.True(outcome.IsSuccess);
        Assert.False(runtime.ActiveStateShape.IsParallel);
        Assert.Equal(ParallelState.Operational, runtime.CurrentState);
    }
}