using Microsoft.Extensions.Logging;
using StateForge.Core.Execution;
using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Observers;

public sealed class LoggingTransitionObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    private readonly ILogger _logger;
    private readonly StateMachineLoggingOptions _options;

    public LoggingTransitionObserver(ILogger logger, StateMachineLoggingOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new StateMachineLoggingOptions();
    }

    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation, CancellationToken cancellationToken = default)
    {
        var record = ToRecord(observation);
        if (record is null || !StateMachineLogFilter.ShouldLog(_options, record)) return ValueTask.CompletedTask;
        using var scope = StateMachineLoggingScope.Begin(_logger, _options, record);
        _logger.Log(record.Level, record.EventId,
            "{Message} Machine={MachineIdentity} Event={EventIdentity} Source={SourceState} Target={TargetState} Outcome={Outcome} Codes={DiagnosticCodes}",
            record.Message,
            record.MachineIdentity,
            record.EventIdentity,
            record.SourceState,
            record.TargetState,
            record.Outcome,
            string.Join(',', record.DiagnosticCodes));
        return ValueTask.CompletedTask;
    }

    private StateMachineLogRecord? ToRecord(TransitionObservation<TState, TEvent> observation)
    {
        if (observation.Kind != TransitionObservationKind.Outcome && observation.Kind != TransitionObservationKind.NotPermitted && observation.Kind != TransitionObservationKind.ConditionDenied && observation.Kind != TransitionObservationKind.BehaviorFailed && observation.Kind != TransitionObservationKind.ValidationFailure)
            return null;

        var category = observation.Committed
            ? StateMachineLogCategory.TransitionSuccess
            : observation.Kind is TransitionObservationKind.BehaviorFailed or TransitionObservationKind.ValidationFailure or TransitionObservationKind.Cancelled
                ? StateMachineLogCategory.TransitionFailure
                : StateMachineLogCategory.TransitionDenied;
        var eventId = category switch
        {
            StateMachineLogCategory.TransitionSuccess => _options.TransitionSucceededEventId,
            StateMachineLogCategory.TransitionFailure => _options.TransitionFailedEventId,
            _ => _options.TransitionDeniedEventId
        };
        var level = category switch
        {
            StateMachineLogCategory.TransitionSuccess => LogLevel.Information,
            StateMachineLogCategory.TransitionFailure => LogLevel.Error,
            _ => LogLevel.Warning
        };
        return new StateMachineLogRecord(
            observation.MachineName,
            category,
            eventId,
            level,
            SafeDiagnosticFormatter.SafeValue(observation.SourceState, _options),
            SafeDiagnosticFormatter.SafeValue(observation.TargetState, _options),
            observation.EventType?.Name ?? typeof(TEvent).Name,
            observation.OutcomeCategory?.ToString() ?? observation.Kind.ToString(),
            SafeDiagnosticFormatter.DiagnosticCodes(observation),
            category switch
            {
                StateMachineLogCategory.TransitionSuccess => "State machine transition succeeded.",
                StateMachineLogCategory.TransitionFailure => "State machine transition failed.",
                _ => "State machine transition was denied."
            });
    }
}
