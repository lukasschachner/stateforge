using Persistence.Tests.TestSupport;
using StateMachineLibrary.Persistence.Execution;
using StateMachineLibrary.Persistence.Snapshots;
using StateMachineLibrary.Persistence.Storage;

namespace Persistence.Tests.Storage;

public class ApplicationProvidedStoreIntegrationTests
{
    [Fact]
    public async Task ApplicationStoreSupportsReloadAndPersistFlow()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var store = new ApplicationOwnedSampleStore();
        store.Seed(new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1")));

        var load = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);
        Assert.Equal(SnapshotLoadCategory.Loaded, load.Category);

        var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
        var outcome = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.Persisted, outcome.PersistenceCategory);
        Assert.Equal(OrderState.Submitted, outcome.CommittedSnapshot!.ActiveState);
    }
}