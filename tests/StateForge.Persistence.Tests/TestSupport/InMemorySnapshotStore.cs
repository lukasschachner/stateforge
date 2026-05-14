using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.TestSupport;

internal sealed class InMemorySnapshotStore : IStateSnapshotStore<OrderState>
{
    private StateSnapshot<OrderState>? _current;

    public SnapshotLoadResult<OrderState>? NextLoadResult { get; set; }
    public SnapshotSaveResult<OrderState>? NextSaveResult { get; set; }

    public int LoadCallCount { get; private set; }
    public int SaveCallCount { get; private set; }

    public string? LastLoadInstanceId { get; private set; }
    public string? LastLoadDefinitionId { get; private set; }

    public ValueTask<SnapshotLoadResult<OrderState>> LoadAsync(string instanceId, string definitionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LoadCallCount++;
        LastLoadInstanceId = instanceId;
        LastLoadDefinitionId = definitionId;

        if (NextLoadResult is not null)
        {
            if (NextLoadResult.Category == SnapshotLoadCategory.Loaded && NextLoadResult.Snapshot is not null)
                _current = NextLoadResult.Snapshot;

            return ValueTask.FromResult(NextLoadResult);
        }

        if (_current is null) return ValueTask.FromResult(SnapshotLoadResult<OrderState>.MissingSnapshot());

        return ValueTask.FromResult(SnapshotLoadResult<OrderState>.Loaded(_current));
    }

    public ValueTask<SnapshotSaveResult<OrderState>> SaveAsync(PersistenceVersion expectedVersion,
        StateSnapshot<OrderState> proposedSnapshot, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaveCallCount++;

        if (NextSaveResult is not null) return ValueTask.FromResult(NextSaveResult);

        if (_current is null)
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.MissingSnapshot(expectedVersion, proposedSnapshot));

        if (!Equals(_current.Version.Value, expectedVersion.Value))
            return ValueTask.FromResult(
                SnapshotSaveResult<OrderState>.ConcurrentStateChange(expectedVersion, proposedSnapshot,
                    _current.Version));

        var nextVersion = NextVersion(_current.Version);
        _current = new StateSnapshot<OrderState>(
            proposedSnapshot.InstanceId,
            proposedSnapshot.DefinitionId,
            proposedSnapshot.ActiveState,
            nextVersion,
            proposedSnapshot.Properties);

        return ValueTask.FromResult(SnapshotSaveResult<OrderState>.Saved(expectedVersion, proposedSnapshot, _current));
    }

    public void Seed(StateSnapshot<OrderState> snapshot)
    {
        _current = snapshot;
    }

    private static PersistenceVersion NextVersion(PersistenceVersion current)
    {
        var text = current.Value.ToString();
        if (text is not null && text.Length > 1 && text[0] == 'v' && int.TryParse(text[1..], out var number))
            return PersistenceVersion.From($"v{number + 1}");

        return PersistenceVersion.From(Guid.NewGuid().ToString("N"));
    }
}