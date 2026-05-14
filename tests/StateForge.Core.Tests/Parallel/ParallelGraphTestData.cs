using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Parallel;

internal static class ParallelGraphTestData
{
    public static StateMachineDefinition<ParallelState, ParallelEvent> CreateTwoRegionDefinition()
    {
        return CreateTwoRegionDefinitionOldStyle();
    }

    public static StateMachineDefinition<ParallelState, ParallelEvent> CreateTwoRegionDefinitionOldStyle()
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
            AddTwoRegionTransitions(builder);
        });
    }

    public static StateMachineDefinition<ParallelState, ParallelEvent> CreateTwoRegionDefinitionNewStyle()
    {
        return StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational, composite =>
            {
                composite.Region("Fulfillment", region =>
                {
                    region.Initial(ParallelState.WaitingForPick)
                        .On(ParallelEvent.PickStarted)
                        .GoTo(ParallelState.Packing);
                    region.State(ParallelState.Packing)
                        .On(ParallelEvent.CompleteFulfillment)
                        .GoTo(ParallelState.FulfillmentDone);
                    region.Terminal(ParallelState.FulfillmentDone);
                });
                composite.Region("Billing", region =>
                {
                    region.Initial(ParallelState.WaitingForPayment)
                        .On(ParallelEvent.PaymentStarted)
                        .GoTo(ParallelState.CapturingPayment);
                    region.State(ParallelState.CapturingPayment)
                        .On(ParallelEvent.CompleteBilling)
                        .GoTo(ParallelState.BillingDone);
                    region.Terminal(ParallelState.BillingDone);
                });
            });
        });
    }

    public static StateMachineDefinition<ParallelState, ParallelEvent> CreateTwoRegionDefinitionMixedStyle()
    {
        return StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational, composite =>
            {
                composite.Region("Fulfillment", region =>
                {
                    region.Initial(ParallelState.WaitingForPick);
                    region.State(ParallelState.Packing);
                    region.Terminal(ParallelState.FulfillmentDone);
                });
                composite.Region("Billing", ParallelState.WaitingForPayment,
                    new[]
                    {
                        ParallelState.WaitingForPayment, ParallelState.CapturingPayment, ParallelState.BillingDone
                    }, new[] { ParallelState.BillingDone });
            });
            AddTwoRegionTransitions(builder);
        });
    }

    private static void AddTwoRegionTransitions(StateMachineDefinitionBuilder<ParallelState, ParallelEvent> builder)
    {
        builder.State(ParallelState.WaitingForPick).On(ParallelEvent.PickStarted).GoTo(ParallelState.Packing);
        builder.State(ParallelState.Packing).On(ParallelEvent.CompleteFulfillment).GoTo(ParallelState.FulfillmentDone);
        builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.PaymentStarted)
            .GoTo(ParallelState.CapturingPayment);
        builder.State(ParallelState.CapturingPayment).On(ParallelEvent.CompleteBilling).GoTo(ParallelState.BillingDone);
    }
}