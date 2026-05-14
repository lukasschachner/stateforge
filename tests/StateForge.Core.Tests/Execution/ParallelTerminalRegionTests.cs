using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelTerminalRegionTests
{
    [Fact]
    public async Task Terminal_region_remains_terminal_while_sibling_advances()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.FulfillmentDone, [ParallelState.FulfillmentDone],
                    [ParallelState.FulfillmentDone])
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.CapturingPayment);
            b.State(ParallelState.WaitingForPayment).On(ParallelEvent.PaymentStarted)
                .GoTo(ParallelState.CapturingPayment);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);
        await runtime.ApplyAsync(ParallelEvent.PaymentStarted);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.FulfillmentDone);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.CapturingPayment);
    }
}