using StateForge.DependencyInjection.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using StateForge.Core.Execution;
using StateForge.DependencyInjection;
using StateForge.DependencyInjection.Runtime;

namespace StateForge.DependencyInjection.Tests.Runtime;

public sealed class RuntimeFactoryTests
{
    [Fact]
    public void CreatesRuntimeWithExplicitInitialStateAndConcurrencyMode()
    {
        var services = new ServiceCollection();
        services.AddStateMachines(m => m.AddDefinition(DependencyInjectionTestDomain.Definition()));
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IStateMachineRuntimeFactoryResolver>().GetFactory<TestState, TestEvent>();

        var runtime = factory.Create(TestState.Created, ConcurrencyMode.Serialized);

        Assert.Equal(TestState.Created, runtime.CurrentState);
        Assert.Equal(ConcurrencyMode.Serialized, runtime.ConcurrencyMode);
    }
}
