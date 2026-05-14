using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelHistoryTerminalRestoreTests
{
    [Fact]
    public async Task Terminal_region_state_is_recorded_and_restored()
    {
        var definition = StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent>.Create(builder =>
        {
            builder.State(ParallelHistoryState.Operational).On(ParallelHistoryEvent.Cancel)
                .GoTo(ParallelHistoryState.Cancelled);
            builder.State(ParallelHistoryState.Cancelled).On(ParallelHistoryEvent.Start)
                .GoTo(ParallelHistoryState.Operational);
            builder.ParallelComposite(ParallelHistoryState.Operational)
                .WithHistory(HistoryMode.Deep)
                .Region("Fulfillment", ParallelHistoryState.WaitingForPick,
                    [ParallelHistoryState.WaitingForPick, ParallelHistoryState.Packing], [ParallelHistoryState.Packing])
                .Region("Billing", ParallelHistoryState.WaitingForPayment, ParallelHistoryState.WaitingForPayment);
            builder.State(ParallelHistoryState.WaitingForPick).On(ParallelHistoryEvent.PickStarted)
                .GoTo(ParallelHistoryState.Packing);
        });
        var runtime = definition.CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelHistoryState.Packing);
        Assert.True(runtime.ActiveStateShape.Region("Fulfillment").IsTerminal);
    }
}