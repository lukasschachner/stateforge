using System.Diagnostics;
using System.Diagnostics.Metrics;
using StateForge.Core.Definitions;
using StateForge.OpenTelemetry;

var activities = new List<Activity>();
using var activityListener = new ActivityListener();
activityListener.ShouldListenTo = source => source.Name == StateMachineTelemetryNames.ActivitySourceName;
activityListener.Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded;
activityListener.ActivityStopped = activity => activities.Add(activity);
ActivitySource.AddActivityListener(activityListener);

using var meterListener = new MeterListener();
meterListener.InstrumentPublished = (instrument, listener) =>
{
    if (instrument.Meter.Name == StateMachineTelemetryNames.MeterName) listener.EnableMeasurementEvents(instrument);
};
meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
    Console.WriteLine($"Metric {instrument.Name}={value}"));
meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
    Console.WriteLine($"Metric {instrument.Name}={value:0.###}"));
meterListener.Start();

var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Draft)
        .On<Submit>().GoTo(OrderState.Submitted)
        .On<Reject>().When(_ => false, "demo denial").GoTo(OrderState.Submitted);
    builder.State(OrderState.Submitted);
});

using var observer = new StateMachineTelemetryObserver<OrderState, OrderEvent>();
await definition.ApplyAsync(OrderState.Draft, new Submit(), observer: observer);
await definition.ApplyAsync(OrderState.Draft, new Reject(), observer: observer);

foreach (var activity in activities)
{
    Console.WriteLine($"Activity {activity.OperationName} status={activity.Status}");
    foreach (var tag in activity.Tags) Console.WriteLine($"  {tag.Key}={tag.Value}");
}

Console.WriteLine("OpenTelemetry instrumentation sample completed");

internal enum OrderState
{
    Draft,
    Submitted
}

internal abstract record OrderEvent;

internal sealed record Submit : OrderEvent;

internal sealed record Reject : OrderEvent;