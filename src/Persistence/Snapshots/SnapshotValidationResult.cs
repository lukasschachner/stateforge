namespace StateMachineLibrary.Persistence.Snapshots;

/// <summary>Single issue found while validating a loaded or proposed state snapshot.</summary>
public sealed record SnapshotValidationIssue(string Code, string Message, string? AffectedElement = null);

/// <summary>Validation result for persistence snapshots.</summary>
public sealed class SnapshotValidationResult
{
    private SnapshotValidationResult(IReadOnlyList<SnapshotValidationIssue> issues)
    {
        Issues = issues;
    }

    public bool IsValid => Issues.Count == 0;
    public IReadOnlyList<SnapshotValidationIssue> Issues { get; }

    public static SnapshotValidationResult Valid()
    {
        return new SnapshotValidationResult(Array.Empty<SnapshotValidationIssue>());
    }

    public static SnapshotValidationResult Invalid(params SnapshotValidationIssue[] issues)
    {
        return new SnapshotValidationResult(issues?.Length > 0
            ? issues
            : [new SnapshotValidationIssue("snapshot.invalid", "Snapshot is invalid.")]);
    }

    public static SnapshotValidationResult FromIssues(IEnumerable<SnapshotValidationIssue> issues)
    {
        return new SnapshotValidationResult(issues?.Where(static issue => issue is not null).ToArray() ??
                                            Array.Empty<SnapshotValidationIssue>());
    }
}