using StateForge.Core.Definitions;

namespace StateForge.Core.Introspection;

/// <summary>Identifies how graph export summarized transition conditions.</summary>
public enum GraphConditionSummaryKind
{
    /// <summary>The transition has no declared conditions.</summary>
    None,

    /// <summary>All declared conditions are represented in declaration order.</summary>
    All,

    /// <summary>Condition information was unavailable.</summary>
    Unavailable
}

/// <summary>Non-executable summary of transition conditions for graph consumers.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
/// <typeparam name="TEvent">The event value type used by the definition.</typeparam>
public sealed class GraphConditionSummary<TState, TEvent>
{
    /// <summary>Initializes a new condition summary.</summary>
    public GraphConditionSummary(
        GraphConditionSummaryKind kind,
        string displayText,
        IEnumerable<GraphConditionDescriptor> conditions,
        MetadataCollection? metadata)
    {
        Kind = kind;
        DisplayText = string.IsNullOrWhiteSpace(displayText) ? kind.ToString() : displayText;
        Conditions = Array.AsReadOnly((conditions ?? throw new ArgumentNullException(nameof(conditions))).ToArray());
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    /// <summary>Gets the summary kind.</summary>
    public GraphConditionSummaryKind Kind { get; }

    /// <summary>Gets human-readable summary text.</summary>
    public string DisplayText { get; }

    /// <summary>Gets condition descriptors in declaration order.</summary>
    public IReadOnlyList<GraphConditionDescriptor> Conditions { get; }

    /// <summary>Gets summary metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }
}

/// <summary>Non-executing description of one declared condition.</summary>
public sealed class GraphConditionDescriptor
{
    /// <summary>Initializes a new condition descriptor.</summary>
    public GraphConditionDescriptor(int position, string displayName, MetadataCollection? metadata)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

        Position = position;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? $"Condition {position + 1}" : displayName;
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    /// <summary>Gets the zero-based condition position on the source transition.</summary>
    public int Position { get; }

    /// <summary>Gets a safe human-readable condition display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets condition metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }
}