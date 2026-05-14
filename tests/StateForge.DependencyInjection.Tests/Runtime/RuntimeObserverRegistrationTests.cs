using StateForge.DependencyInjection.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using StateForge.Core.Execution;
using StateForge.DependencyInjection;
using StateForge.DependencyInjection.Runtime;

namespace StateForge.DependencyInjection.Tests.Runtime;

public sealed class RuntimeObserverRegistrationTests
{
    [Fact]
    public async Task AttachesRegisteredObserversToCreatedRuntime()
    {
        var observer = new RecordingObserver();
        var services = new ServiceCollection();
        services.AddStateMachines(m => m.AddDefinition(DependencyInjectionTestDomain.Definition(), o => o.UseObserver(observer)));
        using var provider = services.BuildServiceProvider();
        var runtime = provider.GetRequiredService<IStateMachineRuntimeFactoryResolver>().GetFactory<TestState, TestEvent>().Create(TestState.Created, ConcurrencyMode.Fast);

        await runtime.ApplyAsync(new Pay());

        Assert.Contains(observer.Observations, o => o.Kind == TransitionObservationKind.Outcome);
    }
}
