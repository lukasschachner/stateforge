namespace StateForge.Core.Validation;

/// <summary>Stable validation codes for hierarchy-specific definition findings.</summary>
public static class HierarchyValidationCodes
{
    public const string ParentMissing = "HIERARCHY001";
    public const string SelfParent = "HIERARCHY002";
    public const string Cycle = "HIERARCHY003";
    public const string MissingInitialChild = "HIERARCHY004";
    public const string InvalidInitialChild = "HIERARCHY005";
    public const string InitialChildMissing = "HIERARCHY006";
    public const string UnreachableNestedState = "HIERARCHY007";
    public const string AmbiguousTransition = "HIERARCHY008";
    public const string AmbiguousCompletion = "HIERARCHY009";
    public const string HistoryOnNonComposite = "HIERARCHY010";
    public const string InvalidHistoryFallback = "HIERARCHY011";
    public const string MissingHistoryFallback = "HIERARCHY012";
    public const string AmbiguousDeepHistory = "HIERARCHY013";
}