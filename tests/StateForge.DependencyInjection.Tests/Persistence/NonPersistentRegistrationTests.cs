using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.Core.Execution;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Runtime;

namespace StateForge.DependencyInjection.Tests.Persistence;

public sealed class NonPersistentRegistrationTests
{
    [Fact]
    public void NonPersistentRegistrationCreatesNormalRuntime()
    {
        var registrations = new StateMachineRegistrationCollection();
        var registration = registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout");
        var runtime = new StateMachineRuntimeFactory<TestState, TestEvent>(new SimpleProvider(), registration).Create(TestState.Created, ConcurrencyMode.Fast);
        Assert.Equal(TestState.Created, runtime.CurrentState);
    }
}
