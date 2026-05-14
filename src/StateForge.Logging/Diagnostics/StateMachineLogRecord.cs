using Microsoft.Extensions.Logging;

namespace StateForge.Logging.Diagnostics;

public enum StateMachineLogCategory
{
    TransitionSuccess,
    TransitionDenied,
    TransitionFailure,
    ValidationFinding
}

public sealed record StateMachineLogRecord(
    string? MachineIdentity,
    StateMachineLogCategory Category,
    EventId EventId,
    LogLevel Level,
    string? SourceState,
    string? TargetState,
    string? EventIdentity,
    string? Outcome,
    IReadOnlyList<string> DiagnosticCodes,
    string Message);
