using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Declarations;

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
    public string? ParentStateKey { get; set; }
    public string? ParentStateExpression { get; set; }
    public Location? ParentLocation { get; set; }
    public string? InitialChildStateKey { get; set; }
    public string? InitialChildExpression { get; set; }
    public Location? InitialChildLocation { get; set; }
    public DeclaredHistoryMode HistoryMode { get; set; }
    public string? HistoryFallbackStateKey { get; set; }
    public string? HistoryFallbackExpression { get; set; }
    public Location? HistoryLocation { get; set; }
    public bool IsParallelComposite { get; set; }
    public List<MetadataEntry> Metadata { get; } = new();
    public Location? SourceLocation { get; }
}