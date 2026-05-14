using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelCompletionTests
{
    [Fact]
    public async Task Composite_is_complete_only_when_all_regions_are_terminal()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, [ParallelState.FulfillmentDone],
                    [ParallelState.FulfillmentDone])
                .Region("Billing", ParallelState.WaitingForPayment, [ParallelState.BillingDone],
                    [ParallelState.BillingDone]);
            b.State(ParallelState.WaitingForPick).On(ParallelEvent.CompleteFulfillment)
                .GoTo(ParallelState.FulfillmentDone);
            b.State(ParallelState.WaitingForPayment).On(ParallelEvent.CompleteBilling).GoTo(ParallelState.BillingDone);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);
        await runtime.ApplyAsync(ParallelEvent.CompleteFulfillment);
        Assert.False(runtime.ActiveStateShape.ActiveRegions.All(r => r.IsTerminal));
        await runtime.ApplyAsync(ParallelEvent.CompleteBilling);
        Assert.True(runtime.ActiveStateShape.ActiveRegions.All(r => r.IsTerminal));
    }
}