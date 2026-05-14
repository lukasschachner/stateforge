using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.EntityFrameworkCore.Diagnostics;
using StateForge.Persistence.EntityFrameworkCore.Serialization;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Storage;

internal static class EntityFrameworkCoreSnapshotMapper
{
    public static SnapshotLoadResult<TState> ToLoadResult<TState>(
        StateForgeSnapshotRecord record,
        string expectedDefinitionId,
        IStateValueConverter<TState> stateValueConverter,
        ISnapshotPayloadConverter payloadConverter)
    {
        if (string.IsNullOrWhiteSpace(record.InstanceId))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    "Stored snapshot record is missing instance identity.",
                    "efcore.record.instance-id-required",
                    nameof(StateForgeSnapshotRecord.InstanceId)));

        if (string.IsNullOrWhiteSpace(record.DefinitionId))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    "Stored snapshot record is missing definition identity.",
                    "efcore.record.definition-id-required",
                    nameof(StateForgeSnapshotRecord.DefinitionId)));

        if (!string.Equals(record.DefinitionId, expectedDefinitionId, StringComparison.Ordinal))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    "Stored snapshot definition identity does not match the requested definition.",
                    "efcore.record.definition-id-mismatch",
                    nameof(StateForgeSnapshotRecord.DefinitionId)));

        if (string.IsNullOrWhiteSpace(record.ActiveState))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    "Stored snapshot record is missing active state.",
                    "efcore.record.active-state-required",
                    nameof(StateForgeSnapshotRecord.ActiveState)));

        if (record.Version <= 0)
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    "Stored snapshot version must be positive.",
                    "efcore.record.version-invalid",
                    nameof(StateForgeSnapshotRecord.Version)));

        try
        {
            var activeState = stateValueConverter.ConvertFromStorage(record.ActiveState);
            var properties = payloadConverter.ConvertFromStorage(record.Payload);
            var snapshot = new StateSnapshot<TState>(
                record.InstanceId,
                record.DefinitionId,
                activeState,
                PersistenceVersion.From(record.Version, record.Version.ToString()),
                properties);

            return SnapshotLoadResult<TState>.Loaded(snapshot);
        }
        catch (Exception)
        {
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                new PersistenceDiagnostics(
                    "Stored snapshot payload/state conversion failed.",
                    code: "efcore.record.conversion-failure",
                    affectedElement: nameof(StateForgeSnapshotRecord.ActiveState)));
        }
    }

    public static StateForgeSnapshotRecord ToNewRecord<TState>(
        StateSnapshot<TState> snapshot,
        IStateValueConverter<TState> stateValueConverter,
        ISnapshotPayloadConverter payloadConverter,
        DateTimeOffset utcNow)
    {
        ValidateSnapshot(snapshot);

        return new StateForgeSnapshotRecord
        {
            InstanceId = snapshot.InstanceId,
            DefinitionId = snapshot.DefinitionId,
            ActiveState = stateValueConverter.ConvertToStorage(snapshot.ActiveState),
            Payload = payloadConverter.ConvertToStorage(snapshot.Properties),
            Version = 1,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public static void ApplyToExistingRecord<TState>(
        StateForgeSnapshotRecord record,
        StateSnapshot<TState> snapshot,
        IStateValueConverter<TState> stateValueConverter,
        ISnapshotPayloadConverter payloadConverter,
        DateTimeOffset utcNow)
    {
        ValidateSnapshot(snapshot);

        record.DefinitionId = snapshot.DefinitionId;
        record.ActiveState = stateValueConverter.ConvertToStorage(snapshot.ActiveState);
        record.Payload = payloadConverter.ConvertToStorage(snapshot.Properties);
        record.Version++;
        record.UpdatedAtUtc = utcNow;
    }

    private static void ValidateSnapshot<TState>(StateSnapshot<TState> snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(snapshot.InstanceId))
            throw new InvalidOperationException("Snapshot instance identity is required.");

        if (string.IsNullOrWhiteSpace(snapshot.DefinitionId))
            throw new InvalidOperationException("Snapshot definition identity is required.");

        if (snapshot.Version.Value is null)
            throw new InvalidOperationException("Snapshot version is required.");
    }
}
