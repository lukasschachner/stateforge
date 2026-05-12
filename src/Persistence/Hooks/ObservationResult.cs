using StateMachineLibrary.Persistence.Diagnostics;

namespace StateMachineLibrary.Persistence.Hooks;

/// <summary>Observer completion category.</summary>
public enum ObservationCategory
{
    Success,
    Failure,
    Cancelled
}

/// <summary>Result returned by transition persistence observers.</summary>
public sealed class ObservationResult
{
    private ObservationResult(ObservationCategory category, PersistenceDiagnostics diagnostics)
    {
        Category = category;
        Diagnostics = diagnostics;
    }

    public ObservationCategory Category { get; }
    public PersistenceDiagnostics Diagnostics { get; }

    public static ObservationResult Success(PersistenceDiagnostics? diagnostics = null)
    {
        return new ObservationResult(ObservationCategory.Success, diagnostics ?? PersistenceDiagnostics.None);
    }

    public static ObservationResult Failure(PersistenceDiagnostics diagnostics)
    {
        return new ObservationResult(ObservationCategory.Failure,
            diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)));
    }

    public static ObservationResult Cancelled(PersistenceDiagnostics? diagnostics = null)
    {
        return new ObservationResult(ObservationCategory.Cancelled,
            diagnostics ?? new PersistenceDiagnostics("Observation was cancelled."));
    }
}