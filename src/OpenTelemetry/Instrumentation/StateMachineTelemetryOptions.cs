namespace StateMachineLibrary.OpenTelemetry;

/// <summary>Options for the transition telemetry observer.</summary>
/// <remarks>
///     The adapter only emits activities and metrics. Applications remain responsible for registering sources/meters,
///     choosing exporters, and integrating with hosting or dependency injection if desired.
/// </remarks>
public sealed class StateMachineTelemetryOptions
{
    public string ActivitySourceName { get; init; } = StateMachineTelemetryNames.ActivitySourceName;
    public string MeterName { get; init; } = StateMachineTelemetryNames.MeterName;
    public string? InstrumentationVersion { get; init; }
    public Func<object?, string?>? StateFormatter { get; init; }
    public Func<object?, string?>? EventFormatter { get; init; }
    public bool RecordExceptionDetails { get; init; }
}