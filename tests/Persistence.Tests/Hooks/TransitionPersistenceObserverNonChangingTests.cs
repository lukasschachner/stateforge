using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Persistence;
using StateMachineLibrary.Persistence.Execution;
using StateMachineLibrary.Persistence.Hooks;
using StateMachineLibrary.Persistence.Snapshots;
using StateMachineLibrary.Persistence.Storage;

namespace Persistence.Tests.Hooks;

public class TransitionPersistenceObserverNonChangingTests
{
    [Fact]
    public async Task ObserverIsCalledForDeniedOutcome()
    {
        var definition = StateMachineDefinition<int, int>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, "d1");
            builder.State(1).On(1).When(_ => false, "deny").GoTo(2);
            builder.State(2);
        });

        var store = new IntStore(new StateSnapshot<int>("i1", "d1", 1, PersistenceVersion.From("v1")));
        var observer = new RecordingIntObserver();
        var runtime =
            PersistentStateMachineRuntime<int, int>.Create(definition, "i1", store, ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(1);

        Assert.Equal(TransitionPersistenceCategory.ObservedOnly, result.PersistenceCategory);
        Assert.Equal(TransitionOutcomeCategory.Denied, observer.LastContext!.TransitionOutcome!.Category);
    }

    [Fact]
    public async Task ObserverIsCalledForNotPermittedOutcome()
    {
        var definition = StateMachineDefinition<int, int>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, "d1");
            builder.State(1).On(1).GoTo(2);
            builder.State(2);
        });

        var store = new IntStore(new StateSnapshot<int>("i1", "d1", 1, PersistenceVersion.From("v1")));
        var observer = new RecordingIntObserver();
        var runtime =
            PersistentStateMachineRuntime<int, int>.Create(definition, "i1", store, ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(999);

        Assert.Equal(TransitionPersistenceCategory.ObservedOnly, result.PersistenceCategory);
        Assert.Equal(TransitionOutcomeCategory.NotPermitted, observer.LastContext!.TransitionOutcome!.Category);
    }

    [Fact]
    public async Task ObserverIsCalledForCancelledOutcome()
    {
        var cts = new CancellationTokenSource();
        var definition = StateMachineDefinition<int, int>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, "d1");
            builder.State(1).On(1).Execute(_ => cts.Cancel()).GoTo(2);
            builder.State(2);
        });

        var store = new IntStore(new StateSnapshot<int>("i1", "d1", 1, PersistenceVersion.From("v1")));
        var observer = new RecordingIntObserver();
        var runtime =
            PersistentStateMachineRuntime<int, int>.Create(definition, "i1", store, ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(1, cts.Token);

        Assert.Equal(TransitionPersistenceCategory.ObservedOnly, result.PersistenceCategory);
        Assert.Equal(TransitionOutcomeCategory.Cancelled, observer.LastContext!.TransitionOutcome!.Category);
    }

    [Fact]
    public async Task ObserverIsCalledForBehaviorFailureOutcome()
    {
        var definition = StateMachineDefinition<int, int>.Create(builder =>
        {
            builder.WithMetadata(PersistenceMetadataKeys.DefinitionId, "d1");
            builder.State(1).On(1).Execute(_ => throw new InvalidOperationException("boom")).GoTo(2);
            builder.State(2);
        });

        var store = new IntStore(new StateSnapshot<int>("i1", "d1", 1, PersistenceVersion.From("v1")));
        var observer = new RecordingIntObserver();
        var runtime =
            PersistentStateMachineRuntime<int, int>.Create(definition, "i1", store, ConcurrencyMode.Fast, observer);

        var result = await runtime.ApplyAndPersistAsync(1);

        Assert.Equal(TransitionPersistenceCategory.ObservedOnly, result.PersistenceCategory);
        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, observer.LastContext!.TransitionOutcome!.Category);
    }

    private sealed class IntStore(StateSnapshot<int> snapshot) : IStateSnapshotStore<int>
    {
        public ValueTask<SnapshotLoadResult<int>> LoadAsync(string instanceId, string definitionId,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(SnapshotLoadResult<int>.Loaded(snapshot));
        }

        public ValueTask<SnapshotSaveResult<int>> SaveAsync(PersistenceVersion expectedVersion,
            StateSnapshot<int> proposedSnapshot, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(SnapshotSaveResult<int>.Saved(expectedVersion, proposedSnapshot,
                proposedSnapshot));
        }
    }

    private sealed class RecordingIntObserver : ITransitionPersistenceObserver<int, int>
    {
        public TransitionPersistenceContext<int, int>? LastContext { get; private set; }

        public ValueTask<ObservationResult> ObserveAsync(TransitionPersistenceContext<int, int> context,
            CancellationToken cancellationToken)
        {
            LastContext = context;
            return ValueTask.FromResult(ObservationResult.Success());
        }
    }
}