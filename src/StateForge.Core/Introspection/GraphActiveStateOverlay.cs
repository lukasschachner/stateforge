namespace StateForge.Core.Introspection;

/// <summary>Renderer-neutral runtime active-state overlay attached to exported definition graph data.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
public sealed class GraphActiveStateOverlay<TState>
{
    /// <summary>Initializes a new immutable active-state overlay.</summary>
    public GraphActiveStateOverlay(
        GraphActiveStateOverlayKind shapeKind,
        long sequence,
        TState? activeLeafState = default,
        string? activeLeafNodeId = null,
        IEnumerable<TState>? activePath = null,
        IEnumerable<string>? activePathNodeIds = null,
        TState? owningCompositeState = default,
        string? owningCompositeNodeId = null,
        bool isTerminal = false,
        bool isComplete = false,
        TState? completionScopeState = default,
        IEnumerable<string>? completedRegionIds = null,
        IEnumerable<GraphActiveRegionOverlay<TState>>? regions = null)
    {
        ShapeKind = shapeKind;
        Sequence = sequence;
        ActiveLeafState = activeLeafState;
        ActiveLeafNodeId = activeLeafNodeId;
        ActivePath = Array.AsReadOnly((activePath ?? []).ToArray());
        ActivePathNodeIds = Array.AsReadOnly((activePathNodeIds ?? []).ToArray());
        OwningCompositeState = owningCompositeState;
        OwningCompositeNodeId = owningCompositeNodeId;
        IsTerminal = isTerminal;
        IsComplete = isComplete;
        CompletionScopeState = completionScopeState;
        CompletedRegionIds = Array.AsReadOnly((completedRegionIds ?? []).ToArray());
        Regions = Array.AsReadOnly((regions ?? []).ToArray());
    }

    /// <summary>Gets the active-state shape classification.</summary>
    public GraphActiveStateOverlayKind ShapeKind { get; }

    /// <summary>Gets the runtime active-state sequence captured with the overlay.</summary>
    public long Sequence { get; }

    /// <summary>Gets the active leaf for flat and hierarchical active shapes.</summary>
    public TState? ActiveLeafState { get; }

    /// <summary>Gets the existing graph node id for <see cref="ActiveLeafState"/>, when resolvable.</summary>
    public string? ActiveLeafNodeId { get; }

    /// <summary>Gets the active path for hierarchical active shapes, ordered from ancestor to leaf.</summary>
    public IReadOnlyList<TState> ActivePath { get; }

    /// <summary>Gets existing graph node ids for <see cref="ActivePath"/> in the same order.</summary>
    public IReadOnlyList<string> ActivePathNodeIds { get; }

    /// <summary>Gets the owning parallel composite state for parallel active shapes.</summary>
    public TState? OwningCompositeState { get; }

    /// <summary>Gets the existing graph node id for <see cref="OwningCompositeState"/>, when resolvable.</summary>
    public string? OwningCompositeNodeId { get; }

    /// <summary>Gets terminal status for the active leaf or owning composite, when meaningful.</summary>
    public bool IsTerminal { get; }

    /// <summary>Gets completion status according to runtime introspection semantics.</summary>
    public bool IsComplete { get; }

    /// <summary>Gets the completion scope state reported by runtime introspection, when recognized.</summary>
    public TState? CompletionScopeState { get; }

    /// <summary>Gets completed parallel region ids in declaration order.</summary>
    public IReadOnlyList<string> CompletedRegionIds { get; }

    /// <summary>Gets declaration-ordered active region overlays for parallel active shapes.</summary>
    public IReadOnlyList<GraphActiveRegionOverlay<TState>> Regions { get; }
}
