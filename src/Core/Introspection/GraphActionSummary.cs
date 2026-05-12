using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Graph-export representation of a non-executable action summary.</summary>
public sealed class GraphActionSummary
{
    public GraphActionSummary(string ownerId, ActionKind kind, string displayName, int order,
        MetadataCollection? metadata = null)
    {
        OwnerId = string.IsNullOrWhiteSpace(ownerId)
            ? throw new ArgumentException("Owner id must be non-empty.", nameof(ownerId))
            : ownerId;
        Kind = kind;
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Display name must be non-empty.", nameof(displayName))
            : displayName;
        Order = order < 0
            ? throw new ArgumentOutOfRangeException(nameof(order), "Action order must be non-negative.")
            : order;
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public string OwnerId { get; }
    public ActionKind Kind { get; }
    public string DisplayName { get; }
    public int Order { get; }
    public MetadataCollection Metadata { get; }
}