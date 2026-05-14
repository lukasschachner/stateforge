using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Validation;

namespace StateForge.DependencyInjection.Tests.Validation;

public sealed class StartupValidationSelectionTests
{
    [Fact]
    public async Task UnknownSelectedRegistrationFailsClearly()
    {
        var registrations = new StateMachineRegistrationCollection();
        var validator = new StateMachineRegistrationValidator(registrations);
        await Assert.ThrowsAsync<StateMachineRegistrationException>(async () => await validator.ValidateAsync([StateMachineIdentity.Named<TestState, TestEvent>("missing")]));
    }
}
