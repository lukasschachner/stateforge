using System.Diagnostics;
using StateMachineLibrary.OpenTelemetry;

namespace OpenTelemetry.Tests.Instrumentation;

internal sealed class ActivityCapture : IDisposable
{
    private readonly ActivityListener _listener;

    public ActivityCapture(string sourceName = StateMachineTelemetryNames.ActivitySourceName)
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => Activities.Add(activity)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public List<Activity> Activities { get; } = [];

    public void Dispose()
    {
        _listener.Dispose();
    }
}