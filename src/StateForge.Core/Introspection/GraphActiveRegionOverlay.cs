namespace StateForge.Core.Introspection;

/// <summary>Runtime active-state overlay metadata for one parallel region.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
public sealed class GraphActiveRegionOverlay<TState>
{
    /// <summary>Initializes a new immutable active region overlay.</summary>
    public GraphActiveRegionOverlay(
        string regionId,
        string? regionName,
        int regionOrder,
        TState activeLeafState,
        string? activeLeafNodeId,
        IEnumerable<TState> activePath,
        IEnumerable<string> activePathNodeIds,
        bool isTerminal,
        bool isComplete,
        long sequence)
    {
        RegionId = string.IsNullOrWhiteSpace(regionId)
            ? throw new ArgumentException("Region id must be non-empty.", nameof(regionId))
            : regionId;
        RegionName = regionName;
        RegionOrder = regionOrder;
        ActiveLeafState = activeLeafState;
        ActiveLeafNodeId = activeLeafNodeId;
        ActivePath = Array.AsReadOnly((activePath ?? throw new ArgumentNullException(nameof(activePath))).ToArray());
        ActivePathNodeIds = Array.AsReadOnly((activePathNodeIds ?? throw new ArgumentNullException(nameof(activePathNodeIds))).ToArray());
        IsTerminal = isTerminal;
        IsComplete = isComplete;
        Sequence = sequence;
    }

    /// <summary>Gets the stable region identifier from the definition.</summary>
    public string RegionId { get; }

    /// <summary>Gets the declared region display name, when available.</summary>
    public string? RegionName { get; }

    /// <summary>Gets the declaration order of the region within its owning composite.</summary>
    public int RegionOrder { get; }

    /// <summary>Gets the active leaf state inside this region.</summary>
    public TState ActiveLeafState { get; }

    /// <summary>Gets the existing graph node id for <see cref="ActiveLeafState"/>, when resolvable.</summary>
    public string? ActiveLeafNodeId { get; }

    /// <summary>Gets the active path for this region ordered from outer ancestor to active leaf.</summary>
    public IReadOnlyList<TState> ActivePath { get; }

    /// <summary>Gets existing graph node ids for <see cref="ActivePath"/> in the same order.</summary>
    public IReadOnlyList<string> ActivePathNodeIds { get; }

    /// <summary>Gets whether the active region leaf is terminal for this region.</summary>
    public bool IsTerminal { get; }

    /// <summary>Gets whether this region is complete according to runtime introspection semantics.</summary>
    public bool IsComplete { get; }

    /// <summary>Gets the runtime sequence captured with the containing overlay.</summary>
    public long Sequence { get; }
}
