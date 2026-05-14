using StateForge.Core.Definitions;
using StateForge.Core.Execution;

internal static class Program
{
    private static async Task Main()
    {
        var definition = StateMachineDefinition<CommercialState, CommercialEvent>.Create(builder =>
        {
            builder.State(CommercialState.OfferDraft)
                .On<SubmitOffer>()
                .When(ctx => ((SubmitOffer)ctx.Event).NetAmount > 0, "offer amount must be positive")
                .GoTo(CommercialState.OfferSent);

            builder.State(CommercialState.OfferSent)
                .On<AcceptOffer>()
                .GoTo(CommercialState.OrderCreated)
                .On<RejectOffer>()
                .GoTo(CommercialState.OfferRejected);

            builder.State(CommercialState.OrderCreated)
                .On<ConfirmOrder>()
                .GoTo(CommercialState.OrderConfirmed)
                .On<CancelOrder>()
                .GoTo(CommercialState.OrderCancelled);

            builder.State(CommercialState.OrderConfirmed)
                .On<CreateInvoice>()
                .GoTo(CommercialState.InvoiceDraft)
                .On<CancelOrder>()
                .GoTo(CommercialState.OrderCancelled);

            builder.State(CommercialState.InvoiceDraft)
                .On<IssueInvoice>()
                .GoTo(CommercialState.InvoiceIssued);

            builder.State(CommercialState.InvoiceIssued)
                .On<RegisterPayment>()
                .When(ctx => ((RegisterPayment)ctx.Event).Amount > 0, "payment amount must be positive")
                .GoTo(CommercialState.InvoicePaid)
                .On<RequestInvoiceCancellation>()
                .GoTo(CommercialState.InvoiceCancellationRequested);

            builder.State(CommercialState.InvoiceCancellationRequested)
                .On<ApproveInvoiceCancellation>()
                .GoTo(CommercialState.InvoiceCancelled)
                .On<RejectInvoiceCancellation>()
                .GoTo(CommercialState.InvoiceIssued);

            builder.State(CommercialState.OfferRejected).Terminal();
            builder.State(CommercialState.OrderCancelled).Terminal();
            builder.State(CommercialState.InvoicePaid).Terminal();
            builder.State(CommercialState.InvoiceCancelled).Terminal();
        });

        var parallelDefinition = CreateParallelFulfillmentDefinition();
        if (parallelDefinition.Validate().Errors.Count > 0)
            throw new InvalidOperationException("Parallel fulfillment sample definition is invalid.");

        var paymentScenario = await RunScenario(
            definition,
            "payment-after-cancellation-review",
            new CommercialEvent[]
            {
                new SubmitOffer(1250m),
                new AcceptOffer("ACME-2026-04"),
                new ConfirmOrder("ORD-1001"),
                new CreateInvoice("INV-DRAFT-1001"),
                new IssueInvoice("INV-1001"),
                new RequestInvoiceCancellation("customer asked for correction"),
                new RejectInvoiceCancellation("invoice data is correct"),
                new RegisterPayment("PAY-1001", 1250m)
            });

        var cancellationScenario = await RunScenario(
            definition,
            "approved-invoice-cancellation",
            new CommercialEvent[]
            {
                new SubmitOffer(840m),
                new AcceptOffer("NORTHWIND-2026-07"),
                new ConfirmOrder("ORD-2001"),
                new CreateInvoice("INV-DRAFT-2001"),
                new IssueInvoice("INV-2001"),
                new RequestInvoiceCancellation("order withdrawn"),
                new ApproveInvoiceCancellation("finance-team")
            });

        Console.WriteLine($"Core sample completed: payment={paymentScenario}, cancellation={cancellationScenario}");
    }

    private static StateMachineDefinition<CommercialState, CommercialEvent> CreateParallelFulfillmentDefinition()
    {
        return StateMachineDefinition<CommercialState, CommercialEvent>.Create(builder =>
        {
            builder.ParallelComposite(CommercialState.OrderProcessing, composite =>
            {
                composite.Region("Fulfillment", region =>
                {
                    region.Initial(CommercialState.PickPending)
                        .On<ConfirmOrder>()
                        .GoTo(CommercialState.Picked);
                    region.Terminal(CommercialState.Picked);
                });

                composite.Region("Billing", region =>
                {
                    region.Initial(CommercialState.PaymentPending)
                        .On<RegisterPayment>()
                        .GoTo(CommercialState.PaymentCaptured);
                    region.Terminal(CommercialState.PaymentCaptured);
                });
            });
        });
    }

    private static async Task<CommercialState> RunScenario(
        StateMachineDefinition<CommercialState, CommercialEvent> definition,
        string scenarioName,
        IReadOnlyList<CommercialEvent> events)
    {
        var current = CommercialState.OfferDraft;

        foreach (var @event in events)
        {
            var outcome = await definition.ApplyAsync(current, @event);
            if (outcome.Category != TransitionOutcomeCategory.Success)
                throw new InvalidOperationException(
                    $"Scenario '{scenarioName}' failed on '{@event.GetType().Name}': {outcome.Diagnostics.Summary}");

            Console.WriteLine($"[{scenarioName}] {current} --{@event.GetType().Name}--> {outcome.ResultingState}");
            current = outcome.ResultingState;
        }

        return current;
    }
}

internal enum CommercialState
{
    OfferDraft,
    OfferSent,
    OfferRejected,
    OrderCreated,
    OrderConfirmed,
    OrderCancelled,
    InvoiceDraft,
    InvoiceIssued,
    InvoiceCancellationRequested,
    InvoicePaid,
    InvoiceCancelled,
    OrderProcessing,
    PickPending,
    Picked,
    PaymentPending,
    PaymentCaptured
}

internal abstract record CommercialEvent;

internal sealed record SubmitOffer(decimal NetAmount) : CommercialEvent;

internal sealed record AcceptOffer(string CustomerReference) : CommercialEvent;

internal sealed record RejectOffer(string Reason) : CommercialEvent;

internal sealed record ConfirmOrder(string OrderNumber) : CommercialEvent;

internal sealed record CancelOrder(string Reason) : CommercialEvent;

internal sealed record CreateInvoice(string DraftNumber) : CommercialEvent;

internal sealed record IssueInvoice(string InvoiceNumber) : CommercialEvent;

internal sealed record RegisterPayment(string PaymentReference, decimal Amount) : CommercialEvent;

internal sealed record RequestInvoiceCancellation(string Reason) : CommercialEvent;

internal sealed record ApproveInvoiceCancellation(string ApprovedBy) : CommercialEvent;

internal sealed record RejectInvoiceCancellation(string Reason) : CommercialEvent;