using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;

namespace Graph.RenderingSample;

internal static class SampleGraphs
{
    public static DefinitionGraph<OrderState, OrderEvent> OrderFlow()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.WithMetadata("title", "Order flow");
            builder.WithMetadata("owner", "docs");

            builder.State(OrderState.Created)
                .WithMetadata("node-role", "entry")
                .On(OrderEvent.Pay)
                .WithMetadata("edge-priority", 1)
                .GoTo(OrderState.Paid)
                .On(OrderEvent.Cancel)
                .WithMetadata("edge-priority", 2)
                .GoTo(OrderState.Cancelled);

            builder.State(OrderState.Paid)
                .WithMetadata("node-role", "in-progress")
                .On(OrderEvent.Ship)
                .WithMetadata("edge-priority", 1)
                .GoTo(OrderState.Shipped)
                .On(OrderEvent.Cancel)
                .WithMetadata("edge-priority", 2)
                .GoTo(OrderState.Cancelled);

            builder.State(OrderState.Shipped)
                .WithMetadata("node-role", "done")
                .Terminal();

            builder.State(OrderState.Cancelled)
                .WithMetadata("node-role", "done")
                .Terminal();
        });

        return ExportValidatedGraph(definition);
    }

    public static DefinitionGraph<CommercialState, CommercialEvent> OfferOrderInvoiceCancellationFlow()
    {
        var definition = StateMachineDefinition<CommercialState, CommercialEvent>.Create(builder =>
        {
            builder.WithMetadata("title", "Offer → Order → Invoice → Invoice Cancellation");
            builder.WithMetadata("owner", "finance-domain");

            builder.State(CommercialState.OfferDraft)
                .WithMetadata("stage", "offer")
                .On(CommercialEvent.SubmitOffer)
                .GoTo(CommercialState.OfferSent);

            builder.State(CommercialState.OfferSent)
                .WithMetadata("stage", "offer")
                .On(CommercialEvent.AcceptOffer)
                .GoTo(CommercialState.OrderCreated)
                .On(CommercialEvent.RejectOffer)
                .GoTo(CommercialState.OfferRejected);

            builder.State(CommercialState.OrderCreated)
                .WithMetadata("stage", "order")
                .On(CommercialEvent.ConfirmOrder)
                .GoTo(CommercialState.OrderConfirmed)
                .On(CommercialEvent.CancelOrder)
                .GoTo(CommercialState.OrderCancelled);

            builder.State(CommercialState.OrderConfirmed)
                .WithMetadata("stage", "order")
                .On(CommercialEvent.CreateInvoice)
                .GoTo(CommercialState.InvoiceDraft)
                .On(CommercialEvent.CancelOrder)
                .GoTo(CommercialState.OrderCancelled);

            builder.State(CommercialState.InvoiceDraft)
                .WithMetadata("stage", "invoice")
                .On(CommercialEvent.IssueInvoice)
                .GoTo(CommercialState.InvoiceIssued);

            builder.State(CommercialState.InvoiceIssued)
                .WithMetadata("stage", "invoice")
                .On(CommercialEvent.RegisterPayment)
                .GoTo(CommercialState.InvoicePaid)
                .On(CommercialEvent.RequestInvoiceCancellation)
                .GoTo(CommercialState.InvoiceCancellationRequested);

            builder.State(CommercialState.InvoiceCancellationRequested)
                .WithMetadata("stage", "invoice-cancellation")
                .On(CommercialEvent.ApproveInvoiceCancellation)
                .GoTo(CommercialState.InvoiceCancelled)
                .On(CommercialEvent.RejectInvoiceCancellation)
                .GoTo(CommercialState.InvoiceIssued);

            builder.State(CommercialState.OfferRejected)
                .WithMetadata("stage", "offer")
                .Terminal();

            builder.State(CommercialState.OrderCancelled)
                .WithMetadata("stage", "order")
                .Terminal();

            builder.State(CommercialState.InvoicePaid)
                .WithMetadata("stage", "invoice")
                .Terminal();

            builder.State(CommercialState.InvoiceCancelled)
                .WithMetadata("stage", "invoice-cancellation")
                .Terminal();
        });

        return ExportValidatedGraph(definition);
    }

    private static DefinitionGraph<TState, TEvent> ExportValidatedGraph<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition)
    {
        var export = definition.ExportGraph();
        if (!export.Succeeded || export.Graph is null)
            throw new InvalidOperationException(export.FailureSummary ?? "Graph export failed.");

        return export.Graph;
    }
}

internal enum OrderState
{
    Created,
    Paid,
    Shipped,
    Cancelled
}

internal enum OrderEvent
{
    Pay,
    Ship,
    Cancel
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
    InvoiceCancelled
}

internal enum CommercialEvent
{
    SubmitOffer,
    AcceptOffer,
    RejectOffer,
    ConfirmOrder,
    CancelOrder,
    CreateInvoice,
    IssueInvoice,
    RegisterPayment,
    RequestInvoiceCancellation,
    ApproveInvoiceCancellation,
    RejectInvoiceCancellation
}