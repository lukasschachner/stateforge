using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Introspection;

/// <summary>Immutable graph projection of a validated reusable state machine definition.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
/// <typeparam name="TEvent">The event value type used by the definition.</typeparam>
public sealed class DefinitionGraph<TState, TEvent>
{
    /// <summary>Initializes a new immutable definition graph without runtime overlay metadata.</summary>
    public DefinitionGraph(
        string id,
        string label,
        IEnumerable<GraphNode<TState>> nodes,
        IEnumerable<GraphEdge<TState, TEvent>> edges,
        MetadataCollection? metadata,
        ValidationResult validation,
        IEnumerable<GraphHierarchyRelationship<TState>>? parentChildRelationships = null,
        IEnumerable<GraphInitialChildMarker<TState>>? initialChildMarkers = null,
        IEnumerable<GraphHistoryMarker<TState>>? historyMarkers = null,
        GraphHierarchyMetadata? hierarchy = null,
        IEnumerable<GraphRegionMetadata<TState>>? regions = null)
        : this(id, label, nodes, edges, metadata, validation, parentChildRelationships, initialChildMarkers,
            historyMarkers, hierarchy, regions, null)
    {
    }

    /// <summary>Initializes a new immutable definition graph with optional runtime overlay metadata.</summary>
    public DefinitionGraph(
        string id,
        string label,
        IEnumerable<GraphNode<TState>> nodes,
        IEnumerable<GraphEdge<TState, TEvent>> edges,
        MetadataCollection? metadata,
        ValidationResult validation,
        IEnumerable<GraphHierarchyRelationship<TState>>? parentChildRelationships,
        IEnumerable<GraphInitialChildMarker<TState>>? initialChildMarkers,
        IEnumerable<GraphHistoryMarker<TState>>? historyMarkers,
        GraphHierarchyMetadata? hierarchy,
        IEnumerable<GraphRegionMetadata<TState>>? regions,
        GraphActiveStateOverlay<TState>? runtimeOverlay)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "definition-graph" : id;
        Label = string.IsNullOrWhiteSpace(label) ? Id : label;
        Nodes = Array.AsReadOnly((nodes ?? throw new ArgumentNullException(nameof(nodes))).ToArray());
        Edges = Array.AsReadOnly((edges ?? throw new ArgumentNullException(nameof(edges))).ToArray());
        Metadata = metadata ?? MetadataCollection.Empty;
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
        ParentChildRelationships = Array.AsReadOnly((parentChildRelationships ?? []).ToArray());
        InitialChildMarkers = Array.AsReadOnly((initialChildMarkers ?? []).ToArray());
        HistoryMarkers = Array.AsReadOnly((historyMarkers ?? []).ToArray());
        Hierarchy = hierarchy ?? GraphHierarchyMetadata.None;
        Regions = Array.AsReadOnly((regions ?? []).ToArray());
        RuntimeOverlay = runtimeOverlay;
    }

    /// <summary>Gets the stable graph identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the graph display label.</summary>
    public string Label { get; }

    /// <summary>Gets graph nodes in declared state order.</summary>
    public IReadOnlyList<GraphNode<TState>> Nodes { get; }

    /// <summary>Gets graph edges in declared transition order.</summary>
    public IReadOnlyList<GraphEdge<TState, TEvent>> Edges { get; }

    /// <summary>Gets definition-level metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }

    /// <summary>Gets validation findings associated with the exported definition.</summary>
    public ValidationResult Validation { get; }

    public IReadOnlyList<GraphHierarchyRelationship<TState>> ParentChildRelationships { get; }
    public IReadOnlyList<GraphInitialChildMarker<TState>> InitialChildMarkers { get; }
    public IReadOnlyList<GraphHistoryMarker<TState>> HistoryMarkers { get; }
    public GraphHierarchyMetadata Hierarchy { get; }
    public IReadOnlyList<GraphRegionMetadata<TState>> Regions { get; }

    /// <summary>Gets runtime active-state overlay metadata when the graph was exported from a runtime instance.</summary>
    public GraphActiveStateOverlay<TState>? RuntimeOverlay { get; }
}