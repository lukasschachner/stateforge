using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Diagnostics;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Storage;

/// <summary>
/// EF Core-backed <see cref="IStateSnapshotStore{TState}"/> implementation.
/// </summary>
/// <remarks>
/// The application owns DbContext provider configuration, migrations, connection lifetime, and transactions.
/// This store executes one load/update operation against the supplied context and does not configure providers,
/// migrations, hosted background services, retries, or distributed locks.
/// </remarks>
public sealed class EntityFrameworkCoreSnapshotStore<TState> : IStateSnapshotStore<TState>
{
    private readonly DbContext _dbContext;
    private readonly StateForgeEntityFrameworkCoreOptions<TState> _options;
    private readonly DbSet<StateForgeSnapshotRecord> _snapshots;

    public EntityFrameworkCoreSnapshotStore(
        DbContext dbContext,
        StateForgeEntityFrameworkCoreOptions<TState>? options = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _options = options ?? new StateForgeEntityFrameworkCoreOptions<TState>();
        _options.Validate();

        _snapshots = _options.SnapshotSetResolver(dbContext)
                     ?? throw new ArgumentException("Snapshot set resolver returned null DbSet.", nameof(options));
    }

    public async ValueTask<SnapshotLoadResult<TState>> LoadAsync(string instanceId, string definitionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidInput(
                    "Instance identity is required.",
                    "efcore.input.instance-id-required",
                    nameof(instanceId)));

        if (string.IsNullOrWhiteSpace(definitionId))
            return SnapshotLoadResult<TState>.InvalidSnapshot(
                EntityFrameworkCorePersistenceDiagnostics.InvalidInput(
                    "Definition identity is required.",
                    "efcore.input.definition-id-required",
                    nameof(definitionId)));

        try
        {
            var record = await _snapshots
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.InstanceId == instanceId, cancellationToken)
                .ConfigureAwait(false);

            if (record is null)
                return SnapshotLoadResult<TState>.MissingSnapshot(
                    new PersistenceDiagnostics("No stored snapshot exists for the instance.",
                        code: "efcore.snapshot-missing"));

