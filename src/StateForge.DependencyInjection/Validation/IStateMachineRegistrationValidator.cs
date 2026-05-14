using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Validation;

public interface IStateMachineRegistrationValidator
{
    ValueTask<StateMachineRegistrationValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    ValueTask<StateMachineRegistrationValidationResult> ValidateAsync(IEnumerable<StateMachineIdentity> identities, CancellationToken cancellationToken = default);
}
