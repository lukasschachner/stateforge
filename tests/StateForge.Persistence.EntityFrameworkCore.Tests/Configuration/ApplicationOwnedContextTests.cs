using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Configuration;

public sealed class ApplicationOwnedContextTests
{
    [Fact]
    public async Task Store_UsesApplicationOwnedContextWithoutProviderConfiguration()
    {
        await using var context = new AdapterTestDbContext(
            new DbContextOptionsBuilder<AdapterTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options);

        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        var result = await sut.LoadAsync("unknown", EntityFrameworkCorePersistenceTestDomain.DefinitionId);

        Assert.NotNull(result);
    }
}
