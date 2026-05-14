using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Runtime;
using StateForge.DependencyInjection.Validation;

namespace StateForge.DependencyInjection;

/// <summary>Marker type for public API snapshot validation of the dependency-injection adapter package.</summary>
public sealed class DependencyInjectionPublicApi
{
    public Type RegistrationCollectionType => typeof(StateMachineRegistrationCollection);
    public Type RuntimeFactoryResolverType => typeof(IStateMachineRuntimeFactoryResolver);
    public Type RegistrationValidatorType => typeof(IStateMachineRegistrationValidator);
}
