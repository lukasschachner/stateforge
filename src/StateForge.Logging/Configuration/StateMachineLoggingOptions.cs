using Microsoft.Extensions.Logging;
using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Configuration;

public sealed class StateMachineLoggingOptions
{
    public bool IncludeTransitions { get; set; } = true;
    public bool IncludeDenials { get; set; } = true;
    public bool IncludeFailures { get; set; } = true;
    public bool IncludeValidationFindings { get; set; } = true;
    public bool EnableScopes { get; set; } = true;
    public int MaxMetadataValueLength { get; set; } = 128;
    public HashSet<string> IncludedMachineNames { get; } = new(StringComparer.Ordinal);
    public Func<StateMachineLogRecord, bool>? Filter { get; set; }
    public EventId TransitionSucceededEventId { get; set; } = StateMachineLoggingEvents.TransitionSucceeded;
    public EventId TransitionDeniedEventId { get; set; } = StateMachineLoggingEvents.TransitionDenied;
    public EventId TransitionFailedEventId { get; set; } = StateMachineLoggingEvents.TransitionFailed;
    public EventId ValidationFindingEventId { get; set; } = StateMachineLoggingEvents.ValidationFinding;

    public StateMachineLoggingOptions IncludeTransitionSuccesses() { IncludeTransitions = true; return this; }
    public StateMachineLoggingOptions IncludeTransitionDenials() { IncludeDenials = true; return this; }
    public StateMachineLoggingOptions IncludeTransitionFailures() { IncludeFailures = true; return this; }
    public StateMachineLoggingOptions IncludeValidationDiagnostics() { IncludeValidationFindings = true; return this; }
    public StateMachineLoggingOptions FilterMachine(string machineName) { IncludedMachineNames.Add(machineName); return this; }
    public StateMachineLoggingOptions UseDefaultSafeDiagnostics() => this;
}
