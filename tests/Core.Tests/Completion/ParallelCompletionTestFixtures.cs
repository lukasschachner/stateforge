using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Tests.Completion;

internal static class ParallelCompletionTestFixtures
{
    public static StateMachineDefinition<CompletionState, CompletionEvent> CreateOperationalDefinition()
    {
        return StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.ParallelComposite(CompletionState.Operational)
                .Region("Fulfillment", CompletionState.WaitingForPick, [CompletionState.FulfillmentDone],
                    [CompletionState.FulfillmentDone])
                .Region("Billing", CompletionState.WaitingForPayment, [CompletionState.BillingDone],
                    [CompletionState.BillingDone])
                .OnCompletion()
                .GoTo(CompletionState.ReadyToClose);

            builder.State(CompletionState.WaitingForPick).On(CompletionEvent.Pick).GoTo(CompletionState.FulfillmentDone);
            builder.State(CompletionState.WaitingForPayment).On(CompletionEvent.Pay).GoTo(CompletionState.BillingDone);
            builder.State(CompletionState.ReadyToClose);
        });
    }
}
