using Microsoft.Extensions.Logging;

namespace StateForge.Logging.Diagnostics;

public static class StateMachineLoggingCategories
{
    public const string Transitions = "StateMachine.Transitions";
    public const string Validation = "StateMachine.Validation";
}

public static class StateMachineLoggingEvents
{
    public static readonly EventId TransitionSucceeded = new(1000, nameof(TransitionSucceeded));
    public static readonly EventId TransitionDenied = new(1001, nameof(TransitionDenied));
    public static readonly EventId TransitionFailed = new(1002, nameof(TransitionFailed));
    public static readonly EventId ValidationFinding = new(1100, nameof(ValidationFinding));
}
