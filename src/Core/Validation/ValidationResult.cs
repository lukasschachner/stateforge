namespace StateMachineLibrary.Core.Validation;

/// <summary>Result of machine definition validation.</summary>
public sealed class ValidationResult
{
    public ValidationResult(IEnumerable<ValidationFinding> findings)
    {
        Findings = findings.ToArray();
        Errors = Findings.Where(f => f.Severity == ValidationSeverity.Error).ToArray();
        Warnings = Findings.Where(f => f.Severity == ValidationSeverity.Warning).ToArray();
    }

    public static ValidationResult Valid { get; } = new([]);

    public IReadOnlyList<ValidationFinding> Findings { get; }
    public IReadOnlyList<ValidationFinding> Errors { get; }
    public IReadOnlyList<ValidationFinding> Warnings { get; }
    public bool IsValid => Errors.Count == 0;
}