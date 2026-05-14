using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Configuration;

public static class StateForgeModelBuilderExtensions
{
    public static ModelBuilder ConfigureStateForgeSnapshotRecord(
        this ModelBuilder modelBuilder,
        string tableName = "StateForgeSnapshots",
        string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var entity = modelBuilder.Entity<StateForgeSnapshotRecord>();
        entity.HasKey(x => x.InstanceId);

        entity.Property(x => x.InstanceId)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(x => x.DefinitionId)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(x => x.ActiveState)
            .IsRequired()
            .HasMaxLength(1024);

        entity.Property(x => x.Payload);

        entity.Property(x => x.Version)
            .IsConcurrencyToken()
            .IsRequired();

        entity.Property(x => x.CreatedAtUtc)
            .IsRequired();

        entity.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        return modelBuilder;
    }
}
