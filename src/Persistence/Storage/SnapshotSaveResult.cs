using StateMachineLibrary.Persistence.Diagnostics;
using StateMachineLibrary.Persistence.Snapshots;

namespace StateMachineLibrary.Persistence.Storage;

/// <summary>Categories produced by snapshot save operations.</summary>
public enum SnapshotSaveCategory
{
    Saved,
    MissingSnapshot,
    ConcurrentStateChange,
    InvalidSnapshot,
    StorageFailure,
    Cancelled
}

/// <summary>Result of attempting to save a proposed snapshot using expected-version concurrency.</summary>
public sealed class SnapshotSaveResult<TState>
{
    private SnapshotSaveResult(
        SnapshotSaveCategory category,
        PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot,
        StateSnapshot<TState>? committedSnapshot,
        PersistenceVersion? currentVersionHint,
        PersistenceDiagnostics diagnostics)
    {
        Category = category;
        ExpectedVersion = expectedVersion;
        ProposedSnapshot = proposedSnapshot;
        CommittedSnapshot = committedSnapshot;
        CurrentVersionHint = currentVersionHint;
        Diagnostics = diagnostics;
    }

    public SnapshotSaveCategory Category { get; }
    public PersistenceVersion ExpectedVersion { get; }
    public StateSnapshot<TState> ProposedSnapshot { get; }
    public StateSnapshot<TState>? CommittedSnapshot { get; }
    public PersistenceVersion? CurrentVersionHint { get; }
    public PersistenceDiagnostics Diagnostics { get; }

    public static SnapshotSaveResult<TState> Saved(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, StateSnapshot<TState> committedSnapshot)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.Saved, expectedVersion, proposedSnapshot,
            committedSnapshot, null,
            PersistenceDiagnostics.None);
    }

    public static SnapshotSaveResult<TState> MissingSnapshot(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.MissingSnapshot, expectedVersion, proposedSnapshot,
            null, null,
            diagnostics ?? new PersistenceDiagnostics("Stored snapshot is missing."));
    }

    public static SnapshotSaveResult<TState> ConcurrentStateChange(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, PersistenceVersion? currentVersionHint = null,
        PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.ConcurrentStateChange, expectedVersion,
            proposedSnapshot, null,
            currentVersionHint,
            diagnostics ?? new PersistenceDiagnostics("Expected version does not match current stored version."));
    }

    public static SnapshotSaveResult<TState> InvalidSnapshot(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.InvalidSnapshot, expectedVersion, proposedSnapshot,
            null, null,
            diagnostics ?? new PersistenceDiagnostics("Proposed snapshot is invalid."));
    }

    public static SnapshotSaveResult<TState> StorageFailure(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, PersistenceDiagnostics diagnostics)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.StorageFailure, expectedVersion, proposedSnapshot,
            null, null,
            diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)));
    }

    public static SnapshotSaveResult<TState> Cancelled(PersistenceVersion expectedVersion,
        StateSnapshot<TState> proposedSnapshot, PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotSaveResult<TState>(SnapshotSaveCategory.Cancelled, expectedVersion, proposedSnapshot, null,
            null,
            diagnostics ?? new PersistenceDiagnostics("Snapshot save was cancelled."));
    }
}