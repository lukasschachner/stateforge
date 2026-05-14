using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Runtime;

/// <summary>Default factory resolver backed by the adapter registration collection.</summary>
public sealed class StateMachineRuntimeFactoryResolver : IStateMachineRuntimeFactoryResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StateMachineRegistrationCollection _registrations;

    public StateMachineRuntimeFactoryResolver(IServiceProvider serviceProvider, StateMachineRegistrationCollection registrations)
    {
        _serviceProvider = serviceProvider;
        _registrations = registrations;
    }

    public IStateMachineRuntimeFactory<TState, TEvent> GetFactory<TState, TEvent>() =>
        new StateMachineRuntimeFactory<TState, TEvent>(_serviceProvider, _registrations.GetTyped<TState, TEvent>());

    public IStateMachineRuntimeFactory<TState, TEvent> GetFactory<TState, TEvent>(string name) =>
        new StateMachineRuntimeFactory<TState, TEvent>(_serviceProvider, _registrations.GetByName<TState, TEvent>(name));
}
