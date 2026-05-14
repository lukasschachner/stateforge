using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelHistoryAtomicRestoreTests
{
    [Fact]
    public async Task Failed_reentry_action_does_not_expose_partial_parallel_shape()
    {
        var failRestore = false;
        var definition = StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent>.Create(builder =>
        {
            builder.State(ParallelHistoryState.Operational).On(ParallelHistoryEvent.Cancel)
                .GoTo(ParallelHistoryState.Cancelled);
            builder.State(ParallelHistoryState.Cancelled).On(ParallelHistoryEvent.Start)
                .Execute(_ =>
                {
                    if (failRestore) throw new InvalidOperationException("boom");
                })
                .GoTo(ParallelHistoryState.Operational);
            builder.ParallelComposite(ParallelHistoryState.Operational)
                .WithHistory(HistoryMode.Deep)
                .Region("Fulfillment", ParallelHistoryState.WaitingForPick, ParallelHistoryState.WaitingForPick,
                    ParallelHistoryState.Packing)
                .Region("Billing", ParallelHistoryState.WaitingForPayment, ParallelHistoryState.WaitingForPayment,
                    ParallelHistoryState.CapturingPayment);
            builder.State(ParallelHistoryState.WaitingForPick).On(ParallelHistoryEvent.PickStarted)
                .GoTo(ParallelHistoryState.Packing);
            builder.State(ParallelHistoryState.WaitingForPayment).On(ParallelHistoryEvent.PaymentStarted)
                .GoTo(ParallelHistoryState.CapturingPayment);
        });
        var runtime = definition.CreateRuntime(ParallelHistoryState.Operational);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.PaymentStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.Cancel);
        failRestore = true;
        var outcome = await runtime.ApplyAsync(ParallelHistoryEvent.Start);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(ParallelHistoryState.Cancelled, runtime.CurrentState);
        Assert.False(runtime.ActiveStateShape.IsParallel);
    }
}