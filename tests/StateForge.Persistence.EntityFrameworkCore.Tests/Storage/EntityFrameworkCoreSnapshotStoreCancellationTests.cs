using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreCancellationTests
{
    [Fact]
    public async Task LoadAsync_ReturnsCancelled_WhenTokenIsCancelled()
    {
        await using var context = CreateContext();
        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await sut.LoadAsync("order-1", EntityFrameworkCorePersistenceTestDomain.DefinitionId, cts.Token);

        Assert.Equal(SnapshotLoadCategory.Cancelled, result.Category);
    }

    [Fact]
    public async Task SaveAsync_ReturnsCancelled_WhenTokenIsCancelled()
    {
        await using var context = CreateContext();
        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context,
            new StateForgeEntityFrameworkCoreOptions<EfOrderState>
            {
                SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots
            });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await sut.SaveAsync(
            PersistenceVersion.From(0L),
            EntityFrameworkCorePersistenceTestDomain.Snapshot(),
            cts.Token);

        Assert.Equal(SnapshotSaveCategory.Cancelled, result.Category);
    }

    private static AdapterTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AdapterTestDbContext(options);
    }
}
