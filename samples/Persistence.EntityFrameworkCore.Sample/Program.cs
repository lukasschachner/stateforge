using Microsoft.EntityFrameworkCore;
using Sample.State;
using StateForge.Persistence.EntityFrameworkCore.Configuration;
using StateForge.Persistence.EntityFrameworkCore.Serialization;
using StateForge.Persistence.EntityFrameworkCore.Storage;
using StateForge.Persistence.Snapshots;

const string InstanceId = "ORD-2026-0001";
const string DefinitionId = "order-persistence-v1";

var dbOptions = new DbContextOptionsBuilder<SampleDbContext>()
    .UseInMemoryDatabase("orders-sample")
    .Options;

await using var dbContext = new SampleDbContext(dbOptions);

var storeOptions = new StateForgeEntityFrameworkCoreOptions<Order>
{
    SnapshotSetResolver = ctx => ((SampleDbContext)ctx).Snapshots,
    StateValueConverter = StateValueConverters.CreateDefault<Order>(),
    SnapshotPayloadConverter = StateValueConverters.CreateDefaultPayloadConverter()
};

var store = new EntityFrameworkCoreSnapshotStore<Order>(dbContext, storeOptions);

// 1) Create new order snapshot
var newOrder = new Order
{
    OrderId = InstanceId,
    Customer = "Acme Corp"
};
newOrder.Positions.Add(new Position { Sku = "SKU-100", Quantity = 2, Price = 12.50m });
newOrder.Positions.Add(new Position { Sku = "SKU-200", Quantity = 1, Price = 5.00m });

var createResult = await store.SaveAsync(
    expectedVersion: PersistenceVersion.From(0L),
    proposedSnapshot: new StateSnapshot<Order>(InstanceId, DefinitionId, newOrder, PersistenceVersion.From(0L)));

Console.WriteLine($"Create: {createResult.Category}");
if (createResult.CommittedSnapshot is null)
{
    Console.WriteLine($"Create failed: {createResult.Diagnostics.Summary}");
    return;
}

// 2) Load and print current order
var loadResult = await store.LoadAsync(InstanceId, DefinitionId);
Console.WriteLine($"Load: {loadResult.Category}");
if (loadResult.Snapshot is null)
{
    Console.WriteLine($"Load failed: {loadResult.Diagnostics.Summary}");
    return;
}

var loadedOrder = loadResult.Snapshot.ActiveState;
Console.WriteLine($"Loaded order for {loadedOrder.Customer}, positions={loadedOrder.Positions.Count}, total={loadedOrder.Total:C}");

// 3) Business update: add one more position and save with optimistic concurrency
loadedOrder.Positions.Add(new Position { Sku = "SKU-300", Quantity = 3, Price = 2.00m });

var updateResult = await store.SaveAsync(
    expectedVersion: loadResult.Snapshot.Version,
    proposedSnapshot: new StateSnapshot<Order>(InstanceId, DefinitionId, loadedOrder, loadResult.Snapshot.Version));

Console.WriteLine($"Update: {updateResult.Category}");
if (updateResult.CommittedSnapshot is not null)
{
    Console.WriteLine($"Updated total: {updateResult.CommittedSnapshot.ActiveState.Total:C}, version={updateResult.CommittedSnapshot.Version}");
}

// 4) Demonstrate stale-write protection with an old version
var staleResult = await store.SaveAsync(
    expectedVersion: PersistenceVersion.From(1L),
    proposedSnapshot: new StateSnapshot<Order>(InstanceId, DefinitionId, loadedOrder, PersistenceVersion.From(1L)));

Console.WriteLine($"Stale write result: {staleResult.Category}");

internal sealed class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
    {
    }

    public DbSet<StateForgeSnapshotRecord> Snapshots => Set<StateForgeSnapshotRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureStateForgeSnapshotRecord();
    }
}
