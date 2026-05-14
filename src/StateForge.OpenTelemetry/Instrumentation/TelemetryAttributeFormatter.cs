using StateForge.Core.Execution;

namespace StateForge.OpenTelemetry;

internal sealed class TelemetryAttributeFormatter
{
    private readonly StateMachineTelemetryOptions _options;

    public TelemetryAttributeFormatter(StateMachineTelemetryOptions options)
    {
        _options = options;
    }

    public string? FormatState(object? state)
    {
        return _options.StateFormatter?.Invoke(state) ?? state?.ToString();
    }

    public string? FormatEvent(TransitionObservation<object?, object?> observation)
    {
        return _options.EventFormatter?.Invoke(observation.Event) ?? FormatEventType(observation.EventType);
    }

    public string? FormatEvent<TState, TEvent>(TransitionObservation<TState, TEvent> observation)
    {
        return _options.EventFormatter?.Invoke(observation.Event) ?? FormatEventType(observation.EventType);
    }

    public static string? FormatEventType(Type? eventType)
    {
        return eventType is null ? null : eventType.FullName ?? eventType.Name;
    }
}