using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Persistence.Diagnostics;
using StateMachineLibrary.Persistence.Snapshots;

namespace StateMachineLibrary.Persistence.Execution;

/// <summary>Combined transition and persistence outcome categories.</summary>
public enum TransitionPersistenceCategory
{
    NotAttempted,
    ObservedOnly,
    Persisted,
    MissingSnapshot,
    InvalidSnapshot,
    ConcurrentStateChange,
    StorageFailure,
    ObservationFailure,
    Cancelled
}

/// <summary>Combined transition execution and persistence result.</summary>
public sealed class TransitionPersistenceOutcome<TState, TEvent>
{
    private TransitionPersistenceOutcome(
        TransitionPersistenceCategory persistenceCategory,
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        StateSnapshot<TState>? loadedSnapshot,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot,
        CommittedSnapshot<TState>? committedSnapshot,
        PersistenceVersion? expectedVersion,
        PersistenceDiagnostics diagnostics)
    {
        PersistenceCategory = persistenceCategory;
        TransitionOutcome = transitionOutcome;
        LoadedSnapshot = loadedSnapshot;
        ProposedSnapshot = proposedSnapshot;
        CommittedSnapshot = committedSnapshot;
        ExpectedVersion = expectedVersion;
        Diagnostics = diagnostics;
    }

    public TransitionPersistenceCategory PersistenceCategory { get; }
    public TransitionOutcome<TState, TEvent>? TransitionOutcome { get; }
    public StateSnapshot<TState>? LoadedSnapshot { get; }
    public ProposedSnapshot<TState, TEvent>? ProposedSnapshot { get; }
    public CommittedSnapshot<TState>? CommittedSnapshot { get; }
    public PersistenceVersion? ExpectedVersion { get; }
    public PersistenceDiagnostics Diagnostics { get; }

    public static TransitionPersistenceOutcome<TState, TEvent> Persisted(
        TransitionOutcome<TState, TEvent> transitionOutcome,
        CommittedSnapshot<TState> committedSnapshot,
        PersistenceVersion expectedVersion,
        StateSnapshot<TState>? loadedSnapshot = null,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.Persisted,
            transitionOutcome, loadedSnapshot, proposedSnapshot,
            committedSnapshot, expectedVersion, PersistenceDiagnostics.None);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> ObservedOnly(
        TransitionOutcome<TState, TEvent> transitionOutcome,
        StateSnapshot<TState> loadedSnapshot,
        PersistenceDiagnostics? diagnostics = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.ObservedOnly,
            transitionOutcome, loadedSnapshot, null, null,
            loadedSnapshot.Version, diagnostics ?? PersistenceDiagnostics.None);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> MissingSnapshot(PersistenceDiagnostics diagnostics)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.MissingSnapshot, null,
            null, null, null, null, diagnostics);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> InvalidSnapshot(PersistenceDiagnostics diagnostics,
        StateSnapshot<TState>? loadedSnapshot = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.InvalidSnapshot, null,
            loadedSnapshot, null, null,
            loadedSnapshot?.Version, diagnostics);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> ConcurrentStateChange(
        TransitionOutcome<TState, TEvent> transitionOutcome,
        ProposedSnapshot<TState, TEvent> proposedSnapshot,
        PersistenceVersion expectedVersion,
        PersistenceDiagnostics? diagnostics = null,
        StateSnapshot<TState>? loadedSnapshot = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.ConcurrentStateChange,
            transitionOutcome, loadedSnapshot,
            proposedSnapshot, null, expectedVersion,
            diagnostics ?? new PersistenceDiagnostics("Concurrent state change detected."));
    }

    public static TransitionPersistenceOutcome<TState, TEvent> StorageFailure(
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        PersistenceDiagnostics diagnostics,
        StateSnapshot<TState>? loadedSnapshot = null,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot = null,
        PersistenceVersion? expectedVersion = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.StorageFailure,
            transitionOutcome, loadedSnapshot, proposedSnapshot,
            null, expectedVersion, diagnostics);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> ObservationFailure(
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        PersistenceDiagnostics diagnostics,
        StateSnapshot<TState>? loadedSnapshot = null,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot = null,
        PersistenceVersion? expectedVersion = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.ObservationFailure,
            transitionOutcome, loadedSnapshot,
            proposedSnapshot, null, expectedVersion, diagnostics);
    }

    public static TransitionPersistenceOutcome<TState, TEvent> Cancelled(
        TransitionOutcome<TState, TEvent>? transitionOutcome,
        PersistenceDiagnostics diagnostics,
        StateSnapshot<TState>? loadedSnapshot = null,
        ProposedSnapshot<TState, TEvent>? proposedSnapshot = null,
        PersistenceVersion? expectedVersion = null)
    {
        return new TransitionPersistenceOutcome<TState, TEvent>(TransitionPersistenceCategory.Cancelled,
            transitionOutcome, loadedSnapshot, proposedSnapshot, null,
            expectedVersion, diagnostics);
    }
}