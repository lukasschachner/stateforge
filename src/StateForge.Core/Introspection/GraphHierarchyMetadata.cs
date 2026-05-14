namespace StateForge.Core.Introspection;

/// <summary>Graph-level hierarchy capability metadata.</summary>
public sealed record GraphHierarchyMetadata(
    bool HasHierarchy,
    int RelationshipCount,
    int InitialChildMarkerCount,
    int HistoryMarkerCount = 0)
{
    public static GraphHierarchyMetadata None { get; } = new(false, 0, 0);
}

/// <summary>Structured parent-child relationship exported for hierarchy-aware graph consumers.</summary>
public sealed record GraphHierarchyRelationship<TState>(
    TState ParentState,
    TState ChildState,
    int Depth,
    int SiblingOrder);

/// <summary>Structured initial-child marker exported for hierarchy-aware graph consumers.</summary>
public sealed record GraphInitialChildMarker<TState>(
    TState CompositeState,
    TState InitialChildState,
    TState ResolvedInitialLeafState,
    IReadOnlyList<TState> ResolvedPath);

/// <summary>Structured history marker exported for hierarchy-aware graph consumers.</summary>
public sealed record GraphHistoryMarker<TState>(
    TState CompositeState,
    string HistoryMode,
    TState? FallbackTargetState,
    bool DeepHistoryDeterministic);