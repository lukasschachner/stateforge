using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public enum DeclaredEventKind
{
    Value,
    PayloadType
}

public sealed class DeclaredEvent
{
    public DeclaredEvent(string name, DeclaredEventKind eventKind, string identityKey, string generatedIdentifier,
        string? valueExpression, string? payloadType, Location? sourceLocation = null)
    {
        Name = name;
        EventKind = eventKind;
        IdentityKey = identityKey;
        GeneratedIdentifier = generatedIdentifier;
        ValueExpression = valueExpression;
        PayloadType = payloadType;
        SourceLocation = sourceLocation;
    }

    public string Name { get; }
    public DeclaredEventKind EventKind { get; }
    public string IdentityKey { get; }
    public string GeneratedIdentifier { get; }
    public string? ValueExpression { get; }
    public string? PayloadType { get; }
    public List<MetadataEntry> Metadata { get; } = new();
    public Location? SourceLocation { get; }
}