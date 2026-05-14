using StateForge.Core.Definitions;
using StateForge.Persistence;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

internal static class Program
{
    private const string DefinitionId = "orders-v1";

    private static async Task Main()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, DefinitionId);
            builder.State(OrderState.Draft).On<Submit>().GoTo(OrderState.Submitted);
            builder.State(OrderState.Submitted)
                .On<Pay>().GoTo(OrderState.Paid)
                .On<Cancel>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid).Terminal();
            builder.State(OrderState.Cancelled).Terminal();
        });

        var store = new ApplicationOwnedStore();
        store.Seed(new StateSnapshot<OrderState>("order-1", DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1")));

        var load = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);
        if (load.Category != SnapshotLoadCategory.Loaded) throw new InvalidOperationException(load.Diagnostics.Summary);

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
        var result = await runtime.ApplyAndPersistAsync(new Submit());
        if (result.PersistenceCategory != TransitionPersistenceCategory.Persisted)
            throw new InvalidOperationException(result.Diagnostics.Summary);

        Console.WriteLine($"Persistence sample completed: {result.CommittedSnapshot!.ActiveState}");
    }
}

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

internal sealed class ApplicationOwnedStore : IStateSnapshotStore<OrderState>
{
    private readonly Dictionary<string, StateSnapshot<OrderState>> rows = new(StringComparer.Ordinal);

    public ValueTask<SnapshotLoadResult<OrderState>> LoadAsync(string instanceId, string definitionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(rows.TryGetValue(instanceId, out var snapshot)
            ? SnapshotLoadResult<OrderState>.Loaded(snapshot)
            : SnapshotLoadResult<OrderState>.MissingSnapshot());
    }

    public ValueTask<SnapshotSaveResult<OrderState>> SaveAsync(PersistenceVersion expectedVersion,
        StateSnapshot<OrderState> proposedSnapshot, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!rows.TryGetValue(proposedSnapshot.InstanceId, out var current))
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.MissingSnapshot(expectedVersion, proposedSnapshot));
        if (!Equals(current.Version.Value, expectedVersion.Value))
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.ConcurrentStateChange(expectedVersion, proposedSnapshot,
                    current.Version));
        var committed = new StateSnapshot<OrderState>(proposedSnapshot.InstanceId, proposedSnapshot.DefinitionId,
            proposedSnapshot.ActiveState, PersistenceVersion.From("v2"), proposedSnapshot.Properties);
        rows[proposedSnapshot.InstanceId] = committed;
        return ValueTask.FromResult(SnapshotSaveResult<OrderState>.Saved(expectedVersion, proposedSnapshot, committed));
    }

    public void Seed(StateSnapshot<OrderState> snapshot)
    {
        rows[snapshot.InstanceId] = snapshot;
    }
}