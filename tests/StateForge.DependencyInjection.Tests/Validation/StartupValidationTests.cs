using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Validation;

namespace StateForge.DependencyInjection.Tests.Validation;

public sealed class StartupValidationTests
{
    [Fact]
    public async Task ValidRegistrationPassesStartupValidation()
    {
        var registrations = new StateMachineRegistrationCollection();
        registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout");
        var result = await new StateMachineRegistrationValidator(registrations).ValidateAsync();
        Assert.True(result.Succeeded);
    }
}
