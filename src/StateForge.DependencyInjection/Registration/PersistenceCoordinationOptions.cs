using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.DependencyInjection.Registration;

/// <summary>Provider-neutral coordination hook supplied by the application for persistent runtime creation.</summary>
public interface IStateMachinePersistenceCoordinator<TState, TEvent>
{
    StateMachineRuntime<TState, TEvent> CreateRuntime(
        StateMachineDefinition<TState, TEvent> definition,
        TState initialState,
        ConcurrencyMode concurrencyMode,
        ITransitionObserver<TState, TEvent>? observer);
}

/// <summary>Stores application-supplied persistence coordination without selecting a provider.</summary>
public sealed class PersistenceCoordinationOptions<TState, TEvent>
{
    public PersistenceCoordinationOptions(IStateMachinePersistenceCoordinator<TState, TEvent> coordinator)
    {
        Coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
    }

    public IStateMachinePersistenceCoordinator<TState, TEvent> Coordinator { get; }
    public bool IsEnabled => true;
}
