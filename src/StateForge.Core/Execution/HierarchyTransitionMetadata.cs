namespace StateForge.Core.Execution;

/// <summary>Non-executable metadata describing how a transition was resolved through a hierarchy.</summary>
public sealed record HierarchyTransitionMetadata<TState>(
    TState DeclaredSourceState,
    TState DeclaredTargetState,
    TState ResolvedSourceState,
    TState ResolvedTargetLeafState,
    ActiveStatePath<TState> SourcePath,
    ActiveStatePath<TState> TargetPath,
    bool SourceMatchedAncestor,
    bool TargetResolvedThroughInitialChild);