using StateForge.DependencyInjection.Tests.TestSupport;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Tests.Registration;

public sealed class TypedRegistrationTests
{
    [Fact]
    public void ResolvesSingleTypedRegistration()
    {
        var registrations = new StateMachineRegistrationCollection();
        registrations.Add(DependencyInjectionTestDomain.Definition());
        Assert.NotNull(registrations.GetTyped<TestState, TestEvent>());
    }

    [Fact]
    public void RejectsAmbiguousTypedLookup()
    {
        var registrations = new StateMachineRegistrationCollection();
        registrations.Add(DependencyInjectionTestDomain.Definition(), "a");
        registrations.Add(DependencyInjectionTestDomain.Definition(), "b");
        Assert.Throws<StateMachineRegistrationException>(() => registrations.GetTyped<TestState, TestEvent>());
    }
}
