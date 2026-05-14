namespace StateForge.Core.Validation;

/// <summary>Validation codes for completion-transition declarations.</summary>
public static class CompletionTransitionValidationCodes
{
    public const string InvalidSource = "COMPLETION001";
    public const string InvalidTarget = "COMPLETION002";
    public const string AmbiguousUnguarded = "COMPLETION003";
    public const string InvalidRegionBoundary = "COMPLETION004";
    public const string InvalidParallelScope = "COMPLETION005";
}
