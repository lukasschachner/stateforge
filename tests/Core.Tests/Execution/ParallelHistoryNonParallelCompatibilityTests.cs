using Core.Tests.History;
using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelHistoryNonParallelCompatibilityTests
{
    [Fact]
    public async Task Ordinary_hierarchical_shallow_history_still_restores_last_active_child()
    {
        var runtime = HistoryTestDomain.CreateOperationalMachine().CreateRuntime(HistoryState.Offline);

        await runtime.ApplyAsync(new Resume());
        await runtime.ApplyAsync(new Start());
        await runtime.ApplyAsync(new Pause());
        var resumed = await runtime.ApplyAsync(new Resume());

        Assert.True(resumed.IsSuccess);
        Assert.Equal(HistoryState.Processing, runtime.CurrentState);
    }

    [Fact]
    public async Task Parallel_composite_without_direct_history_still_enters_region_initials()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinition().CreateRuntime(ParallelState.Operational);

        await runtime.ApplyAsync(ParallelEvent.PickStarted);
        await runtime.ApplyAsync(ParallelEvent.PaymentStarted);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.CapturingPayment);
    }
}