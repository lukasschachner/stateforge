using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Non-executing graph description of a declared transition event.</summary>
/// <typeparam name="TEvent">The event value type used by the definition.</typeparam>
public sealed class GraphEventDescriptor<TEvent>
{
    /// <summary>Initializes a new event descriptor.</summary>
    public GraphEventDescriptor(string identity, string displayName, MetadataCollection? metadata, string category)
    {
        Identity = string.IsNullOrWhiteSpace(identity)
            ? throw new ArgumentException("Event identity must be non-empty.", nameof(identity))
            : identity;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Identity : displayName;
        Metadata = metadata ?? MetadataCollection.Empty;
        Category = string.IsNullOrWhiteSpace(category) ? "Event" : category;
    }

    /// <summary>Gets the stable event identity from the source definition.</summary>
    public string Identity { get; }

    /// <summary>Gets the event display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets event metadata, or an explicit empty collection when absent.</summary>
    public MetadataCollection Metadata { get; }

    /// <summary>Gets a lightweight event descriptor category, such as Value or Type.</summary>
    public string Category { get; }
}