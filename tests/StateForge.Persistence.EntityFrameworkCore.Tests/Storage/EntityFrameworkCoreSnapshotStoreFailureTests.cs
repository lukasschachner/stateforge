using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Storage;

public sealed class EntityFrameworkCoreSnapshotStoreFailureTests
{
    [Fact]
    public async Task SaveAsync_ReturnsStorageFailure_WhenConverterThrows()
    {
        await using var context = new AdapterTestDbContext(new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);

        var options = new StateForgeEntityFrameworkCoreOptions<EfOrderState>
        {
            SnapshotSetResolver = c => ((AdapterTestDbContext)c).Snapshots,
            StateValueConverter = new ThrowingStateConverter()
        };

        var sut = new EntityFrameworkCoreSnapshotStore<EfOrderState>(context, options);
        var proposed = EntityFrameworkCorePersistenceTestDomain.Snapshot();

        var result = await sut.SaveAsync(PersistenceVersion.From(0L), proposed);

        Assert.Equal(SnapshotSaveCategory.InvalidSnapshot, result.Category);
    }

    private sealed class ThrowingStateConverter : StateForge.Persistence.EntityFrameworkCore.Serialization.IStateValueConverter<EfOrderState>
    {
        public string ConvertToStorage(EfOrderState state) => throw new InvalidOperationException("boom");

        public EfOrderState ConvertFromStorage(string value) => throw new InvalidOperationException("boom");
    }
}
