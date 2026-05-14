using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreLoadTests
{
    [Fact]
    public async Task LoadAsync_ReturnsMissingSnapshot_WhenRowDoesNotExist()
    {
        await using var context = CreateContext();
        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        var result = await sut.LoadAsync("unknown", EntityFrameworkCorePersistenceTestDomain.DefinitionId);

        Assert.Equal(SnapshotLoadCategory.MissingSnapshot, result.Category);
        Assert.Null(result.Snapshot);
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
