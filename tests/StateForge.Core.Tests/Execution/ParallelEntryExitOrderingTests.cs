using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelEntryExitOrderingTests
{
    [Fact]
    public async Task Parent_exit_processes_regions_in_reverse_declaration_order()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick)
                .Region("Billing", ParallelState.WaitingForPayment);
            b.State(ParallelState.WaitingForPick).OnExit(_ => log.Add("fulfillment"));
            b.State(ParallelState.WaitingForPayment).OnExit(_ => log.Add("billing"));
            b.State(ParallelState.Operational).On(ParallelEvent.Cancel).GoTo(ParallelState.Cancelled);
            b.State(ParallelState.Cancelled);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);
        await runtime.ApplyAsync(ParallelEvent.Cancel);
        Assert.Equal(["billing", "fulfillment"], log);
    }
}