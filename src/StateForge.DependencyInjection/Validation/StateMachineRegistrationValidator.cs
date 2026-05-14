using StateForge.Core.Definitions;
using StateForge.Core.Validation;
using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Validation;

/// <summary>Validates registered definitions without creating runtime instances.</summary>
public sealed class StateMachineRegistrationValidator : IStateMachineRegistrationValidator
{
    private readonly StateMachineRegistrationCollection _registrations;
    private readonly StateMachineValidationOptions _options;

    public StateMachineRegistrationValidator(StateMachineRegistrationCollection registrations, StateMachineValidationOptions? options = null)
    {
        _registrations = registrations;
        _options = options ?? new StateMachineValidationOptions();
    }

    public ValueTask<StateMachineRegistrationValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selected = _options.IncludeAllRegistrations
            ? _registrations.Registrations
            : _registrations.Registrations.Where(r => r.ValidateOnStartup);
        return new ValueTask<StateMachineRegistrationValidationResult>(Validate(selected));
    }

    public ValueTask<StateMachineRegistrationValidationResult> ValidateAsync(IEnumerable<StateMachineIdentity> identities,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requested = identities.ToArray();
        var selected = requested.Select(identity => _registrations.Registrations.SingleOrDefault(r => Equals(r.Identity, identity))
            ?? throw new StateMachineRegistrationException($"No state machine registration matches '{identity}'."));
        return new ValueTask<StateMachineRegistrationValidationResult>(Validate(selected));
    }

    private StateMachineRegistrationValidationResult Validate(IEnumerable<IStateMachineRegistration> selected)
    {
        var entries = selected.Select(r => new StateMachineRegistrationValidationEntry(r.Identity, ValidateDefinition(r.Definition))).ToArray();
        return new StateMachineRegistrationValidationResult(entries, _options.TreatWarningsAsFailures);
    }

    private static ValidationResult ValidateDefinition(object definition)
    {
        var type = definition.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(StateMachineDefinition<,>))
        {
            var method = type.GetMethod(nameof(StateMachineDefinition<int, int>.Validate), Type.EmptyTypes)!;
            return (ValidationResult)method.Invoke(definition, null)!;
        }

        throw new StateMachineRegistrationException("Registered definition type is not supported.");
    }
}
