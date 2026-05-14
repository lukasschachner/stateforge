using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal static class OrderWorkflowDefinition
{
    public static StateMachineDefinition<OrderDemoState, OrderDemoEvent> Create()
    {
        return StateMachineDefinition<OrderDemoState, OrderDemoEvent>.Create(builder =>
        {
            builder.State(OrderDemoState.Draft)
                .On<SubmitOrder>()
                .When(ctx => ((SubmitOrder)ctx.Event).TotalAmount > 0m, "order total must be positive")
                .GoTo(OrderDemoState.Reviewing)
                .On<CancelOrder>()
                .GoTo(OrderDemoState.Cancelled);

            builder.State(OrderDemoState.Reviewing)
                .InitialChild(OrderDemoState.ReviewPending)
                .On<PlaceOnHold>()
                .GoTo(OrderDemoState.OnHold)
                .On<CancelOrder>()
                .GoTo(OrderDemoState.Cancelled)
                .OnCompletion()
                .GoTo(OrderDemoState.Processing);

            builder.State(OrderDemoState.ReviewPending)
                .ChildOf(OrderDemoState.Reviewing)
                .On<ApproveReview>()
                .When(ctx => ((ApproveReview)ctx.Event).RiskScore <= 80, "risk score must be <= 80")
                .GoTo(OrderDemoState.ReviewApproved)
                .On<EscalateReview>()
                .GoTo(OrderDemoState.ReviewEscalated);

            builder.State(OrderDemoState.ReviewEscalated)
                .ChildOf(OrderDemoState.Reviewing)
                .On<ApproveEscalation>()
                .GoTo(OrderDemoState.ReviewApproved)
                .On<CancelOrder>()
                .GoTo(OrderDemoState.Cancelled);

            builder.State(OrderDemoState.ReviewApproved)
                .ChildOf(OrderDemoState.Reviewing)
                .Terminal();

            builder.State(OrderDemoState.Processing)
                .On<PlaceOnHold>()
                .GoTo(OrderDemoState.OnHold)
                .On<CancelOrder>()
                .GoTo(OrderDemoState.Cancelled);

            builder.ParallelComposite(OrderDemoState.Processing, parallel =>
            {
                parallel.WithHistory();
                parallel.OnCompletion().GoTo(OrderDemoState.Completed);

                parallel.Region("Fulfillment", region =>
                {
                    region.Initial(OrderDemoState.FulfillmentBlocked)
                        .On<FinalCapturePayment>()
                        .When(IsFullCapture, "captured total must satisfy required amount before fulfillment can start")
                        .GoTo(OrderDemoState.Picking);
                    region.State(OrderDemoState.Picking)
                        .On<StartPacking>()
                        .GoTo(OrderDemoState.Packed);
                    region.State(OrderDemoState.Packed)
                        .On<ShipOrder>()
                        .GoTo(OrderDemoState.Shipped);
                    region.Terminal(OrderDemoState.Shipped);
                });

                parallel.Region("Billing", region =>
                {
                    region.Initial(OrderDemoState.PaymentPending)
                        .On<AuthorizePayment>()
                        .GoTo(OrderDemoState.PaymentAuthorized);

                    region.State(OrderDemoState.PaymentAuthorized)
                        .On<PartialCapturePayment>()
                        .When(IsPositiveCapture, "capture amount must be positive")
                        .GoTo(OrderDemoState.PaymentPartiallyCaptured)
                        .On<FinalCapturePayment>()
                        .When(IsPositiveCapture, "capture amount must be positive")
                        .When(IsFullCapture, "captured total must satisfy required amount")
                        .GoTo(OrderDemoState.PaymentCaptured);

                    region.State(OrderDemoState.PaymentPartiallyCaptured)
                        .On<PartialCapturePayment>()
                        .When(IsPositiveCapture, "capture amount must be positive")
                        .GoTo(OrderDemoState.PaymentPartiallyCaptured)
                        .On<FinalCapturePayment>()
                        .When(IsPositiveCapture, "capture amount must be positive")
                        .When(IsFullCapture, "captured total must satisfy required amount")
                        .GoTo(OrderDemoState.PaymentCaptured);

                    region.Terminal(OrderDemoState.PaymentCaptured);
                });

                parallel.Region("Compliance", region =>
                {
                    region.Initial(OrderDemoState.FraudCheckPending)
                        .On<ClearFraudCheck>()
                        .GoTo(OrderDemoState.FraudCheckCleared);
                    region.Terminal(OrderDemoState.FraudCheckCleared);
                });
            });

            builder.State(OrderDemoState.OnHold)
                .On<ResumeProcessing>()
                .GoTo(OrderDemoState.Processing)
                .On<CancelOrder>()
                .GoTo(OrderDemoState.Cancelled);

            builder.State(OrderDemoState.Completed).Terminal();
            builder.State(OrderDemoState.Cancelled).Terminal();
        });
    }

    private static bool IsPositiveCapture(TransitionContext<OrderDemoState, OrderDemoEvent> context)
    {
        return ((CapturePayment)context.Event).Amount > 0m;
    }

    private static bool IsFullCapture(TransitionContext<OrderDemoState, OrderDemoEvent> context)
    {
        var capture = (CapturePayment)context.Event;
        return capture.RequiredAmount > 0m && capture.CapturedTotal >= capture.RequiredAmount;
    }
}
