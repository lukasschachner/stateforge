using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public enum ReferenceSignatureStatus
{
    Unknown,
    Compatible,
    Incompatible,
    Unresolved
}

public sealed class ConditionReference
{
    public ConditionReference(string memberName, string? displayName, Location? sourceLocation = null)
    {
        MemberName = memberName;
        DisplayName = displayName;
        SourceLocation = sourceLocation;
    }

    public string MemberName { get; }
    public string? DisplayName { get; }
    public ReferenceSignatureStatus SignatureStatus { get; set; }
    public bool UseAsyncBuilder { get; set; }
    public Location? SourceLocation { get; }
}

public enum BehaviorPhase
{
    Exit,
    Transition,
    Entry
}

public sealed class BehaviorReference
{
    public BehaviorReference(string memberName, BehaviorPhase phase, string? displayName,
        Location? sourceLocation = null)
    {
        MemberName = memberName;
        Phase = phase;
        DisplayName = displayName;
        SourceLocation = sourceLocation;
    }

    public string MemberName { get; }
    public BehaviorPhase Phase { get; }
    public string? DisplayName { get; }
    public ReferenceSignatureStatus SignatureStatus { get; set; }
    public bool UseAsyncBuilder { get; set; }
    public Location? SourceLocation { get; }
}

public sealed class MetadataEntry
{
    public MetadataEntry(string key, string valueExpression, Location? sourceLocation = null)
    {
        Key = key;
        ValueExpression = valueExpression;
        SourceLocation = sourceLocation;
    }

    public string Key { get; }
    public string ValueExpression { get; }
    public Location? SourceLocation { get; }
}