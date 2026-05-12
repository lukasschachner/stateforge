using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Persistence.Snapshots;
using StateMachineLibrary.Persistence.Storage;

namespace StateMachineLibrary.Persistence.Hooks;

/// <summary>Observer context for a transition persistence attempt.</summary>
public sealed class TransitionPersistenceContext<TState, TEvent>
{
    public TransitionPersistenceContext(
        StateSnapshot<TState>? loadedSnapshot,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot,
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        PersistenceVersion? expectedVersion,
        SnapshotSaveResult<TState>? persistenceResult)
    {
        LoadedSnapshot = loadedSnapshot;
        ProposedSnapshot = proposedSnapshot;
        TransitionOutcome = transitionOutcome;
        ExpectedVersion = expectedVersion;
        PersistenceResult = persistenceResult;
    }

    public StateSnapshot<TState>? LoadedSnapshot { get; }
    public ProposedSnapshot<TState, TEvent>? ProposedSnapshot { get; }
    public TransitionOutcome<TState, TEvent>? TransitionOutcome { get; }
    public PersistenceVersion? ExpectedVersion { get; }
    public SnapshotSaveResult<TState>? PersistenceResult { get; }
}