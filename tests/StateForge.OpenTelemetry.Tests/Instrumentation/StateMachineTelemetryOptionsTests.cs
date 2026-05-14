using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public class StateMachineTelemetryOptionsTests
{
    [Fact]
    public async Task CustomSourceAndFormattersAreUsedForActivityTags()
    {
        const string sourceName = "custom.source";
        using var capture = new ActivityCapture(sourceName);
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>(
            new StateMachineTelemetryOptions
            {
                ActivitySourceName = sourceName,
                MeterName = "custom.meter",
                StateFormatter = state => $"state:{state}",
                EventFormatter = evt => $"event:{evt?.GetType().Name}"
            });

        await TelemetryTestDomain.Create().ApplyAsync(TelemetryState.Draft, new Submit(), observer: observer);

        var activity = Assert.Single(capture.Activities);
        Assert.Equal("state:Draft", activity.GetTagItem(StateMachineTelemetryNames.SourceStateAttribute));
        Assert.Equal("event:Submit", activity.GetTagItem(StateMachineTelemetryNames.EventAttribute));
    }
}