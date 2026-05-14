using StateForge.Core.Execution;

namespace StateForge.DependencyInjection.Registration;

/// <summary>Configures a single application state machine registration.</summary>
public sealed class StateMachineRegistrationOptions<TState, TEvent>
{
    private readonly List<ObserverRegistration<TState, TEvent>> _observers = [];

    public IReadOnlyList<ObserverRegistration<TState, TEvent>> ObserverRegistrations => _observers;
    public bool ValidateOnStartupEnabled { get; private set; }
    public PersistenceCoordinationOptions<TState, TEvent>? PersistenceCoordination { get; private set; }

    public StateMachineRegistrationOptions<TState, TEvent> UseObserver(ITransitionObserver<TState, TEvent> observer, int order = 0)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(ObserverRegistration<TState, TEvent>.ForInstance(observer, order));
        return this;
    }

    public StateMachineRegistrationOptions<TState, TEvent> UseObserver<TObserver>(int order = 0)
        where TObserver : ITransitionObserver<TState, TEvent>
    {
        _observers.Add(ObserverRegistration<TState, TEvent>.ForService<TObserver>(order));
        return this;
    }

    public StateMachineRegistrationOptions<TState, TEvent> ValidateOnApplicationStartup()
    {
        ValidateOnStartupEnabled = true;
        return this;
    }

    public StateMachineRegistrationOptions<TState, TEvent> ValidateOnStartupCheck() => ValidateOnApplicationStartup();
    public StateMachineRegistrationOptions<TState, TEvent> ValidateOnStartup() => ValidateOnApplicationStartup();

    public StateMachineRegistrationOptions<TState, TEvent> UsePersistence(
        IStateMachinePersistenceCoordinator<TState, TEvent> coordinator)
    {
        PersistenceCoordination = new PersistenceCoordinationOptions<TState, TEvent>(coordinator);
        return this;
    }
}
