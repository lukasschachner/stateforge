using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.TestSupport;

internal sealed class ApplicationOwnedSampleStore : IStateSnapshotStore<OrderState>
{
    private readonly Dictionary<string, StateSnapshot<OrderState>> _rows = new(StringComparer.Ordinal);

    public ValueTask<SnapshotLoadResult<OrderState>> LoadAsync(string instanceId, string definitionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_rows.TryGetValue(instanceId, out var snapshot))
            return ValueTask.FromResult(SnapshotLoadResult<OrderState>.MissingSnapshot());

        return ValueTask.FromResult(SnapshotLoadResult<OrderState>.Loaded(snapshot));
    }

    public ValueTask<SnapshotSaveResult<OrderState>> SaveAsync(PersistenceVersion expectedVersion,
        StateSnapshot<OrderState> proposedSnapshot, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_rows.TryGetValue(proposedSnapshot.InstanceId, out var current))
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.MissingSnapshot(expectedVersion, proposedSnapshot));

        if (!Equals(current.Version.Value, expectedVersion.Value))
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.ConcurrentStateChange(expectedVersion, proposedSnapshot,
                    current.Version));

        var newVersion = IncrementVersion(current.Version);
        var committed = new StateSnapshot<OrderState>(
            proposedSnapshot.InstanceId,
            proposedSnapshot.DefinitionId,
            proposedSnapshot.ActiveState,
            newVersion,
            proposedSnapshot.Properties);

        _rows[proposedSnapshot.InstanceId] = committed;
        return ValueTask.FromResult(SnapshotSaveResult<OrderState>.Saved(expectedVersion, proposedSnapshot, committed));
    }

    public void Seed(StateSnapshot<OrderState> snapshot)
    {
        _rows[snapshot.InstanceId] = snapshot;
    }

    private static PersistenceVersion IncrementVersion(PersistenceVersion version)
    {
        var text = version.Value.ToString();
        if (text is not null && text.StartsWith("v", StringComparison.Ordinal) &&
            int.TryParse(text[1..], out var number)) return PersistenceVersion.From($"v{number + 1}");

        return PersistenceVersion.From(Guid.NewGuid().ToString("N"));
    }
}