using StateForge.Core.Execution;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Runtime;

/// <summary>Default runtime factory for a single registered definition.</summary>
public sealed class StateMachineRuntimeFactory<TState, TEvent> : IStateMachineRuntimeFactory<TState, TEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StateMachineRegistration<TState, TEvent> _registration;

    public StateMachineRuntimeFactory(IServiceProvider serviceProvider, StateMachineRegistration<TState, TEvent> registration)
    {
        _serviceProvider = serviceProvider;
        _registration = registration;
    }

    public StateMachineIdentity Identity => _registration.Identity;
    public Core.Definitions.StateMachineDefinition<TState, TEvent> Definition => _registration.Definition;

    public StateMachineRuntime<TState, TEvent> Create(TState initialState, ConcurrencyMode concurrencyMode) =>
        Create(initialState, concurrencyMode, null);

    public StateMachineRuntime<TState, TEvent> Create(TState initialState, ConcurrencyMode concurrencyMode,
        ITransitionObserver<TState, TEvent>? observer)
    {
        var pipeline = BuildObserverPipeline(observer);
        if (_registration.PersistenceCoordination is { Coordinator: not null } persistence)
            return persistence.Coordinator.CreateRuntime(_registration.Definition, initialState, concurrencyMode, pipeline);

        return _registration.Definition.CreateRuntime(initialState, concurrencyMode, pipeline);
    }

    private ITransitionObserver<TState, TEvent>? BuildObserverPipeline(ITransitionObserver<TState, TEvent>? supplied)
    {
        var observers = _registration.ObserverRegistrations
            .OrderBy(o => o.Order)
            .Select(o => o.Resolve(_serviceProvider))
            .ToList();
        if (supplied is not null) observers.Add(supplied);
        return observers.Count switch
        {
            0 => null,
            1 => observers[0],
            _ => new CompositeTransitionObserver<TState, TEvent>(observers)
        };
    }
}
