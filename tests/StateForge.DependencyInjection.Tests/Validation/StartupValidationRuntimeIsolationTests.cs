using StateForge.DependencyInjection.Tests.Persistence;
using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Validation;

namespace StateForge.DependencyInjection.Tests.Validation;

public sealed class StartupValidationRuntimeIsolationTests
{
    [Fact]
    public async Task ValidationDoesNotInvokePersistenceCoordinatorOrCreateRuntime()
    {
        var registrations = new StateMachineRegistrationCollection();
        var coordinator = new ThrowingCoordinator();
        registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout", o => o.UsePersistence(coordinator));
        var result = await new StateMachineRegistrationValidator(registrations).ValidateAsync();
        Assert.True(result.Succeeded);
    }
}
