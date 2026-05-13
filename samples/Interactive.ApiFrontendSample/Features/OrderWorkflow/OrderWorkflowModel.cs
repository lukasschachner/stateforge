namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal enum OrderDemoState
{
    Draft,
    Reviewing,
    ReviewPending,
    ReviewEscalated,
    ReviewApproved,
    Processing,
    FulfillmentBlocked,
    Picking,
    Packed,
    Shipped,
    PaymentPending,
    PaymentAuthorized,
    PaymentPartiallyCaptured,
    PaymentCaptured,
    FraudCheckPending,
    FraudCheckCleared,
    OnHold,
    Completed,
    Cancelled
}

internal abstract record OrderDemoEvent;

internal sealed record SubmitOrder(decimal TotalAmount, string CustomerId) : OrderDemoEvent;

internal sealed record ApproveReview(int RiskScore, string ApprovedBy) : OrderDemoEvent;

internal sealed record EscalateReview(string Reason) : OrderDemoEvent;

internal sealed record ApproveEscalation(string ApprovedBy) : OrderDemoEvent;

internal sealed record StartPacking(string Worker) : OrderDemoEvent;

internal sealed record ShipOrder(string TrackingNumber) : OrderDemoEvent;

internal sealed record AuthorizePayment(string AuthorizationCode) : OrderDemoEvent;

internal abstract record CapturePayment(decimal Amount, decimal CapturedTotal, decimal RequiredAmount) : OrderDemoEvent;

internal sealed record PartialCapturePayment(decimal Amount, decimal CapturedTotal, decimal RequiredAmount)
    : CapturePayment(Amount, CapturedTotal, RequiredAmount);

internal sealed record FinalCapturePayment(decimal Amount, decimal CapturedTotal, decimal RequiredAmount)
    : CapturePayment(Amount, CapturedTotal, RequiredAmount);

internal sealed record ClearFraudCheck(string Analyst) : OrderDemoEvent;

internal sealed record PlaceOnHold(string Reason) : OrderDemoEvent;

internal sealed record ResumeProcessing(string Note) : OrderDemoEvent;

internal sealed record CancelOrder(string Reason) : OrderDemoEvent;
