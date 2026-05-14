namespace StateForge.Core.Tests.Completion;

internal enum CompletionState
{
    Reviewing,
    AuthorReview,
    LegalReview,
    ReviewDone,
    Approved,
    Escalated,
    Operational,
    WaitingForPick,
    FulfillmentDone,
    WaitingForPayment,
    BillingDone,
    ReadyToClose,
    Cancelled,
    Leaf
}

internal enum CompletionEvent
{
    Approve,
    LegalApprove,
    Pick,
    Pay,
    Other
}
