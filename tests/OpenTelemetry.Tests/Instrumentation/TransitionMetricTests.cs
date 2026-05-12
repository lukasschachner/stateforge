using StateMachineLibrary.OpenTelemetry;

namespace OpenTelemetry.Tests.Instrumentation;

public class TransitionMetricTests
{
    [Fact]
    public async Task FinalOutcomeRecordsAttemptAndDurationMetrics()
    {
        using var metrics = new MetricCapture();
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>();

        await TelemetryTestDomain.Create().ApplyAsync(TelemetryState.Draft, new Submit(), observer: observer);
        metrics.Dispose();

        Assert.Contains(metrics.Measurements,
            m => m.InstrumentName == StateMachineTelemetryNames.TransitionAttemptsInstrumentName && m.Value == 1);
        Assert.Contains(metrics.Measurements,
            m => m.InstrumentName == StateMachineTelemetryNames.TransitionDurationInstrumentName && m.Value >= 0);
        var attempt = metrics.Measurements.First(m =>
            m.InstrumentName == StateMachineTelemetryNames.TransitionAttemptsInstrumentName);
        Assert.Equal("Success", attempt.Tags[StateMachineTelemetryNames.OutcomeAttribute]);
    }
}