using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreConcurrencyTests
{
    [Fact]
    public async Task SaveAsync_Updates_WhenExpectedVersionMatches()
    {
        await using var context = CreateContext();
        context.Snapshots.Add(new StateForgeSnapshotRecord
        {
            InstanceId = "order-1",
            DefinitionId = EntityFrameworkCorePersistenceTestDomain.DefinitionId,
            ActiveState = EfOrderState.Draft.ToString(),
            Version = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var sut = CreateStore(context);
        var proposed = EntityFrameworkCorePersistenceTestDomain.Snapshot(state: EfOrderState.Submitted, version: 1);

        var result = await sut.SaveAsync(PersistenceVersion.From(1L), proposed);

        Assert.Equal(SnapshotSaveCategory.Saved, result.Category);
        Assert.Equal(2L, (long)result.CommittedSnapshot!.Version.Value);
    }

    [Fact]
    public async Task SaveAsync_ReturnsConflict_WhenExpectedVersionIsStale()
    {
        await using var context = CreateContext();
        context.Snapshots.Add(new StateForgeSnapshotRecord
        {
            InstanceId = "order-1",
            DefinitionId = EntityFrameworkCorePersistenceTestDomain.DefinitionId,
            ActiveState = EfOrderState.Draft.ToString(),
            Version = 2,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var sut = CreateStore(context);
        var proposed = EntityFrameworkCorePersistenceTestDomain.Snapshot(state: EfOrderState.Submitted, version: 1);

        var result = await sut.SaveAsync(PersistenceVersion.From(1L), proposed);

        Assert.Equal(SnapshotSaveCategory.ConcurrentStateChange, result.Category);
        Assert.Null(result.CommittedSnapshot);
    }

    private static EntityFrameworkCoreSnapshotStore<EfOrderState> CreateStore(AdapterTestDbContext context)
    {
        return new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
