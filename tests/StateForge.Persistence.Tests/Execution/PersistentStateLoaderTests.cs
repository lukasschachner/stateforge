using StateForge.Persistence.Tests.TestSupport;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.Execution;

public class PersistentStateLoaderTests
{
    [Fact]
    public async Task ReloadReturnsLoadedWhenSnapshotMatchesDefinition()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore();
        store.NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(new StateSnapshot<OrderState>(
            "order-1",
            PersistenceTestDomain.DefinitionId,
            OrderState.Submitted,
            PersistenceVersion.From("v1")));

        var result = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.Loaded, result.Category);
        Assert.Equal(OrderState.Submitted, result.Snapshot!.ActiveState);
    }

    [Fact]
    public async Task ReloadReturnsMissingWhenSnapshotNotFound()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.MissingSnapshot()
        };

        var result = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.MissingSnapshot, result.Category);
    }

    [Fact]
    public async Task ReloadReturnsInvalidWhenDefinitionMismatch()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(new StateSnapshot<OrderState>(
                "order-1",
                "different",
                OrderState.Draft,
                PersistenceVersion.From("v1")))
        };

        var result = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.InvalidSnapshot, result.Category);
    }

    [Fact]
    public async Task ReloadReturnsInvalidWhenStateUnknown()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(new StateSnapshot<OrderState>(
                "order-1",
                PersistenceTestDomain.DefinitionId,
                (OrderState)999,
                PersistenceVersion.From("v1")))
        };

        var result = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.InvalidSnapshot, result.Category);
    }

    [Fact]
    public async Task ReloadReturnsCancelled()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Cancelled()
        };

        var result = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.Cancelled, result.Category);
    }
}