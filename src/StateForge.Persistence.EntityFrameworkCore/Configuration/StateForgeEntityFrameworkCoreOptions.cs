using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Serialization;
using StateForge.Persistence.EntityFrameworkCore.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Configuration for <see cref="Storage.EntityFrameworkCoreSnapshotStore{TState}"/>.
/// </summary>
public sealed class StateForgeEntityFrameworkCoreOptions<TState>
{
    public string TableName { get; set; } = "StateForgeSnapshots";

    public string? Schema { get; set; }

    public Func<DbContext, DbSet<StateForgeSnapshotRecord>> SnapshotSetResolver { get; set; } =
        context => context.Set<StateForgeSnapshotRecord>();

    public IStateValueConverter<TState> StateValueConverter { get; set; } = StateValueConverters.CreateDefault<TState>();

    public ISnapshotPayloadConverter SnapshotPayloadConverter { get; set; } =
        StateValueConverters.CreateDefaultPayloadConverter();

    /// <summary>
    /// Expected version numeric value that signals create semantics.
    /// </summary>
    public long CreateExpectedVersion { get; set; } = 0;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TableName))
            throw new ArgumentException("Table name is required.", nameof(TableName));

        ArgumentNullException.ThrowIfNull(SnapshotSetResolver);
        ArgumentNullException.ThrowIfNull(StateValueConverter);
        ArgumentNullException.ThrowIfNull(SnapshotPayloadConverter);
    }
}
