using StateForge.Core.Definitions;

namespace StateForge.Core.Introspection;

/// <summary>Immutable graph edge representing one declared transition.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
/// <typeparam name="TEvent">The event value type used by the definition.</typeparam>
public sealed class GraphEdge<TState, TEvent>
{
    /// <summary>Initializes a new graph edge.</summary>
    public GraphEdge(
        string id,
        string sourceNodeId,
        string targetNodeId,
        TState sourceState,
        TState targetState,
        string label,
        GraphEventDescriptor<TEvent> @event,
        TransitionKind kind,
        GraphConditionSummary<TState, TEvent> conditions,
        MetadataCollection? metadata,
        IEnumerable<GraphActionSummary>? transitionActions = null,
        bool sourceIsComposite = false,
        bool targetIsComposite = false,
        bool targetResolvesThroughInitialChild = false,
        TState? resolvedTargetLeafState = default,
        string hierarchyRelationship = "Flat",
        string regionClassification = "None",
        string? sourceRegionId = null,
        string? targetRegionId = null,
        GraphTriggerKind triggerKind = GraphTriggerKind.Event,
        bool sourceIsParallel = false)
    {
        Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Edge id must be non-empty.", nameof(id)) : id;
        SourceNodeId = string.IsNullOrWhiteSpace(sourceNodeId)
            ? throw new ArgumentException("Source node id must be non-empty.", nameof(sourceNodeId))
            : sourceNodeId;
        TargetNodeId = string.IsNullOrWhiteSpace(targetNodeId)
            ? throw new ArgumentException("Target node id must be non-empty.", nameof(targetNodeId))
            : targetNodeId;
        SourceState = sourceState;
        TargetState = targetState;
        Label = string.IsNullOrWhiteSpace(label) ? Id : label;
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        Kind = kind;
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        Metadata = metadata ?? MetadataCollection.Empty;
        TransitionActions = (transitionActions ?? []).ToArray();
        SourceIsComposite = sourceIsComposite;
        TargetIsComposite = targetIsComposite;
        TargetResolvesThroughInitialChild = targetResolvesThroughInitialChild;
        ResolvedTargetLeafState = resolvedTargetLeafState;
        HierarchyRelationship = hierarchyRelationship;
        RegionClassification = regionClassification;
        SourceRegionId = sourceRegionId;
        TargetRegionId = targetRegionId;
        TriggerKind = triggerKind;
        SourceIsParallel = sourceIsParallel;
    }

    /// <summary>Gets the stable edge identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the source graph node identifier.</summary>
    public string SourceNodeId { get; }

    /// <summary>Gets the target graph node identifier.</summary>
    public string TargetNodeId { get; }

    /// <summary>Gets the typed source state value.</summary>
    public TState SourceState { get; }

    /// <summary>Gets the typed target state value.</summary>
    public TState TargetState { get; }

    /// <summary>Gets the transition display label.</summary>
    public string Label { get; }

    /// <summary>Gets the event descriptor for the transition.</summary>
    public GraphEventDescriptor<TEvent> Event { get; }

    /// <summary>Gets the transition kind.</summary>
    public TransitionKind Kind { get; }

    /// <summary>Gets the non-executable condition summary.</summary>
    public GraphConditionSummary<TState, TEvent> Conditions { get; }

    /// <summary>Gets transition metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }

    /// <summary>Gets non-executable summaries of actions that run for this transition.</summary>
    public IReadOnlyList<GraphActionSummary> TransitionActions { get; }

    public bool SourceIsComposite { get; }
    public bool TargetIsComposite { get; }
    public bool TargetResolvesThroughInitialChild { get; }
    public TState? ResolvedTargetLeafState { get; }
    public string HierarchyRelationship { get; }
    public string RegionClassification { get; }
    public string? SourceRegionId { get; }
    public string? TargetRegionId { get; }
    public GraphTriggerKind TriggerKind { get; }
    public bool SourceIsParallel { get; }
    public bool IsCompletionEdge => TriggerKind == GraphTriggerKind.Completion;
}