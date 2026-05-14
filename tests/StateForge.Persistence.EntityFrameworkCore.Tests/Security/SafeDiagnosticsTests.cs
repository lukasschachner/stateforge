using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Security;

public sealed class SafeDiagnosticsTests
{
    [Fact]
    public async Task Diagnostics_DoNotContainPayloadContent_OnInvalidSnapshot()
    {
        await using var context = new AdapterTestDbContext(new DbContextOptionsBuilder<AdapterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);

        context.Snapshots.Add(new StateForgeSnapshotRecord
        {
            InstanceId = "order-1",
            DefinitionId = EntityFrameworkCorePersistenceTestDomain.DefinitionId,
            ActiveState = string.Empty,
            Payload = "secret=token-value",
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

        Assert.DoesNotContain("token-value", result.Diagnostics.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", result.Diagnostics.Summary, StringComparison.OrdinalIgnoreCase);
    }
}
