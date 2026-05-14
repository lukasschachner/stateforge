using StateForge.Core.Definitions;

namespace StateForge.DependencyInjection.Registration;

/// <summary>Collects state machine registrations and rejects duplicate names or ambiguous typed lookup.</summary>
public sealed class StateMachineRegistrationCollection
{
    private readonly List<IStateMachineRegistration> _registrations = [];

    public IReadOnlyList<IStateMachineRegistration> Registrations => _registrations;

    public StateMachineRegistration<TState, TEvent> Add<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        string? name = null,
        Action<StateMachineRegistrationOptions<TState, TEvent>>? configure = null)
    {
        var identity = new StateMachineIdentity(name, typeof(TState), typeof(TEvent));
        if (identity.Name is not null && _registrations.Any(r => string.Equals(r.Identity.Name, identity.Name, StringComparison.Ordinal)))
            throw new StateMachineRegistrationException($"A state machine named '{identity.Name}' is already registered.");

        var options = new StateMachineRegistrationOptions<TState, TEvent>();
        configure?.Invoke(options);
        var registration = new StateMachineRegistration<TState, TEvent>(identity, definition, options);
        _registrations.Add(registration);
        return registration;
    }

    public StateMachineRegistration<TState, TEvent> GetByName<TState, TEvent>(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Machine name must be non-empty.", nameof(name));
        var match = _registrations.SingleOrDefault(r => string.Equals(r.Identity.Name, name, StringComparison.Ordinal));
        if (match is null) throw new StateMachineRegistrationException($"No state machine named '{name}' is registered.");
        if (match is not StateMachineRegistration<TState, TEvent> typed)
            throw new StateMachineRegistrationException($"State machine '{name}' is not compatible with {typeof(TState).Name}/{typeof(TEvent).Name}.");
        return typed;
    }

    public StateMachineRegistration<TState, TEvent> GetTyped<TState, TEvent>()
    {
        var matches = _registrations.OfType<StateMachineRegistration<TState, TEvent>>().ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new StateMachineRegistrationException($"No state machine for {typeof(TState).Name}/{typeof(TEvent).Name} is registered."),
            _ => throw new StateMachineRegistrationException($"Multiple state machines for {typeof(TState).Name}/{typeof(TEvent).Name} are registered; resolve by name.")
        };
    }
}
