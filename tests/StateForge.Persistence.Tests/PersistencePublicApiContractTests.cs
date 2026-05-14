using StateForge.Persistence.Tests.TestSupport;
using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Persistence;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests;

public class PersistencePublicApiContractTests
{
    [Fact]
    public void DefinitionIdUsesWellKnownMetadataKey()
    {
        var definition = StateMachineDefinition<int, int>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, "machine-v1");
            builder.State(1).On(1).GoTo(2);
            builder.State(2);
        });

        Assert.Equal("machine-v1", definition.GetPersistenceDefinitionId());
    }

    [Fact]
    public void ProposedAndCommittedSnapshotsUseComposition()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var transition = TransitionOutcome<OrderState, OrderEvent>.Success(
            OrderState.Draft,
            OrderState.Submitted,
            new Submit(),
            definition.Transitions[0]);
        var snapshot = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v2"));

        var proposed =
            new ProposedSnapshot<OrderState, OrderEvent>(snapshot, PersistenceVersion.From("v1"), transition);
        var committed = new CommittedSnapshot<OrderState>(snapshot);

        Assert.Same(snapshot, proposed.Snapshot);
        Assert.Same(snapshot, committed.Snapshot);
    }

    [Fact]
    public async Task MissingSnapshotIsExplicit()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore();

        var load = await PersistentStateLoader.ReloadAsync(definition, "missing", store);

        Assert.Equal(SnapshotLoadCategory.MissingSnapshot, load.Category);
    }

    [Fact]
    public async Task InvalidSnapshotIsExplicit()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(
                new StateSnapshot<OrderState>("order-1", "wrong", OrderState.Draft, PersistenceVersion.From("v1")))
        };

        var load = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);

        Assert.Equal(SnapshotLoadCategory.InvalidSnapshot, load.Category);
    }

    [Fact]
    public async Task SaveConcurrencyConflictIsExplicit()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded),
            NextSaveResult = SnapshotSaveResult<OrderState>.ConcurrentStateChange(
                loaded.Version,
                new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Submitted,
                    loaded.Version),
                PersistenceVersion.From("v2"))
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.ConcurrentStateChange, result.PersistenceCategory);
        Assert.Null(result.CommittedSnapshot);
    }

    [Fact]
    public async Task CancellationFlowsToExplicitCategory()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Cancelled()
        };

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.Cancelled, result.PersistenceCategory);
    }
}