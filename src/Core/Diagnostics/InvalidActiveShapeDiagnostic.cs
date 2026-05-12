namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Structured details for a planned active-state shape that would be invalid if committed.</summary>
public sealed class InvalidActiveShapeDiagnostic
{
    public InvalidActiveShapeDiagnostic(
        object? compositeState = null,
        string? regionId = null,
        string? regionName = null,
        string expectedShape = "Valid active-state shape",
        IEnumerable<object?>? actualActiveStates = null,
        IEnumerable<string>? missingRegions = null,
        IEnumerable<string>? duplicateRegions = null)
    {
        CompositeState = compositeState;
        RegionId = regionId;
        RegionName = regionName;
        ExpectedShape = expectedShape;
        ActualActiveStates = (actualActiveStates ?? []).ToArray();
        MissingRegions = (missingRegions ?? []).ToArray();
        DuplicateRegions = (duplicateRegions ?? []).ToArray();
    }

    public object? CompositeState { get; }
    public string? RegionId { get; }
    public string? RegionName { get; }
    public string ExpectedShape { get; }
    public IReadOnlyList<object?> ActualActiveStates { get; }
    public IReadOnlyList<string> MissingRegions { get; }
    public IReadOnlyList<string> DuplicateRegions { get; }
}
