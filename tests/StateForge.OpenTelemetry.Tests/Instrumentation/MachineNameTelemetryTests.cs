using StateForge.Core.Definitions;
using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public class MachineNameTelemetryTests
{
    [Fact]
    public async Task ActivityAndMetricsIncludeMachineNameAttributeWhenConfigured()
    {
        var definition = StateMachineDefinition<TelemetryState, TelemetryEvent>.Create(builder =>
        {
            builder.WithMetadata(StateMachineMetadataKeys.Name, "orders");
            builder.State(TelemetryState.Draft).On<Submit>().GoTo(TelemetryState.Submitted);
            builder.State(TelemetryState.Submitted);
        });
        using var activities = new ActivityCapture();
        using var metrics = new MetricCapture();
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>();

        await definition.ApplyAsync(TelemetryState.Draft, new Submit(), observer: observer);
        metrics.Dispose();

        Assert.Equal("orders",
            Assert.Single(activities.Activities).GetTagItem(StateMachineTelemetryNames.MachineNameAttribute));
        Assert.Contains(metrics.Measurements, measurement =>
            measurement.InstrumentName == StateMachineTelemetryNames.TransitionAttemptsInstrumentName &&
            measurement.Tags.TryGetValue(StateMachineTelemetryNames.MachineNameAttribute, out var value) &&
            Equals("orders", value));
    }
}