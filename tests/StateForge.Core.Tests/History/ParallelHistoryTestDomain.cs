using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.History;

internal enum ParallelHistoryState
{
    Idle,
    Operational,
    WaitingForPick,
    Packing,
    WaitingForPayment,
    CapturingPayment,
    Cancelled
}

internal enum ParallelHistoryEvent
{
    Start,
    PickStarted,
    PaymentStarted,
    Cancel
}

internal static class ParallelHistoryTestDomain
{
    public static StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent> CreateTwoRegionDefinition(
        HistoryMode mode)
    {
        return StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent>.Create(builder =>
        {
            builder.State(ParallelHistoryState.Idle).On(ParallelHistoryEvent.Start)
                .GoTo(ParallelHistoryState.Operational);
            builder.State(ParallelHistoryState.Operational).On(ParallelHistoryEvent.Cancel)
                .GoTo(ParallelHistoryState.Cancelled);
            builder.State(ParallelHistoryState.Cancelled).On(ParallelHistoryEvent.Start)
                .GoTo(ParallelHistoryState.Operational);
            builder.ParallelComposite(ParallelHistoryState.Operational)
                .WithHistory(mode)
                .Region("Fulfillment", ParallelHistoryState.WaitingForPick, ParallelHistoryState.WaitingForPick,
                    ParallelHistoryState.Packing)
                .Region("Billing", ParallelHistoryState.WaitingForPayment, ParallelHistoryState.WaitingForPayment,
                    ParallelHistoryState.CapturingPayment);
            builder.State(ParallelHistoryState.WaitingForPick).On(ParallelHistoryEvent.PickStarted)
                .GoTo(ParallelHistoryState.Packing);
            builder.State(ParallelHistoryState.WaitingForPayment).On(ParallelHistoryEvent.PaymentStarted)
                .GoTo(ParallelHistoryState.CapturingPayment);
        });
    }
}