using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Runtime;

namespace StateForge.DependencyInjection.Tests.Persistence;

public sealed class PersistenceCoordinationTests
{
    [Fact]
    public void UsesApplicationSuppliedPersistenceCoordinator()
    {
        var coordinator = new CountingCoordinator();
        var registrations = new StateMachineRegistrationCollection();
        var registration = registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout", o => o.UsePersistence(coordinator));
        var runtime = new StateMachineRuntimeFactory<TestState, TestEvent>(new SimpleProvider(), registration).Create(TestState.Created, ConcurrencyMode.Fast);
        Assert.Equal(1, coordinator.Count);
        Assert.Equal(TestState.Created, runtime.CurrentState);
    }
}

public sealed class CountingCoordinator : IStateMachinePersistenceCoordinator<TestState, TestEvent>
{
    public int Count { get; private set; }
    public StateMachineRuntime<TestState, TestEvent> CreateRuntime(StateMachineDefinition<TestState, TestEvent> definition, TestState initialState, ConcurrencyMode concurrencyMode, ITransitionObserver<TestState, TestEvent>? observer)
    {
        Count++;
        return definition.CreateRuntime(initialState, concurrencyMode, observer);
    }
}

public sealed class ThrowingCoordinator : IStateMachinePersistenceCoordinator<TestState, TestEvent>
{
    public StateMachineRuntime<TestState, TestEvent> CreateRuntime(StateMachineDefinition<TestState, TestEvent> definition, TestState initialState, ConcurrencyMode concurrencyMode, ITransitionObserver<TestState, TestEvent>? observer) => throw new InvalidOperationException();
}

public sealed class SimpleProvider : IServiceProvider { public object? GetService(Type serviceType) => null; }
