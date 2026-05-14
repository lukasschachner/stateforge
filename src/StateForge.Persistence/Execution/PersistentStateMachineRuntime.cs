using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.Hooks;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Execution;

/// <summary>
///     Applies events against loaded snapshots and persists successful state changes with expected-version checks.
/// </summary>
public sealed class PersistentStateMachineRuntime<TState, TEvent> : IAsyncDisposable
{
    private readonly SemaphoreSlim? _gate;
    private readonly ITransitionPersistenceObserver<TState, TEvent>? _observer;

    private PersistentStateMachineRuntime(
        StateMachineDefinition<TState, TEvent> definition,
        string instanceId,
        IStateSnapshotStore<TState> store,
        ConcurrencyMode concurrencyMode,
        ITransitionPersistenceObserver<TState, TEvent>? observer)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance id is required.", nameof(instanceId));

        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        InstanceId = instanceId;
        Store = store ?? throw new ArgumentNullException(nameof(store));
        ConcurrencyMode = concurrencyMode;
        _observer = observer;
        _gate = concurrencyMode == ConcurrencyMode.Serialized ? new SemaphoreSlim(1, 1) : null;
    }

    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public string InstanceId { get; }
    public IStateSnapshotStore<TState> Store { get; }
    public ConcurrencyMode ConcurrencyMode { get; }

    public ValueTask DisposeAsync()
    {
        _gate?.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Creates a persistence runtime. Serialized mode coordinates local in-process calls;
    ///     cross-process correctness still requires expected-version storage checks.
    /// </summary>
    public static PersistentStateMachineRuntime<TState, TEvent> Create(
        StateMachineDefinition<TState, TEvent> definition,
        string instanceId,
        IStateSnapshotStore<TState> store,
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Fast,
        ITransitionPersistenceObserver<TState, TEvent>? observer = null)
    {
        return new PersistentStateMachineRuntime<TState, TEvent>(definition, instanceId, store, concurrencyMode,
            observer);
    }

    /// <summary>
    ///     Loads, applies, and attempts to persist a transition in one coordinated operation.
    /// </summary>
    public async ValueTask<TransitionPersistenceOutcome<TState, TEvent>> ApplyAndPersistAsync(
        TEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (_gate is null) return await ApplyAndPersistCoreAsync(@event, cancellationToken).ConfigureAwait(false);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ApplyAndPersistCoreAsync(@event, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async ValueTask<TransitionPersistenceOutcome<TState, TEvent>> ApplyAndPersistCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        var load = await PersistentStateLoader.ReloadAsync(Definition, InstanceId, Store, cancellationToken)
            .ConfigureAwait(false);
        switch (load.Category)
        {
            case SnapshotLoadCategory.MissingSnapshot:
                return TransitionPersistenceOutcome<TState, TEvent>.MissingSnapshot(load.Diagnostics);
            case SnapshotLoadCategory.InvalidSnapshot:
                return TransitionPersistenceOutcome<TState, TEvent>.InvalidSnapshot(load.Diagnostics, load.Snapshot);
            case SnapshotLoadCategory.StorageFailure:
                return TransitionPersistenceOutcome<TState, TEvent>.StorageFailure(null, load.Diagnostics);
            case SnapshotLoadCategory.Cancelled:
                return TransitionPersistenceOutcome<TState, TEvent>.Cancelled(null, load.Diagnostics);
            case SnapshotLoadCategory.Loaded:
            default:
                break;
        }

        var loaded = load.Snapshot!;
        var transitionOutcome =
            await Definition.ApplyAsync(loaded.ActiveState, @event, cancellationToken).ConfigureAwait(false);
        if (!transitionOutcome.IsSuccess)
        {
            var observedOnly = TransitionPersistenceOutcome<TState, TEvent>.ObservedOnly(transitionOutcome, loaded);
            return await ObserveAsync(observedOnly, loaded, null, transitionOutcome, loaded.Version, null,
                cancellationToken).ConfigureAwait(false);
        }

        var proposed = new ProposedSnapshot<TState, TEvent>(
            loaded.InstanceId,
            loaded.DefinitionId,
            transitionOutcome.ResultingState,
            loaded.Version,
            loaded.Version,
            transitionOutcome,
            loaded.Properties);

        var save = await Store.SaveAsync(loaded.Version, proposed.Snapshot, cancellationToken).ConfigureAwait(false);
        var outcome = save.Category switch
        {
            SnapshotSaveCategory.Saved => TransitionPersistenceOutcome<TState, TEvent>.Persisted(
                transitionOutcome,
                ToCommittedSnapshot(save.CommittedSnapshot!),
                loaded.Version,
                loaded,
                proposed),
            SnapshotSaveCategory.ConcurrentStateChange => TransitionPersistenceOutcome<TState, TEvent>
                .ConcurrentStateChange(
                    transitionOutcome,
                    proposed,
                    loaded.Version,
                    save.Diagnostics,
                    loaded),
            SnapshotSaveCategory.MissingSnapshot => TransitionPersistenceOutcome<TState, TEvent>.MissingSnapshot(
                save.Diagnostics),
            SnapshotSaveCategory.InvalidSnapshot => TransitionPersistenceOutcome<TState, TEvent>.InvalidSnapshot(
                save.Diagnostics, loaded),
            SnapshotSaveCategory.Cancelled => TransitionPersistenceOutcome<TState, TEvent>.Cancelled(transitionOutcome,
                save.Diagnostics, loaded, proposed, loaded.Version),
            _ => TransitionPersistenceOutcome<TState, TEvent>.StorageFailure(transitionOutcome, save.Diagnostics,
                loaded, proposed, loaded.Version)
        };

        return await ObserveAsync(outcome, loaded, proposed, transitionOutcome, loaded.Version, save, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<TransitionPersistenceOutcome<TState, TEvent>> ObserveAsync(
        TransitionPersistenceOutcome<TState, TEvent> outcome,
        StateSnapshot<TState>? loadedSnapshot,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot,
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        PersistenceVersion? expectedVersion,
        SnapshotSaveResult<TState>? persistenceResult,
        CancellationToken cancellationToken)
    {
        if (_observer is null) return outcome;

        var context = new TransitionPersistenceContext<TState, TEvent>(
            loadedSnapshot,
            proposedSnapshot,
            transitionOutcome,
            expectedVersion,
            persistenceResult);

        try
        {
            var observation = await _observer.ObserveAsync(context, cancellationToken).ConfigureAwait(false);
            return observation.Category switch
            {
                ObservationCategory.Success => outcome,
                ObservationCategory.Cancelled => TransitionPersistenceOutcome<TState, TEvent>.Cancelled(
                    transitionOutcome,
                    observation.Diagnostics,
                    loadedSnapshot,
                    proposedSnapshot,
                    expectedVersion),
                _ => TransitionPersistenceOutcome<TState, TEvent>.ObservationFailure(
                    transitionOutcome,
                    observation.Diagnostics,
                    loadedSnapshot,
                    proposedSnapshot,
                    expectedVersion)
            };
        }
        catch (OperationCanceledException)
        {
            return TransitionPersistenceOutcome<TState, TEvent>.Cancelled(
                transitionOutcome,
                new PersistenceDiagnostics("Observer was cancelled."),
                loadedSnapshot,
                proposedSnapshot,
                expectedVersion);
        }
        catch (Exception ex)
        {
            return TransitionPersistenceOutcome<TState, TEvent>.ObservationFailure(
                transitionOutcome,
                new PersistenceDiagnostics("Observer failed.", "observer.failure", ex),
                loadedSnapshot,
                proposedSnapshot,
                expectedVersion);
        }
    }

    private static CommittedSnapshot<TState> ToCommittedSnapshot(StateSnapshot<TState> snapshot)
    {
        return new CommittedSnapshot<TState>(snapshot);
    }
}