using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using StateForge.Core.Execution;

namespace StateForge.OpenTelemetry;

/// <summary>
///     Converts Core transition observations into OpenTelemetry-compatible activities and metrics.
/// </summary>
/// <remarks>
///     This type does not configure exporters, hosted services, dependency injection, logging, or application startup.
///     Consumers own telemetry pipeline registration for <see cref="StateMachineTelemetryNames.ActivitySourceName" /> and
///     <see cref="StateMachineTelemetryNames.MeterName" /> (or configured custom names).
/// </remarks>
public sealed class StateMachineTelemetryObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>, IDisposable
{
    private readonly ConcurrentDictionary<Guid, Activity?> _activities = new();
    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _attempts;
    private readonly Histogram<double> _duration;
    private readonly TelemetryAttributeFormatter _formatter;
    private readonly Meter _meter;
    private readonly StateMachineTelemetryOptions _options;

    public StateMachineTelemetryObserver()
        : this(null)
    {
    }

    public StateMachineTelemetryObserver(StateMachineTelemetryOptions? options)
    {
        _options = options ?? new StateMachineTelemetryOptions();
        _formatter = new TelemetryAttributeFormatter(_options);
        _activitySource = new ActivitySource(_options.ActivitySourceName, _options.InstrumentationVersion);
        _meter = new Meter(_options.MeterName, _options.InstrumentationVersion);
        _attempts = _meter.CreateCounter<long>(StateMachineTelemetryNames.TransitionAttemptsInstrumentName, "{attempt}",
            "State machine transition attempts.");
        _duration = _meter.CreateHistogram<double>(StateMachineTelemetryNames.TransitionDurationInstrumentName, "ms",
            "Observed state machine transition duration.");
    }

    public void Dispose()
    {
        foreach (var activity in _activities.Values) activity?.Dispose();

        _activities.Clear();
        _activitySource.Dispose();
        _meter.Dispose();
    }

    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation,
        CancellationToken cancellationToken = default)
    {
        switch (observation.Kind)
        {
            case TransitionObservationKind.Started:
                StartActivity(observation);
                break;
            case TransitionObservationKind.Outcome:
                RecordOutcome(observation);
                break;
            default:
                AnnotateActivity(observation);
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void StartActivity(TransitionObservation<TState, TEvent> observation)
    {
        var activity = _activitySource.StartActivity(StateMachineTelemetryNames.TransitionActivityName);
        if (activity is not null) ApplyCommonTags(activity, observation);

        _activities[observation.AttemptId] = activity;
    }

    private void AnnotateActivity(TransitionObservation<TState, TEvent> observation)
    {
        if (!_activities.TryGetValue(observation.AttemptId, out var activity) || activity is null) return;

        ApplyCommonTags(activity, observation);
        activity.AddEvent(new ActivityEvent(observation.Kind.ToString(), tags: new ActivityTagsCollection
        {
            [StateMachineTelemetryNames.LifecyclePhaseAttribute] = observation.Phase.ToString(),
            [StateMachineTelemetryNames.CommittedAttribute] = observation.Committed
        }));
        ApplyDiagnostics(activity, observation);
    }

    private void RecordOutcome(TransitionObservation<TState, TEvent> observation)
    {
        if (_activities.TryRemove(observation.AttemptId, out var activity) && activity is not null)
        {
            ApplyCommonTags(activity, observation);
            ApplyDiagnostics(activity, observation);
            if (observation.OutcomeCategory is TransitionOutcomeCategory.Success)
                activity.SetStatus(ActivityStatusCode.Ok);
            else
                activity.SetStatus(ActivityStatusCode.Error, observation.Diagnostics.Summary);

            activity.Stop();
        }

        var tags = CreateTags(observation);
        _attempts.Add(1, tags);
        if (observation.Elapsed is { } elapsed) _duration.Record(elapsed.TotalMilliseconds, tags);
    }

    private void ApplyCommonTags(Activity activity, TransitionObservation<TState, TEvent> observation)
    {
        SetIfNotNull(activity, StateMachineTelemetryNames.AttemptIdAttribute, observation.AttemptId.ToString("D"));
        SetIfNotNull(activity, StateMachineTelemetryNames.MachineNameAttribute, observation.MachineName);
        SetIfNotNull(activity, StateMachineTelemetryNames.SourceStateAttribute,
            _formatter.FormatState(observation.SourceState));
        SetIfNotNull(activity, StateMachineTelemetryNames.TargetStateAttribute,
            _formatter.FormatState(observation.TargetState));
        SetIfNotNull(activity, StateMachineTelemetryNames.ResultingStateAttribute,
            _formatter.FormatState(observation.ResultingState));
        SetIfNotNull(activity, StateMachineTelemetryNames.EventTypeAttribute,
            TelemetryAttributeFormatter.FormatEventType(observation.EventType));
        SetIfNotNull(activity, StateMachineTelemetryNames.EventAttribute,
            _options.EventFormatter?.Invoke(observation.Event));
        SetIfNotNull(activity, StateMachineTelemetryNames.TransitionKindAttribute,
            observation.TransitionKind?.ToString());
        SetIfNotNull(activity, StateMachineTelemetryNames.LifecyclePhaseAttribute, observation.Phase.ToString());
        SetIfNotNull(activity, StateMachineTelemetryNames.OutcomeAttribute, observation.OutcomeCategory?.ToString());
        activity.SetTag(StateMachineTelemetryNames.CommittedAttribute, observation.Committed);
    }

    private void ApplyDiagnostics(Activity activity, TransitionObservation<TState, TEvent> observation)
    {
        if (!_options.RecordExceptionDetails || observation.Diagnostics.Exception is null) return;

        var exception = observation.Diagnostics.Exception;
        SetIfNotNull(activity, StateMachineTelemetryNames.ErrorTypeAttribute,
            exception.GetType().FullName ?? exception.GetType().Name);
        SetIfNotNull(activity, StateMachineTelemetryNames.ErrorMessageAttribute, exception.Message);
    }

    private TagList CreateTags(TransitionObservation<TState, TEvent> observation)
    {
        var tags = new TagList
        {
            { StateMachineTelemetryNames.CommittedAttribute, observation.Committed }
        };
        AddIfNotNull(ref tags, StateMachineTelemetryNames.MachineNameAttribute, observation.MachineName);
        AddIfNotNull(ref tags, StateMachineTelemetryNames.SourceStateAttribute,
            _formatter.FormatState(observation.SourceState));
        AddIfNotNull(ref tags, StateMachineTelemetryNames.TargetStateAttribute,
            _formatter.FormatState(observation.TargetState));
        AddIfNotNull(ref tags, StateMachineTelemetryNames.ResultingStateAttribute,
            _formatter.FormatState(observation.ResultingState));
        AddIfNotNull(ref tags, StateMachineTelemetryNames.EventTypeAttribute,
            TelemetryAttributeFormatter.FormatEventType(observation.EventType));
        AddIfNotNull(ref tags, StateMachineTelemetryNames.TransitionKindAttribute,
            observation.TransitionKind?.ToString());
        AddIfNotNull(ref tags, StateMachineTelemetryNames.OutcomeAttribute, observation.OutcomeCategory?.ToString());
        return tags;
    }

    private static void SetIfNotNull(Activity activity, string key, string? value)
    {
        if (value is not null) activity.SetTag(key, value);
    }

    private static void AddIfNotNull(ref TagList tags, string key, string? value)
    {
        if (value is not null) tags.Add(key, value);
    }
}