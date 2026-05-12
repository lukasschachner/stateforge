using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Persistence;

namespace Persistence.Tests.TestSupport;

internal enum OrderState
{
    Draft,
    Submitted,
    Paid,
    Cancelled
}

internal abstract record OrderEvent;

internal sealed record Submit : OrderEvent;

internal sealed record Pay : OrderEvent;

internal sealed record Cancel : OrderEvent;

internal sealed record Reject(string Reason) : OrderEvent;

internal static class PersistenceTestDomain
{
    public const string DefinitionId = "orders-v1";

    public static StateMachineDefinition<OrderState, OrderEvent> CreateDefinition()
    {
        return StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, DefinitionId);

            builder.State(OrderState.Draft)
                .On<Submit>()
                .GoTo(OrderState.Submitted);

            builder.State(OrderState.Submitted)
                .On<Pay>()
                .GoTo(OrderState.Paid)
                .On<Cancel>()
                .GoTo(OrderState.Cancelled)
                .On<Reject>()
                .GoTo(OrderState.Draft);

            builder.State(OrderState.Paid).Terminal();
            builder.State(OrderState.Cancelled).Terminal();
        });
    }
}