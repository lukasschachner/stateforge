using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreValidationTests
{
    [Fact]
    public async Task SaveAsync_ReturnsInvalidSnapshot_WhenExpectedVersionIsNotNumeric()
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

        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        var proposed = EntityFrameworkCorePersistenceTestDomain.Snapshot(version: 1);
        var result = await sut.SaveAsync(PersistenceVersion.From("not-a-number"), proposed);

        Assert.Equal(SnapshotSaveCategory.InvalidSnapshot, result.Category);
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
