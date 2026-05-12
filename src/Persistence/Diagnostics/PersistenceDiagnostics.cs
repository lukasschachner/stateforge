using StateMachineLibrary.Persistence.Snapshots;

namespace StateMachineLibrary.Persistence.Diagnostics;

/// <summary>Diagnostics payload for persistence load/save/reload/apply outcomes.</summary>
public sealed class PersistenceDiagnostics
{
    public PersistenceDiagnostics(
        string summary,
        string? code = null,
        Exception? exception = null,
        IReadOnlyList<SnapshotValidationIssue>? validationIssues = null,
        string? affectedElement = null)
    {
        Summary = summary;
        Code = code;
        Exception = exception;
        ValidationIssues = validationIssues ?? Array.Empty<SnapshotValidationIssue>();
        AffectedElement = affectedElement;
    }

    public static PersistenceDiagnostics None { get; } = new("Persistence operation completed.");

    public string Summary { get; }
    public string? Code { get; }
    public Exception? Exception { get; }
    public IReadOnlyList<SnapshotValidationIssue> ValidationIssues { get; }
    public string? AffectedElement { get; }
}