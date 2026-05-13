using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public sealed class StaticDeclarationGraph
{
    public StaticDeclarationGraph(IReadOnlyList<StaticDeclarationGraphNode> nodes,
        IReadOnlyList<StaticDeclarationGraphEdge> edges, IReadOnlyList<string> initialRoots)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Edges = edges ?? throw new ArgumentNullException(nameof(edges));
        InitialRoots = initialRoots ?? throw new ArgumentNullException(nameof(initialRoots));
    }

    public IReadOnlyList<StaticDeclarationGraphNode> Nodes { get; }
    public IReadOnlyList<StaticDeclarationGraphEdge> Edges { get; }
    public IReadOnlyList<string> InitialRoots { get; }
}

public sealed class StaticDeclarationGraphNode
{
    public StaticDeclarationGraphNode(string stateKey, string displayName, bool isTerminal, Location? location)
    {
        if (string.IsNullOrWhiteSpace(stateKey)) throw new ArgumentException("State key is required.", nameof(stateKey));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        StateKey = stateKey;
        DisplayName = displayName;
        IsTerminal = isTerminal;
        Location = location;
    }

    public string StateKey { get; }
    public string DisplayName { get; }
    public bool IsTerminal { get; }
    public Location? Location { get; }
}

public sealed class StaticDeclarationGraphEdge
{
    public StaticDeclarationGraphEdge(string sourceStateKey, string targetStateKey, StaticDeclarationGraphEdgeKind kind,
        Location? location)
    {
        if (string.IsNullOrWhiteSpace(sourceStateKey)) throw new ArgumentException("Source state key is required.", nameof(sourceStateKey));
        if (string.IsNullOrWhiteSpace(targetStateKey)) throw new ArgumentException("Target state key is required.", nameof(targetStateKey));
        SourceStateKey = sourceStateKey;
        TargetStateKey = targetStateKey;
        Kind = kind;
        Location = location;
    }

    public string SourceStateKey { get; }
    public string TargetStateKey { get; }
    public StaticDeclarationGraphEdgeKind Kind { get; }
    public Location? Location { get; }
}

public enum StaticDeclarationGraphEdgeKind
{
    Transition,
    Completion,
    InitialChild,
    RegionInitial,
    HistoryFallback
}
