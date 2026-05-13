using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Tests.Introspection;

internal enum RuntimeGraphState
{
    Created,
    Paid,
    Reviewing,
    AuthorReview,
    LegalReview,
    Operational,
    WaitingForPick,
    Packing,
    FulfillmentDone,
    WaitingForPayment,
    CapturingPayment,
    BillingDone,
    Archived,
    Unknown
}

internal enum RuntimeGraphEvent
{
    Pay,
    AuthorApproved,
    PickStarted,
    PickCompleted,
    PaymentStarted,
    PaymentCaptured,
    Archive
}

internal static class RuntimeGraphExportTestDomain
{
    public static StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent> CreateFlatDefinition()
    {
        return StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent>.Create(builder =>
        {
            builder.State(RuntimeGraphState.Created).On(RuntimeGraphEvent.Pay).GoTo(RuntimeGraphState.Paid);
            builder.State(RuntimeGraphState.Paid).Terminal();
        });
    }

    public static StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent> CreateHierarchicalDefinition()
    {
        return StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent>.Create(builder =>
        {
            builder.CompositeState(RuntimeGraphState.Reviewing, RuntimeGraphState.AuthorReview);
            builder.ChildState(RuntimeGraphState.LegalReview, RuntimeGraphState.Reviewing);
            builder.State(RuntimeGraphState.AuthorReview)
                .On(RuntimeGraphEvent.AuthorApproved)
                .GoTo(RuntimeGraphState.LegalReview);
        });
    }

    public static StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent> CreateParallelDefinition()
    {
        return StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent>.Create(builder =>
        {
            builder.ParallelComposite(RuntimeGraphState.Operational)
                .Region("Fulfillment", RuntimeGraphState.WaitingForPick,
                    [RuntimeGraphState.WaitingForPick, RuntimeGraphState.Packing, RuntimeGraphState.FulfillmentDone],
                    [RuntimeGraphState.FulfillmentDone])
                .Region("Billing", RuntimeGraphState.WaitingForPayment,
                    [RuntimeGraphState.WaitingForPayment, RuntimeGraphState.CapturingPayment, RuntimeGraphState.BillingDone],
                    [RuntimeGraphState.BillingDone]);

            builder.State(RuntimeGraphState.WaitingForPick)
                .On(RuntimeGraphEvent.PickStarted)
                .GoTo(RuntimeGraphState.Packing);
            builder.State(RuntimeGraphState.Packing)
                .On(RuntimeGraphEvent.PickCompleted)
                .GoTo(RuntimeGraphState.FulfillmentDone);
            builder.State(RuntimeGraphState.WaitingForPayment)
                .On(RuntimeGraphEvent.PaymentStarted)
                .GoTo(RuntimeGraphState.CapturingPayment);
            builder.State(RuntimeGraphState.CapturingPayment)
                .On(RuntimeGraphEvent.PaymentCaptured)
                .GoTo(RuntimeGraphState.BillingDone);
            builder.State(RuntimeGraphState.Operational)
                .On(RuntimeGraphEvent.Archive)
                .GoTo(RuntimeGraphState.Archived);
            builder.State(RuntimeGraphState.Archived).Terminal();
        });
    }
}
