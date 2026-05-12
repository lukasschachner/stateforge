using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelDispatchOrderingTests
{
    [Fact]
    public async Task Shared_event_advances_regions_in_declaration_order()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment,
                    ParallelState.CapturingPayment);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).Execute(_ => log.Add("fulfillment"))
                .GoTo(ParallelState.Packing);
            builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.Cancel).Execute(_ => log.Add("billing"))
                .GoTo(ParallelState.CapturingPayment);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);

        await runtime.ApplyAsync(ParallelEvent.Cancel);

        Assert.Equal(["fulfillment", "billing"], log);
    }
}