using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Configuration;

public sealed class StateForgeModelBuilderExtensionsTests
{
    [Fact]
    public void ConfigureStateForgeSnapshotRecord_RegistersEntityAndConcurrencyToken()
    {
        var options = new DbContextOptionsBuilder<CustomMapContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        using var context = new CustomMapContext(options);
        var entityType = context.Model.FindEntityType(typeof(StateForgeSnapshotRecord));

        Assert.NotNull(entityType);
        var version = entityType!.FindProperty(nameof(StateForgeSnapshotRecord.Version));
        Assert.NotNull(version);
        Assert.True(version!.IsConcurrencyToken);
    }

    private sealed class CustomMapContext : DbContext
    {
        public CustomMapContext(DbContextOptions<CustomMapContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureStateForgeSnapshotRecord("CustomSnapshots");
        }
    }
}
