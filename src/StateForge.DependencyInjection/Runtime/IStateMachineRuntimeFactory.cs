using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Runtime;

/// <summary>Creates state-owning runtimes while keeping initial state and concurrency mode caller-owned.</summary>
public interface IStateMachineRuntimeFactory<TState, TEvent>
{
    StateMachineIdentity Identity { get; }
    StateMachineDefinition<TState, TEvent> Definition { get; }
    StateMachineRuntime<TState, TEvent> Create(TState initialState, ConcurrencyMode concurrencyMode);
    StateMachineRuntime<TState, TEvent> Create(TState initialState, ConcurrencyMode concurrencyMode, ITransitionObserver<TState, TEvent>? observer);
}

/// <summary>Resolves runtime factories by name or by unambiguous state/event type pair.</summary>
public interface IStateMachineRuntimeFactoryResolver
{
    IStateMachineRuntimeFactory<TState, TEvent> GetFactory<TState, TEvent>();
    IStateMachineRuntimeFactory<TState, TEvent> GetFactory<TState, TEvent>(string name);
}
