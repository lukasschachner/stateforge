namespace StateMachineLibrary.Core.Definitions;

/// <summary>Describes how an event is matched for a transition.</summary>
public sealed class EventDefinition<TEvent>
{
    private readonly Func<TEvent, bool> _matcher;

    private EventDefinition(string identity, string displayName, Func<TEvent, bool> matcher,
        MetadataCollection? metadata = null)
    {
        Identity = identity;
        DisplayName = displayName;
        _matcher = matcher;
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public string Identity { get; }
    public string DisplayName { get; }
    public MetadataCollection Metadata { get; }

    public bool Matches(TEvent @event)
    {
        return _matcher(@event);
    }

    public static EventDefinition<TEvent> ForValue(TEvent value, IEqualityComparer<TEvent>? comparer = null,
        string? displayName = null)
    {
        comparer ??= EqualityComparer<TEvent>.Default;
        var identity = $"value:{value}";
        return new EventDefinition<TEvent>(identity, displayName ?? value?.ToString() ?? "<null>",
            e => comparer.Equals(e, value));
    }

    public static EventDefinition<TEvent> ForType<TSpecificEvent>(string? displayName = null)
        where TSpecificEvent : TEvent
    {
        var type = typeof(TSpecificEvent);
        return new EventDefinition<TEvent>($"type:{type.FullName ?? type.Name}", displayName ?? type.Name,
            e => e is TSpecificEvent);
    }

    internal static EventDefinition<TEvent> ForCompletion()
    {
        return new EventDefinition<TEvent>("completion", "completion", _ => false,
            MetadataCollection.Empty.With("triggerKind", TransitionTriggerKind.Completion));
    }

    public override string ToString()
    {
        return DisplayName;
    }
}