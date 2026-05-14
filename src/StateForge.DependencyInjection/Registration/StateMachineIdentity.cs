namespace StateForge.DependencyInjection.Registration;

/// <summary>Identifies an application-registered state machine by name and/or state-event type pair.</summary>
public sealed record StateMachineIdentity
{
    public StateMachineIdentity(string? name, Type stateType, Type eventType)
    {
        if (stateType is null) throw new ArgumentNullException(nameof(stateType));
        if (eventType is null) throw new ArgumentNullException(nameof(eventType));
        if (name is not null && string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Machine name must be non-empty when supplied.", nameof(name));

        Name = name;
        StateType = stateType;
        EventType = eventType;
    }

    public string? Name { get; }
    public Type StateType { get; }
    public Type EventType { get; }
    public string DisplayName => Name ?? $"{StateType.Name}/{EventType.Name}";

    public static StateMachineIdentity Named<TState, TEvent>(string name) => new(name, typeof(TState), typeof(TEvent));
    public static StateMachineIdentity Typed<TState, TEvent>() => new(null, typeof(TState), typeof(TEvent));
    public bool MatchesTypes<TState, TEvent>() => StateType == typeof(TState) && EventType == typeof(TEvent);
    public override string ToString() => DisplayName;
}
