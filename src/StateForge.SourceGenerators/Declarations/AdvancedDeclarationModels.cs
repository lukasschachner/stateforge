using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Declarations;

public enum DeclaredHistoryMode
{
    None,
    Shallow,
    Deep,
    Unsupported
}

public sealed class CompositeDeclaration
{
    public CompositeDeclaration(string compositeStateKey, string compositeStateExpression, Location? sourceLocation = null)
    {
        CompositeStateKey = compositeStateKey;
        CompositeStateExpression = compositeStateExpression;
        SourceLocation = sourceLocation;
    }

    public string CompositeStateKey { get; }
    public string CompositeStateExpression { get; }
    public string? InitialChildStateKey { get; set; }
    public string? InitialChildExpression { get; set; }
    public Location? InitialChildLocation { get; set; }
    public DeclaredHistoryMode HistoryMode { get; set; }
    public string? HistoryFallbackStateKey { get; set; }
    public string? HistoryFallbackExpression { get; set; }
    public Location? HistoryLocation { get; set; }
    public Location? SourceLocation { get; }
}

public sealed class ParallelCompositeDeclaration
{
    public ParallelCompositeDeclaration(string compositeStateKey, string compositeStateExpression,
        Location? sourceLocation = null)
    {
        CompositeStateKey = compositeStateKey;
        CompositeStateExpression = compositeStateExpression;
        SourceLocation = sourceLocation;
    }

    public string CompositeStateKey { get; }
    public string CompositeStateExpression { get; }
    public Location? SourceLocation { get; }
}

public sealed class RegionDeclaration
{
    public RegionDeclaration(string ownerCompositeStateKey, string ownerCompositeExpression, string regionName,
        int order, bool isExplicit, Location? sourceLocation = null)
    {
        OwnerCompositeStateKey = ownerCompositeStateKey;
        OwnerCompositeExpression = ownerCompositeExpression;
        RegionName = regionName;
        Order = order;
        IsExplicit = isExplicit;
        SourceLocation = sourceLocation;
    }

    public string OwnerCompositeStateKey { get; }
    public string OwnerCompositeExpression { get; }
    public string RegionName { get; }
    public int Order { get; }
    private readonly List<RegionMembership> _memberships = new();
    private readonly List<Location?> _relatedLocations = new();

    public bool IsExplicit { get; private set; }
    public Location? SourceLocation { get; }
    public Location? InitialLocation { get; set; }
    public string? InitialStateKey { get; set; }
    public string? InitialStateExpression { get; set; }
    public IReadOnlyList<RegionMembership> Memberships => _memberships;
    public IReadOnlyList<Location?> RelatedLocations => _relatedLocations;

    internal void AddMembership(RegionMembership membership)
    {
        _memberships.Add(membership);
    }

    internal void AddRelatedLocation(Location? location)
    {
        _relatedLocations.Add(location);
    }

    internal void MarkExplicit()
    {
        IsExplicit = true;
    }
}

public sealed class RegionMembership
{
    public RegionMembership(string stateKey, string stateExpression, string ownerCompositeStateKey,
        string ownerCompositeExpression, string regionName, bool isInitial, bool isTerminal,
        Location? sourceLocation = null)
    {
        StateKey = stateKey;
        StateExpression = stateExpression;
        OwnerCompositeStateKey = ownerCompositeStateKey;
        OwnerCompositeExpression = ownerCompositeExpression;
        RegionName = regionName;
        IsInitial = isInitial;
        IsTerminal = isTerminal;
        SourceLocation = sourceLocation;
    }

    public string StateKey { get; }
    public string StateExpression { get; }
    public string OwnerCompositeStateKey { get; }
    public string OwnerCompositeExpression { get; }
    public string RegionName { get; }
    public bool IsInitial { get; }
    public bool IsTerminal { get; }
    public Location? SourceLocation { get; }
}
