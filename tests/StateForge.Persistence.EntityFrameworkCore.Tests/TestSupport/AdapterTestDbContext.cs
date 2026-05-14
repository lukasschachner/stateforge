using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;

internal sealed class AdapterTestDbContext : DbContext
{
    public AdapterTestDbContext(DbContextOptions<AdapterTestDbContext> options) : base(options)
    {
    }

    public DbSet<StateForgeSnapshotRecord> Snapshots => Set<StateForgeSnapshotRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureStateForgeSnapshotRecord();
    }
}
