using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Introspection;

internal static class GraphExportTestData
{
    public static StateMachineDefinition<OrderState, OrderEvent> CreateValidOrderDefinition()
    {
        return StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.WithMetadata("title", "Order lifecycle");

            builder.State(OrderState.Created)
                .WithMetadata("description", "Order was created")
                .On<Pay>()
                .When(_ => true, "payment amount is positive")
                .WithMetadata("businessRule", "Payment must be captured before shipping")
                .GoTo(OrderState.Paid)
                .On<Cancel>()
                .GoTo(OrderState.Cancelled);

            builder.State(OrderState.Paid)
                .On<Ship>()
                .GoTo(OrderState.Shipped)
                .On<Cancel>()
                .GoTo(OrderState.Cancelled);

            builder.State(OrderState.Shipped).Terminal();
            builder.State(OrderState.Cancelled).Terminal();
        });
    }
}