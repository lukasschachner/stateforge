using StateForge.Core.Definitions;

namespace StateForge.DependencyInjection.Registration;

/// <summary>Immutable descriptor for an application-registered state machine definition.</summary>
public sealed class StateMachineRegistration<TState, TEvent> : IStateMachineRegistration
{
    public StateMachineRegistration(
        StateMachineIdentity identity,
        StateMachineDefinition<TState, TEvent> definition,
        StateMachineRegistrationOptions<TState, TEvent>? options = null)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(definition);
        if (!identity.MatchesTypes<TState, TEvent>())
            throw new StateMachineRegistrationException($"Registration identity '{identity}' does not match definition type arguments.");

        Identity = identity;
        Definition = definition;
        ObserverRegistrations = (options?.ObserverRegistrations ?? []).ToArray();
        ValidateOnStartup = options?.ValidateOnStartupEnabled ?? false;
        PersistenceCoordination = options?.PersistenceCoordination;
    }

    public StateMachineIdentity Identity { get; }
    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public IReadOnlyList<ObserverRegistration<TState, TEvent>> ObserverRegistrations { get; }
    public bool ValidateOnStartup { get; }
    public PersistenceCoordinationOptions<TState, TEvent>? PersistenceCoordination { get; }
    Type IStateMachineRegistration.StateType => typeof(TState);
    Type IStateMachineRegistration.EventType => typeof(TEvent);
    object IStateMachineRegistration.Definition => Definition;
}

public interface IStateMachineRegistration
{
    StateMachineIdentity Identity { get; }
    Type StateType { get; }
    Type EventType { get; }
    object Definition { get; }
    bool ValidateOnStartup { get; }
}
