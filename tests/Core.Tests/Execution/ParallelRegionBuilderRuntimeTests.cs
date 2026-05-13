using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelRegionBuilderRuntimeTests
{
    [Fact]
    public async Task Region_block_definition_progresses_independent_regions()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle()
            .CreateRuntime(ParallelState.Operational);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.WaitingForPick);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);

        await runtime.ApplyAsync(ParallelEvent.PickStarted);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);

        await runtime.ApplyAsync(ParallelEvent.PaymentStarted);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.Packing);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.CapturingPayment);
    }

    [Fact]
    public async Task Region_scoped_terminal_states_complete_parallel_regions()
    {
        var runtime = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle()
            .CreateRuntime(ParallelState.Operational);

        await runtime.ApplyAsync(ParallelEvent.PickStarted);
        await runtime.ApplyAsync(ParallelEvent.PaymentStarted);
        await runtime.ApplyAsync(ParallelEvent.CompleteFulfillment);
        await runtime.ApplyAsync(ParallelEvent.CompleteBilling);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.FulfillmentDone);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.BillingDone);
    }
}
