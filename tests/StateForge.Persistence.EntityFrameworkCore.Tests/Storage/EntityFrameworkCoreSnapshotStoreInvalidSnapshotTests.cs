using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreInvalidSnapshotTests
{
    [Fact]
    public async Task LoadAsync_ReturnsInvalidSnapshot_WhenDefinitionIdentityMismatches()
    {
        await using var context = CreateContext();
        context.Snapshots.Add(new StateForgeSnapshotRecord
        {
            InstanceId = "order-1",
            DefinitionId = "other-definition",
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

        var result = await sut.LoadAsync("order-1", EntityFrameworkCorePersistenceTestDomain.DefinitionId);

        Assert.Equal(SnapshotLoadCategory.InvalidSnapshot, result.Category);
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
