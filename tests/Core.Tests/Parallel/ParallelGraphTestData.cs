using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Tests.Parallel;

internal static class ParallelGraphTestData
{
    public static StateMachineDefinition<ParallelState, ParallelEvent> CreateTwoRegionDefinition()
    {
        return StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick,
                    new[] { ParallelState.WaitingForPick, ParallelState.Packing, ParallelState.FulfillmentDone },
                    new[] { ParallelState.FulfillmentDone })
                .Region("Billing", ParallelState.WaitingForPayment,
                    new[]
                    {
                        ParallelState.WaitingForPayment, ParallelState.CapturingPayment, ParallelState.BillingDone
                    }, new[] { ParallelState.BillingDone });
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.PickStarted).GoTo(ParallelState.Packing);
            builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.PaymentStarted)
                .GoTo(ParallelState.CapturingPayment);
        });
    }
}