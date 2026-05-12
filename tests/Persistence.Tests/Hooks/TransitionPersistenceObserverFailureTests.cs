using Persistence.Tests.TestSupport;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Persistence.Execution;
using StateMachineLibrary.Persistence.Hooks;
using StateMachineLibrary.Persistence.Snapshots;
using StateMachineLibrary.Persistence.Storage;

namespace Persistence.Tests.Hooks;

public class TransitionPersistenceObserverFailureTests
{
    [Fact]
    public async Task ObserverFailureIsReportedAsObservationFailureCategory()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded)
        };
        var observer = new ThrowingObserver();
        var runtime =
            PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store,
                ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.ObservationFailure, result.PersistenceCategory);
    }

    [Fact]
    public async Task ObserverCancellationIsReportedAsCancelledCategory()
    {
        var definition = PersistenceTestDomain.CreateDefinition();
        var loaded = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));
        var store = new InMemorySnapshotStore
        {
            NextLoadResult = SnapshotLoadResult<OrderState>.Loaded(loaded)
        };
        var observer = new CancellingObserver();
        var runtime =
            PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store,
                ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(new Submit());

        Assert.Equal(TransitionPersistenceCategory.Cancelled, result.PersistenceCategory);
    }

    private sealed class ThrowingObserver : ITransitionPersistenceObserver<OrderState, OrderEvent>
    {
        public ValueTask<ObservationResult> ObserveAsync(TransitionPersistenceContext<OrderState, OrderEvent> context,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("observer failed");
        }
    }

    private sealed class CancellingObserver : ITransitionPersistenceObserver<OrderState, OrderEvent>
    {
        public ValueTask<ObservationResult> ObserveAsync(TransitionPersistenceContext<OrderState, OrderEvent> context,
            CancellationToken cancellationToken)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }
}