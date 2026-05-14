using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.Storage;

/// <summary>Categories produced by snapshot load operations.</summary>
public enum SnapshotLoadCategory
{
    Loaded,
    MissingSnapshot,
    InvalidSnapshot,
    StorageFailure,
    Cancelled
}

/// <summary>Result of loading a snapshot from application-owned storage.</summary>
public sealed class SnapshotLoadResult<TState>
{
    private SnapshotLoadResult(SnapshotLoadCategory category, StateSnapshot<TState>? snapshot,
        PersistenceDiagnostics diagnostics)
    {
        Category = category;
        Snapshot = snapshot;
        Diagnostics = diagnostics;
    }

    public SnapshotLoadCategory Category { get; }
    public StateSnapshot<TState>? Snapshot { get; }
    public PersistenceDiagnostics Diagnostics { get; }

    public static SnapshotLoadResult<TState> Loaded(StateSnapshot<TState> snapshot)
    {
        return new SnapshotLoadResult<TState>(SnapshotLoadCategory.Loaded,
            snapshot ?? throw new ArgumentNullException(nameof(snapshot)),
            PersistenceDiagnostics.None);
    }

    public static SnapshotLoadResult<TState> MissingSnapshot(PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotLoadResult<TState>(SnapshotLoadCategory.MissingSnapshot, null,
            diagnostics ?? new PersistenceDiagnostics("No stored snapshot exists."));
    }

    public static SnapshotLoadResult<TState> InvalidSnapshot(PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotLoadResult<TState>(SnapshotLoadCategory.InvalidSnapshot, null,
            diagnostics ?? new PersistenceDiagnostics("Stored snapshot is invalid."));
    }

    public static SnapshotLoadResult<TState> StorageFailure(PersistenceDiagnostics diagnostics)
    {
        return new SnapshotLoadResult<TState>(SnapshotLoadCategory.StorageFailure, null,
            diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)));
    }

    public static SnapshotLoadResult<TState> Cancelled(PersistenceDiagnostics? diagnostics = null)
    {
        return new SnapshotLoadResult<TState>(SnapshotLoadCategory.Cancelled, null,
            diagnostics ?? new PersistenceDiagnostics("Snapshot load was cancelled."));
    }
}