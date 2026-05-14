using System.Diagnostics.Metrics;
using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

internal sealed record CapturedMeasurement(
    string InstrumentName,
    double Value,
    IReadOnlyDictionary<string, object?> Tags);

internal sealed class MetricCapture : IDisposable
{
    private readonly MeterListener _listener = new();

    public MetricCapture(string meterName = StateMachineTelemetryNames.MeterName)
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == meterName) listener.EnableMeasurementEvents(instrument);
        };
        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            Measurements.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            Measurements.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        _listener.Start();
    }

    public List<CapturedMeasurement> Measurements { get; } = [];

    public void Dispose()
    {
        _listener.Dispose();
    }

    private static IReadOnlyDictionary<string, object?> ToDictionary(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dictionary = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var tag in tags) dictionary[tag.Key] = tag.Value;
        return dictionary;
    }
}