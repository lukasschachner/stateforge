using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Tests.Registration;

public sealed class NamedRegistrationTests
{
    [Fact]
    public void RegistersDistinctNamedDefinitions()
    {
        var registrations = new StateMachineRegistrationCollection();
        registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout");
        registrations.Add(DependencyInjectionTestDomain.OtherDefinition(), "other");
        Assert.Equal(2, registrations.Registrations.Count);
    }

    [Fact]
    public void RejectsDuplicateNames()
    {
        var registrations = new StateMachineRegistrationCollection();
        registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout");
        Assert.Throws<StateMachineRegistrationException>(() => registrations.Add(DependencyInjectionTestDomain.Definition(), "checkout"));
    }
}
