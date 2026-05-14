using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.Storage;

/// <summary>Application-owned storage participant for loading and saving state snapshots.</summary>
public interface IStateSnapshotStore<TState>
{
    /// <summary>
    ///     Loads a snapshot for the specified machine instance and definition identity.
    /// </summary>
    ValueTask<SnapshotLoadResult<TState>> LoadAsync(
        string instanceId,
        string definitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempts to save <paramref name="proposedSnapshot" /> only if storage still matches
    ///     <paramref name="expectedVersion" />.
    /// </summary>
    /// <remarks>
    ///     Implementations must report optimistic concurrency mismatches as
    ///     <see cref="SnapshotSaveCategory.ConcurrentStateChange" /> and must not return a committed
    ///     snapshot for rejected expected-version writes. Implementations should not auto-retry.
    /// </remarks>
    ValueTask<SnapshotSaveResult<TState>> SaveAsync(
        PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot,
        CancellationToken cancellationToken = default);
}