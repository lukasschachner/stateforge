using StateForge.Core.Diagnostics;

namespace StateForge.Core.Validation;

/// <summary>Result of machine definition validation.</summary>
public sealed class ValidationResult
{
    public ValidationResult(IEnumerable<ValidationFinding> findings,
        IEnumerable<TransitionConflictDiagnostic>? conflictDiagnostics = null)
    {
        Findings = findings.ToArray();
        Errors = Findings.Where(f => f.Severity == ValidationSeverity.Error).ToArray();
        Warnings = Findings.Where(f => f.Severity == ValidationSeverity.Warning).ToArray();
        ConflictDiagnostics = (conflictDiagnostics ?? []).ToArray();
    }

    public static ValidationResult Valid { get; } = new([]);

    public IReadOnlyList<ValidationFinding> Findings { get; }
    public IReadOnlyList<ValidationFinding> Errors { get; }
    public IReadOnlyList<ValidationFinding> Warnings { get; }
    public IReadOnlyList<TransitionConflictDiagnostic> ConflictDiagnostics { get; }
    public bool IsValid => Errors.Count == 0;
}