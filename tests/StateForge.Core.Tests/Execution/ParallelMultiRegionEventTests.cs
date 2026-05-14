using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelMultiRegionEventTests
{
    [Fact]
    public async Task Same_event_can_advance_multiple_independent_regions()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment,
                    ParallelState.CapturingPayment);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.Packing);
            builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.Cancel)
                .GoTo(ParallelState.CapturingPayment);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);

        var outcome = await runtime.ApplyAsync(ParallelEvent.Cancel);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(2, outcome.ParallelTransitions.Count);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.CapturingPayment);
    }
}