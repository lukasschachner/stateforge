using StateForge.Core.Validation;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Validation;

public sealed record StateMachineRegistrationValidationEntry(
    StateMachineIdentity Identity,
    ValidationResult ValidationResult);

public sealed class StateMachineRegistrationValidationResult
{
    public StateMachineRegistrationValidationResult(IEnumerable<StateMachineRegistrationValidationEntry> entries, bool treatWarningsAsFailures = false)
    {
        Entries = entries.ToArray();
        TreatWarningsAsFailures = treatWarningsAsFailures;
    }

    public IReadOnlyList<StateMachineRegistrationValidationEntry> Entries { get; }
    public bool TreatWarningsAsFailures { get; }
    public bool Succeeded => Entries.All(e => e.ValidationResult.IsValid && (!TreatWarningsAsFailures || e.ValidationResult.Findings.All(f => f.Severity != ValidationSeverity.Warning)));

    public string ToDisplayString() => string.Join(Environment.NewLine,
        Entries.SelectMany(e => e.ValidationResult.Findings.Select(f => $"{e.Identity.DisplayName}: {f.Code} {f.Message}")));
}
