using StateForge.Persistence.Tests.TestSupport;
using StateForge.Core.Execution;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Hooks;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.Hooks;

public class TransitionPersistenceObserverSuccessTests
{
    [Fact]
    public async Task ObserverReceivesLoadedProposedExpectedVersionAndPersistenceResult()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var committed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v2"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded),
            NextSaveResult = SnapshotSaveResult<OrderState>.Saved(loaded.Version,
                new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Submitted,
                    loaded.Version), committed)
        };
        var observer = new RecordingObserver();

        var runtime =
            PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store,
                ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.Persisted, result.PersistenceCategory);
        Assert.NotNull(observer.LastContext);
        Assert.Equal(OrderState.Draft, observer.LastContext!.LoadedSnapshot!.ActiveState);
        Assert.Equal(OrderState.Submitted, observer.LastContext.ProposedSnapshot!.ActiveState);
        Assert.Equal("v1", observer.LastContext.ExpectedVersion!.Value.ToString());
        Assert.Equal(SnapshotSaveCategory.Saved, observer.LastContext.PersistenceResult!.Category);
    }

    private sealed class RecordingObserver : ITransitionPersistenceObserver<OrderState, OrderEvent>
    {
        public TransitionPersistenceContext<OrderState, OrderEvent>? LastContext { get; private set; }

        public ValueTask<ObservationResult> ObserveAsync(TransitionPersistenceContext<OrderState, OrderEvent> context,
            CancellationToken cancellationToken)
        {
            LastContext = context;
            return ValueTask.FromResult(ObservationResult.Success());
        }
    }
}