using StateForge.Persistence.Tests.TestSupport;
using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.Execution;

public class PersistentStateMachineRuntimeTests
{
    [Fact]
    public async Task ApplyAndPersistReturnsPersistedOnMatchingVersion()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(new StateSnapshot<OrderState>("order-1",
                PersistenceTestDomain.DefinitionId, OrderState.Draft, PersistenceVersion.From("v1")))
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.Persisted, result.PersistenceCategory);
        Assert.NotNull(result.CommittedSnapshot);
    }

    [Fact]
    public async Task ApplyAndPersistReturnsConcurrentStateChangeForStaleVersion()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded),
            NextSaveResult =
                SnapshotSaveResult<OrderState>.ConcurrentStateChange(loaded.Version, proposed,
                    PersistenceVersion.From("v2"))
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.ConcurrentStateChange, result.PersistenceCategory);
        Assert.NotNull(result.ProposedSnapshot);
        Assert.Null(result.CommittedSnapshot);
    }

    [Fact]
    public async Task ApplyAndPersistReturnsStorageFailureWhenSaveFails()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded),
            NextSaveResult =
                SnapshotSaveResult<OrderState>.StorageFailure(loaded.Version, proposed,
                    new PersistenceDiagnostics("db down"))
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.StorageFailure, result.PersistenceCategory);
        Assert.Equal(1, store.SaveCallCount);
    }

    [Fact]
    public async Task ApplyAndPersistDoesNotAutomaticallyRetryOnSaveFailure()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded),
            NextSaveResult =
                SnapshotSaveResult<OrderState>.StorageFailure(loaded.Version, proposed,
                    new PersistenceDiagnostics("db down"))
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);

        _ = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(1, store.SaveCallCount);
    }
}