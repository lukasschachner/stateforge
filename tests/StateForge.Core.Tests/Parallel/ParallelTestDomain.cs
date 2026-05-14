namespace StateForge.Core.Tests.Parallel;

internal enum ParallelState
{
    Idle,
    Operational,
    WaitingForPick,
    Packing,
    FulfillmentDone,
    WaitingForPayment,
    CapturingPayment,
    BillingDone,
    Cancelled
}

internal enum ParallelEvent
{
    Start,
    PickStarted,
    PaymentStarted,
    CompleteFulfillment,
    CompleteBilling,
    PickFinished,
    PaymentCaptured,
    Cancel
}