            return EntityFrameworkCoreSnapshotMapper.ToLoadResult(
                record,
                definitionId,
                _options.StateValueConverter,
                _options.SnapshotPayloadConverter);
        }
        catch (OperationCanceledException)
        {
            return SnapshotLoadResult<TState>.Cancelled(EntityFrameworkCorePersistenceDiagnostics.Cancelled("load"));
        }
        catch (Exception ex)
        {
            return SnapshotLoadResult<TState>.StorageFailure(
                EntityFrameworkCorePersistenceDiagnostics.StorageFailure("load", ex));
        }
    }

    public async ValueTask<SnapshotSaveResult<TState>> SaveAsync(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, CancellationToken cancellationToken = default)
    {
        if (proposedSnapshot is null)
            throw new ArgumentNullException(nameof(proposedSnapshot));

        if (string.IsNullOrWhiteSpace(proposedSnapshot.InstanceId))
            return SnapshotSaveResult<TState>.InvalidSnapshot(
                expectedVersion,
                proposedSnapshot,
                EntityFrameworkCorePersistenceDiagnostics.InvalidInput(
                    "Snapshot instance identity is required.",
                    "efcore.input.instance-id-required",
                    nameof(proposedSnapshot.InstanceId)));

        if (string.IsNullOrWhiteSpace(proposedSnapshot.DefinitionId))
            return SnapshotSaveResult<TState>.InvalidSnapshot(
                expectedVersion,
                proposedSnapshot,
                EntityFrameworkCorePersistenceDiagnostics.InvalidInput(
                    "Snapshot definition identity is required.",
                    "efcore.input.definition-id-required",
                    nameof(proposedSnapshot.DefinitionId)));

        try
        {
            var existing = await _snapshots
                .SingleOrDefaultAsync(r => r.InstanceId == proposedSnapshot.InstanceId, cancellationToken)
                .ConfigureAwait(false);

            var createPath = IsCreateExpectedVersion(expectedVersion);
            if (existing is null)
            {
                if (!createPath)
                    return SnapshotSaveResult<TState>.MissingSnapshot(expectedVersion, proposedSnapshot,
                        new PersistenceDiagnostics("Stored snapshot does not exist for update.",
                            code: "efcore.snapshot-missing"));

                var newRecord = EntityFrameworkCoreSnapshotMapper.ToNewRecord(
                    proposedSnapshot,
                    _options.StateValueConverter,
                    _options.SnapshotPayloadConverter,
                    DateTimeOffset.UtcNow);

                await _snapshots.AddAsync(newRecord, cancellationToken).ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var committed = new StateSnapshot<TState>(
                    newRecord.InstanceId,
                    newRecord.DefinitionId,
                    _options.StateValueConverter.ConvertFromStorage(newRecord.ActiveState),
                    PersistenceVersion.From(newRecord.Version, newRecord.Version.ToString()),
                    _options.SnapshotPayloadConverter.ConvertFromStorage(newRecord.Payload));

                return SnapshotSaveResult<TState>.Saved(expectedVersion, proposedSnapshot, committed);
            }

            if (createPath)
                return SnapshotSaveResult<TState>.ConcurrentStateChange(
                    expectedVersion,
                    proposedSnapshot,
                    PersistenceVersion.From(existing.Version, existing.Version.ToString()),
                    EntityFrameworkCorePersistenceDiagnostics.ConcurrentStateChange(existing.Version));

            if (!TryReadVersion(expectedVersion, out var expectedNumeric))
                return SnapshotSaveResult<TState>.InvalidSnapshot(
                    expectedVersion,
                    proposedSnapshot,
                    EntityFrameworkCorePersistenceDiagnostics.InvalidInput(
                        "Expected version must be numeric for EF Core adapter.",
                        "efcore.input.expected-version-invalid",
                        nameof(expectedVersion)));

            if (expectedNumeric != existing.Version)
                return SnapshotSaveResult<TState>.ConcurrentStateChange(
                    expectedVersion,
                    proposedSnapshot,
                    PersistenceVersion.From(existing.Version, existing.Version.ToString()),
                    EntityFrameworkCorePersistenceDiagnostics.ConcurrentStateChange(existing.Version));

            EntityFrameworkCoreSnapshotMapper.ApplyToExistingRecord(
                existing,
                proposedSnapshot,
                _options.StateValueConverter,
                _options.SnapshotPayloadConverter,
                DateTimeOffset.UtcNow);

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var committedSnapshot = new StateSnapshot<TState>(
                existing.InstanceId,
                existing.DefinitionId,
                _options.StateValueConverter.ConvertFromStorage(existing.ActiveState),
                PersistenceVersion.From(existing.Version, existing.Version.ToString()),
                _options.SnapshotPayloadConverter.ConvertFromStorage(existing.Payload));

            return SnapshotSaveResult<TState>.Saved(expectedVersion, proposedSnapshot, committedSnapshot);
        }
        catch (OperationCanceledException)
        {
            return SnapshotSaveResult<TState>.Cancelled(expectedVersion, proposedSnapshot,
                EntityFrameworkCorePersistenceDiagnostics.Cancelled("save"));
        }
        catch (DbUpdateConcurrencyException)
        {
            return SnapshotSaveResult<TState>.ConcurrentStateChange(
                expectedVersion,
                proposedSnapshot,
                diagnostics: EntityFrameworkCorePersistenceDiagnostics.ConcurrentStateChange());
        }
        catch (InvalidOperationException ex)
        {
            return SnapshotSaveResult<TState>.InvalidSnapshot(
                expectedVersion,
                proposedSnapshot,
                EntityFrameworkCorePersistenceDiagnostics.InvalidSnapshot(
                    ex.Message,
                    "efcore.snapshot-invalid"));
        }
        catch (Exception ex)
        {
            return SnapshotSaveResult<TState>.StorageFailure(
                expectedVersion,
                proposedSnapshot,
                EntityFrameworkCorePersistenceDiagnostics.StorageFailure("save", ex));
        }
    }

    private bool IsCreateExpectedVersion(PersistenceVersion expectedVersion)
    {
        return TryReadVersion(expectedVersion, out var numeric) && numeric == _options.CreateExpectedVersion;
    }

    private static bool TryReadVersion(PersistenceVersion expectedVersion, out long value)
    {
        if (expectedVersion.Value is long l)
        {
            value = l;
            return true;
        }

        if (expectedVersion.Value is int i)
        {
            value = i;
            return true;
        }

        if (expectedVersion.Value is string s && long.TryParse(s, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = default;
        return false;
    }
}
