using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreSaveReloadTests
{
    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsSnapshot()
    {
        await using var context = CreateContext();
        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        var proposed = EntityFrameworkCorePersistenceTestDomain.Snapshot(version: 1);
        var save = await sut.SaveAsync(PersistenceVersion.From(0L), proposed);

        Assert.Equal(SnapshotSaveCategory.Saved, save.Category);
        Assert.NotNull(save.CommittedSnapshot);

        var load = await sut.LoadAsync(proposed.InstanceId, proposed.DefinitionId);

        Assert.Equal(SnapshotLoadCategory.Loaded, load.Category);
        Assert.NotNull(load.Snapshot);
        Assert.Equal(EfOrderState.Draft, load.Snapshot!.ActiveState);
        Assert.Equal(1L, (long)load.Snapshot.Version.Value);
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
