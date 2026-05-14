namespace StateForge.DependencyInjection.Validation;

/// <summary>Controls explicit startup/readiness validation behavior.</summary>
public sealed class StateMachineValidationOptions
{
    public bool IncludeAllRegistrations { get; set; } = true;
    public bool TreatWarningsAsFailures { get; set; }
}
