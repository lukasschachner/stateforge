namespace StateForge.DependencyInjection.Registration;

/// <summary>Exception raised for invalid or ambiguous state machine adapter registrations.</summary>
public sealed class StateMachineRegistrationException : InvalidOperationException
{
    public StateMachineRegistrationException(string message) : base(message) { }
    public StateMachineRegistrationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Result of adapter registration validation.</summary>
public sealed record StateMachineRegistrationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static StateMachineRegistrationResult Success { get; } = new(true, Array.Empty<string>());
    public static StateMachineRegistrationResult Failure(params string[] errors) => new(false, errors);
}
