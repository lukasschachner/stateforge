namespace StateForge.Core.Validation;

/// <summary>An actionable issue found in a machine definition.</summary>
public sealed record ValidationFinding(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? AffectedElement = null,
    string? SuggestedResolution = null,
    object? CompositeState = null,
    string? RegionId = null,
    string? RegionName = null,
    object? SourceState = null,
    object? TargetState = null,
    object? Event = null,
    string? TransitionId = null);