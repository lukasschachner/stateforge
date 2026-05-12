using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public sealed class DeclaredState
{
    public DeclaredState(string name, string valueExpression, string identityKey, string generatedIdentifier,
        bool isTerminal, Location? sourceLocation = null)
    {
        Name = name;
        ValueExpression = valueExpression;
        IdentityKey = identityKey;
        GeneratedIdentifier = generatedIdentifier;
        IsTerminal = isTerminal;
        SourceLocation = sourceLocation;
    }

    public string Name { get; }
    public string ValueExpression { get; }
    public string IdentityKey { get; }
    public string GeneratedIdentifier { get; }
    public bool IsTerminal { get; set; }
    public List<MetadataEntry> Metadata { get; } = new();
    public Location? SourceLocation { get; }
}