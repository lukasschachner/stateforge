using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Immutable graph node representing one declared state.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
public sealed class GraphNode<TState>
{
    /// <summary>Initializes a new graph node.</summary>
    public GraphNode(
        string id,
        TState state,
        string label,
        bool isTerminal,
        MetadataCollection? metadata,
        IEnumerable<GraphActionSummary>? entryActions = null,
        IEnumerable<GraphActionSummary>? exitActions = null,
        bool isComposite = false,
        bool isLeaf = true,
        bool hasParent = false,
        TState? parentState = default,
        int childCount = 0,
        bool hasInitialChild = false,
        TState? initialChildState = default,
        bool hasHistory = false,
        string? historyMode = null,
        bool hasHistoryFallback = false,
        TState? historyFallbackState = default,
        bool isParallelComposite = false,
        string? parallelRegionId = null,
        string? parallelRegionName = null,
        int? parallelRegionOrder = null)
    {
        Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Node id must be non-empty.", nameof(id)) : id;
        State = state;
        Label = string.IsNullOrWhiteSpace(label) ? Id : label;
        IsTerminal = isTerminal;
        Metadata = metadata ?? MetadataCollection.Empty;
        EntryActions = (entryActions ?? []).ToArray();
        ExitActions = (exitActions ?? []).ToArray();
        IsComposite = isComposite;
        IsLeaf = isLeaf;
        HasParent = hasParent;
        ParentState = parentState;
        ChildCount = childCount;
        HasInitialChild = hasInitialChild;
        InitialChildState = initialChildState;
        HasHistory = hasHistory;
        HistoryMode = historyMode;
        HasHistoryFallback = hasHistoryFallback;
        HistoryFallbackState = historyFallbackState;
        IsParallelComposite = isParallelComposite;
        ParallelRegionId = parallelRegionId;
        ParallelRegionName = parallelRegionName;
        ParallelRegionOrder = parallelRegionOrder;
    }

    /// <summary>Gets the stable node identifier used by graph edges.</summary>
    public string Id { get; }

    /// <summary>Gets the typed state value represented by this node.</summary>
    public TState State { get; }

    /// <summary>Gets a human-readable state label.</summary>
    public string Label { get; }

    /// <summary>Gets a value indicating whether the source state is terminal.</summary>
    public bool IsTerminal { get; }

    /// <summary>Gets state metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }

    /// <summary>Gets non-executable summaries of actions that run before entering this state.</summary>
    public IReadOnlyList<GraphActionSummary> EntryActions { get; }

    /// <summary>Gets non-executable summaries of actions that run before exiting this state.</summary>
    public IReadOnlyList<GraphActionSummary> ExitActions { get; }

    public bool IsComposite { get; }
    public bool IsLeaf { get; }
    public bool HasParent { get; }
    public TState? ParentState { get; }
    public int ChildCount { get; }
    public bool HasInitialChild { get; }
    public TState? InitialChildState { get; }
    public bool HasHistory { get; }
    public string? HistoryMode { get; }
    public bool HasHistoryFallback { get; }
    public TState? HistoryFallbackState { get; }
    public bool IsParallelComposite { get; }
    public string? ParallelRegionId { get; }
    public string? ParallelRegionName { get; }
    public int? ParallelRegionOrder { get; }
}