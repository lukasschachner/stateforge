using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

internal enum SnapshotState
{
    Idle,
    Running,
    RunningChild,
    RunningDone,
    Operational,
    FulfillmentWaiting,
    FulfillmentPacking,
    FulfillmentDone,
    BillingWaiting,
    BillingCapturing,
    BillingDone,
    Complete,
    Other
}

internal enum SnapshotEvent
{
    Start,
    Advance,
    Pack,
    Bill,
    Finish,
    Reset
}

internal static class ActiveStateSnapshotTestDomain
{
    public static StateMachineDefinition<SnapshotState, SnapshotEvent> CreateFlatDefinition()
    {
        return StateMachineDefinition<SnapshotState, SnapshotEvent>.Create(builder =>
        {
            builder.State(SnapshotState.Idle).On(SnapshotEvent.Start).GoTo(SnapshotState.Running);
            builder.State(SnapshotState.Running).On(SnapshotEvent.Finish).GoTo(SnapshotState.Complete);
            builder.State(SnapshotState.Complete).Terminal();
        });
    }

    public static StateMachineDefinition<SnapshotState, SnapshotEvent> CreateHierarchicalDefinition()
    {
        return StateMachineDefinition<SnapshotState, SnapshotEvent>.Create(builder =>
        {
            builder.State(SnapshotState.Running).InitialChild(SnapshotState.RunningChild);
            builder.State(SnapshotState.RunningChild).On(SnapshotEvent.Finish).GoTo(SnapshotState.RunningDone);
            builder.State(SnapshotState.RunningDone).ChildOf(SnapshotState.Running).Terminal();
        });
    }

    public static StateMachineDefinition<SnapshotState, SnapshotEvent> CreateParallelDefinition(string? fingerprint = null)
    {
        return StateMachineDefinition<SnapshotState, SnapshotEvent>.Create(builder =>
        {
            if (fingerprint is not null)
                builder.WithMetadata(StateMachineMetadataKeys.DefinitionFingerprint, fingerprint);

            builder.ParallelComposite(SnapshotState.Operational)
                .Region("Fulfillment", SnapshotState.FulfillmentWaiting,
                    new[]
                    {
                        SnapshotState.FulfillmentWaiting,
                        SnapshotState.FulfillmentPacking,
                        SnapshotState.FulfillmentDone
                    }, new[] { SnapshotState.FulfillmentDone })
                .Region("Billing", SnapshotState.BillingWaiting,
                    new[]
                    {
                        SnapshotState.BillingWaiting,
                        SnapshotState.BillingCapturing,
                        SnapshotState.BillingDone
                    }, new[] { SnapshotState.BillingDone });

            builder.State(SnapshotState.FulfillmentWaiting).On(SnapshotEvent.Pack)
                .GoTo(SnapshotState.FulfillmentPacking);
            builder.State(SnapshotState.FulfillmentPacking).On(SnapshotEvent.Finish)
                .GoTo(SnapshotState.FulfillmentDone);
            builder.State(SnapshotState.BillingWaiting).On(SnapshotEvent.Bill)
                .GoTo(SnapshotState.BillingCapturing);
            builder.State(SnapshotState.BillingCapturing).On(SnapshotEvent.Finish)
                .GoTo(SnapshotState.BillingDone);
        });
    }
}
